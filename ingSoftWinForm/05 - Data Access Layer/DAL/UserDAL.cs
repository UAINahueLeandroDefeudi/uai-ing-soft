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
        private readonly DigitoVerificadorDAL integridad = new();

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

        public bool EmailExists(string email)
        {
            const string query = "SELECT COUNT(1) FROM [User] WHERE Email = @Email";
            SqlParameter[] parameters = [new SqlParameter("@Email", email)];

            return Convert.ToInt32(dbHelper.ExecuteScalar(query, CommandType.Text, parameters)) > 0;
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

        public void Insert(User user, string roleName)
        {
            const string insertUser =
                "INSERT INTO [User] (Id, Username, PasswordHash, Salt, FirstName, LastName, Email, CreatedBy) " +
                "VALUES (@Id, @Username, @PasswordHash, @Salt, @FirstName, @LastName, @Email, @CreatedBy)";
            SqlParameter[] userParameters =
            [
                new SqlParameter("@Id", user.Id),
                new SqlParameter("@Username", user.Username),
                new SqlParameter("@PasswordHash", user.PasswordHash),
                new SqlParameter("@Salt", user.Salt),
                new SqlParameter("@FirstName", user.FirstName),
                new SqlParameter("@LastName", user.LastName),
                new SqlParameter("@Email", (object?)user.Email ?? DBNull.Value),
                new SqlParameter("@CreatedBy", (object?)user.CreatedBy ?? DBNull.Value)
            ];

            // Falla (y la transacción hace rollback) si el rol no existe: no queda un usuario sin rol.
            const string insertRole =
                "INSERT INTO [User_Permission] (UserId, PermissionId) SELECT @UserId, Id FROM [Permission] WHERE Name = @RoleName AND IsRol = 1; " +
                "IF @@ROWCOUNT = 0 THROW 50001, 'El rol indicado no existe', 1;";
            SqlParameter[] roleParameters =
            [
                new SqlParameter("@UserId", user.Id),
                new SqlParameter("@RoleName", roleName)
            ];

            // T07: el rol asignado entra a [User_Permission], tabla verificable.
            integridad.Escribir((connection, transaction) =>
            {
                foreach (var (query, parameters) in new[] { (insertUser, userParameters), (insertRole, roleParameters) })
                {
                    using var command = new SqlCommand(query, connection, transaction);
                    command.Parameters.AddRange(parameters);
                    command.ExecuteNonQuery();
                }

                using var select = new SqlCommand(
                    "SELECT PermissionId FROM [User_Permission] WHERE UserId = @UserId", connection, transaction);
                select.Parameters.AddWithValue("@UserId", user.Id);

                var claves = new List<(EntidadVerificable, Dictionary<string, object?>?)>();
                using var reader = select.ExecuteReader();
                while (reader.Read())
                    claves.Add((EntidadesVerificables.UserPermission,
                        new Dictionary<string, object?> { ["UserId"] = user.Id, ["PermissionId"] = reader.GetInt32(0) }));

                return claves;
            });
        }
    }
}
