using System.Data;
using BE.Entity;
using BE.Mapper;
using Microsoft.Data.SqlClient;

namespace DAL
{
    public class UserDAL : IUserDAL
    {
        private readonly DatabaseHelper dbHelper;
        private readonly UserMapper mapper;

        public UserDAL()
        {
            dbHelper = new DatabaseHelper();
            mapper = new UserMapper();
        }

        /// <summary>
        /// Busca por nombre de usuario solamente. La contraseña no se manda al SQL:
        /// la verifica la BLL contra el hash con Services.HashManager.
        /// </summary>
        public User? GetByUsername(string username)
        {
            const string query = "SELECT * FROM [User] WHERE Username = @Username";
            SqlParameter[] parameters =
            [
                new SqlParameter("@Username", username)
            ];

            DataSet ds = dbHelper.ExecuteDataSet(query, CommandType.Text, parameters);
            return ds.Tables[0].Rows.Count > 0
                ? mapper.MapToEntity(ds.Tables[0].Rows[0])
                : null;
        }

        public List<User> GetAll()
        {
            const string query = "SELECT * FROM [User]";
            DataSet ds = dbHelper.ExecuteDataSet(query, CommandType.Text, []);
            return mapper.MapAll(ds.Tables[0]).ToList();
        }

        public bool Block(string username)
        {
            const string query = "UPDATE [User] SET IsBlocked = @IsBlocked, UpdatedAt = SYSDATETIME() WHERE Username = @Username";
            SqlParameter[] parameters =
            [
                new SqlParameter("@IsBlocked", true),
                new SqlParameter("@Username", username)
            ];

            return dbHelper.ExecuteNonQuery(query, CommandType.Text, parameters) > 0;
        }

        public void IncrementFailedAttempts(string username)
        {
            const string query = "UPDATE [User] SET FailedAttempts = FailedAttempts + 1, UpdatedAt = SYSDATETIME() WHERE Username = @Username";
            SqlParameter[] parameters =
            [
                new SqlParameter("@Username", username)
            ];

            dbHelper.ExecuteNonQuery(query, CommandType.Text, parameters);
        }

        public void ResetFailedAttempts(Guid id)
        {
            const string query = "UPDATE [User] SET FailedAttempts = 0, LastLoginAt = SYSDATETIME(), UpdatedAt = SYSDATETIME() WHERE Id = @Id";
            SqlParameter[] parameters =
            [
                new SqlParameter("@Id", id)
            ];

            dbHelper.ExecuteNonQuery(query, CommandType.Text, parameters);
        }
    }
}
