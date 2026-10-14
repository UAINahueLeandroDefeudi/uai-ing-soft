using System.Configuration;
using System.Data;
using Microsoft.Data.SqlClient;

namespace DAL
{
    public class BackupDAL : IBackupDAL
    {
        private readonly DatabaseHelper dbHelper = new();

        public string NombreBase => dbHelper.DatabaseName;

        public string CarpetaBackups
        {
            get
            {
                var configurada = ConfigurationManager.AppSettings["BackupPath"];
                if (!string.IsNullOrWhiteSpace(configurada)) return configurada;

                // Carpeta de backups por defecto de la instancia (la única donde SQL Server seguro puede escribir).
                const string query = "SELECT CAST(SERVERPROPERTY('InstanceDefaultBackupPath') AS NVARCHAR(500))";
                return dbHelper.ExecuteScalar(query, CommandType.Text, []) as string
                    ?? throw new ConfigurationErrorsException("Configure 'BackupPath' en App.config");
            }
        }

        public void Backup(string rutaArchivo)
        {
            // La ruta la arma la BLL (nombre de la base + fecha); se escapan las comillas por las dudas.
            var ruta = rutaArchivo.Replace("'", "''");
            dbHelper.ExecuteOnMaster(
                [$"BACKUP DATABASE [{NombreBase}] TO DISK = N'{ruta}' WITH INIT"]);
        }

        public void Restaurar(string rutaArchivo)
        {
            var ruta = rutaArchivo.Replace("'", "''");
            var db = NombreBase;

            dbHelper.ExecuteOnMaster(
            [
                $"ALTER DATABASE [{db}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE",
                $"RESTORE DATABASE [{db}] FROM DISK = N'{ruta}' WITH REPLACE",
                $"ALTER DATABASE [{db}] SET MULTI_USER"
            ]);

            // Las conexiones del pool apuntaban a la base anterior a la restauración.
            SqlConnection.ClearAllPools();
        }

        public T ConBackupMontado<T>(string rutaArchivo, Func<InfoBackup, IDigitoVerificadorDAL, T> accion)
        {
            var ruta = rutaArchivo.Replace("'", "''");
            var temporal = $"{NombreBase}_verif";

            dbHelper.ExecuteOnMaster([$"RESTORE VERIFYONLY FROM DISK = N'{ruta}'"]);

            var cabecera = dbHelper.QueryOnMaster($"RESTORE HEADERONLY FROM DISK = N'{ruta}'").Rows[0];
            var info = new InfoBackup(
                Convert.ToString(cabecera["DatabaseName"]) ?? string.Empty,
                Convert.ToDateTime(cabecera["BackupFinishDate"]));

            // Una base de otra instancia no se monta: la BLL la rechaza sin restaurar nada.
            if (!string.Equals(info.BaseOrigen, NombreBase, StringComparison.OrdinalIgnoreCase))
                return accion(info, new DigitoVerificadorDAL(NombreBase));

            var rutaDatos = dbHelper.ExecuteScalar(
                "SELECT CAST(SERVERPROPERTY('InstanceDefaultDataPath') AS NVARCHAR(500))", CommandType.Text, []) as string
                ?? throw new ConfigurationErrorsException("No se pudo determinar la carpeta de datos de SQL Server");

            var move = dbHelper.QueryOnMaster($"RESTORE FILELISTONLY FROM DISK = N'{ruta}'").Rows.Cast<DataRow>()
                .Select(r => $"MOVE N'{Convert.ToString(r["LogicalName"])!.Replace("'", "''")}' TO N'{Path.Combine(rutaDatos,
                    temporal + "_" + Convert.ToString(r["LogicalName"]) + (Convert.ToString(r["Type"]) == "L" ? ".ldf" : ".mdf")).Replace("'", "''")}'");

            try
            {
                Descartar(temporal);
                dbHelper.ExecuteOnMaster([$"RESTORE DATABASE [{temporal}] FROM DISK = N'{ruta}' WITH {string.Join(", ", move)}, REPLACE"]);

                return accion(info, new DigitoVerificadorDAL(temporal));
            }
            finally
            {
                SqlConnection.ClearAllPools();
                Descartar(temporal);
            }
        }

        private void Descartar(string baseTemporal)
            => dbHelper.ExecuteOnMaster(
            [
                $"IF DB_ID(N'{baseTemporal}') IS NOT NULL BEGIN ALTER DATABASE [{baseTemporal}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [{baseTemporal}]; END"
            ]);
    }
}
