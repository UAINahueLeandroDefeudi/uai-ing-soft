using System.Configuration;
using System.Data;
using Microsoft.Data.SqlClient;

namespace DAL
{
    /// <summary>
    /// Único punto del sistema que abre conexiones contra la base.
    /// </summary>
    public class DatabaseHelper
    {
        private readonly string connectionString;

        /// <param name="catalogo">Base distinta de la configurada (p. ej. la temporal con que se valida un backup).</param>
        public DatabaseHelper(string? catalogo = null)
        {
            var setting = ConfigurationManager.ConnectionStrings["IF_DB"];
            if (setting == null)
                throw new ConfigurationErrorsException(
                    "No se encontró la cadena de conexión 'IF_DB' en App.config");

            connectionString = catalogo == null
                ? setting.ConnectionString
                : new SqlConnectionStringBuilder(setting.ConnectionString) { InitialCatalog = catalogo }.ConnectionString;
        }

        public DataSet ExecuteDataSet(string query, CommandType commandType, SqlParameter[] parameters)
        {
            using var connection = new SqlConnection(connectionString);
            using var command = new SqlCommand(query, connection) { CommandType = commandType };
            command.Parameters.AddRange(parameters);

            using var adapter = new SqlDataAdapter(command);
            var ds = new DataSet();
            adapter.Fill(ds);
            return ds;
        }

        public int ExecuteNonQuery(string query, CommandType commandType, SqlParameter[] parameters)
        {
            using var connection = new SqlConnection(connectionString);
            using var command = new SqlCommand(query, connection) { CommandType = commandType };
            command.Parameters.AddRange(parameters);

            connection.Open();
            return command.ExecuteNonQuery();
        }

        /// <summary>
        /// Para los INSERT que necesitan recuperar el identity recién generado
        /// (INSERT ... ; SELECT SCOPE_IDENTITY()).
        /// </summary>
        public object? ExecuteScalar(string query, CommandType commandType, SqlParameter[] parameters)
        {
            using var connection = new SqlConnection(connectionString);
            using var command = new SqlCommand(query, connection) { CommandType = commandType };
            command.Parameters.AddRange(parameters);

            connection.Open();
            return command.ExecuteScalar();
        }

        /// <summary>
        /// Ejecuta varias sentencias (query + parámetros) en una única transacción:
        /// o se aplican todas o ninguna. Para operaciones que tocan más de una tabla
        /// (alta de usuario + rol, alta de rol + permisos).
        /// </summary>
        /// <summary>
        /// Transacción con lógica propia (leer y escribir en la misma conexión):
        /// commit si <paramref name="work"/> termina, rollback si lanza.
        /// </summary>
        public T ExecuteTransaction<T>(Func<SqlConnection, SqlTransaction, T> work)
        {
            using var connection = new SqlConnection(connectionString);
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                var result = work(connection, transaction);
                transaction.Commit();
                return result;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        /// <summary>Nombre de la base configurada (el Database de la cadena IF_DB).</summary>
        public string DatabaseName => new SqlConnectionStringBuilder(connectionString).InitialCatalog;

        /// <summary>
        /// T07: ejecuta sentencias contra [master] (BACKUP/RESTORE no pueden correr dentro
        /// de la base que se restaura). Sin timeout: un restore puede tardar.
        /// </summary>
        public void ExecuteOnMaster(IEnumerable<string> statements)
        {
            var builder = new SqlConnectionStringBuilder(connectionString) { InitialCatalog = "master", Pooling = false };

            using var connection = new SqlConnection(builder.ConnectionString);
            connection.Open();

            foreach (var statement in statements)
            {
                using var command = new SqlCommand(statement, connection) { CommandTimeout = 0 };
                command.ExecuteNonQuery();
            }
        }

        /// <summary>T07: consulta contra [master] (RESTORE HEADERONLY / FILELISTONLY devuelven filas).</summary>
        public DataTable QueryOnMaster(string query)
        {
            var builder = new SqlConnectionStringBuilder(connectionString) { InitialCatalog = "master", Pooling = false };

            using var connection = new SqlConnection(builder.ConnectionString);
            using var command = new SqlCommand(query, connection) { CommandTimeout = 0 };
            using var adapter = new SqlDataAdapter(command);

            var table = new DataTable();
            adapter.Fill(table);
            return table;
        }

        public void ExecuteTransaction(IEnumerable<(string Query, SqlParameter[] Parameters)> statements)
        {
            using var connection = new SqlConnection(connectionString);
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                foreach (var (query, parameters) in statements)
                {
                    using var command = new SqlCommand(query, connection, transaction);
                    command.Parameters.AddRange(parameters);
                    command.ExecuteNonQuery();
                }

                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }
    }
}
