namespace DAL
{
    /// <summary>Datos de cabecera de un .bkp (RESTORE HEADERONLY).</summary>
    public record InfoBackup(string BaseOrigen, DateTime FechaBackup);

    /// <summary>T07: backup y restauración de la base completa (.bkp).</summary>
    public interface IBackupDAL
    {
        /// <summary>Nombre de la base (para armar &lt;NombreBD&gt;_yyyyMMdd_HHmmss.bkp).</summary>
        string NombreBase { get; }

        /// <summary>Carpeta de los .bkp (App.config "BackupPath" o la de backups por defecto de SQL Server).</summary>
        string CarpetaBackups { get; }

        void Backup(string rutaArchivo);

        void Restaurar(string rutaArchivo);

        /// <summary>
        /// Monta el backup en una base temporal, ejecuta <paramref name="accion"/> con su cabecera y un lector
        /// de sus tablas, y descarta la temporal. La base real no se toca. Lanza si el archivo está dañado.
        /// </summary>
        T ConBackupMontado<T>(string rutaArchivo, Func<InfoBackup, IDigitoVerificadorDAL, T> accion);
    }
}
