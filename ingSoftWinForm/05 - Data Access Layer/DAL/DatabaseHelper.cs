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

        public DatabaseHelper()
        {
            var setting = ConfigurationManager.ConnectionStrings["IF_DB"];
            if (setting == null)
                throw new ConfigurationErrorsException(
                    "No se encontró la cadena de conexión 'IF_DB' en App.config");

            connectionString = setting.ConnectionString;
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
    }
}
