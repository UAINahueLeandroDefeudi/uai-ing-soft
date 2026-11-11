using System.Data;
using BE.Entity;
using BE.Enum;
using BE.Mapper;
using Microsoft.Data.SqlClient;

namespace DAL
{
    public class UserDAL : IUserDAL
    {
        private readonly DatabaseHelper dbHelper;
        private readonly UserMapper mapper;
        private readonly DigitoVerificadorDAL integridad = new();
        private readonly UserHistoryMapper historyMapper = new();

        /// <summary>Copia los seis campos versionados de la fila actual del usuario a [User_History].</summary>
        private const string InsertHistory = @"
            INSERT INTO [User_History]
                ([UserId], [Version], [NickName], [FirstName], [LastName], [Pais], [Provincia], [Direccion],
                 [TipoCambio], [RestauradaDesde], [EventBy])
            SELECT [Id], [Version], [NickName], [FirstName], [LastName], [Pais], [Provincia], [Direccion],
                   @TipoCambio, @RestauradaDesde, @EventBy
            FROM [User]
            WHERE [Id] = @Id;
            SELECT CAST(SCOPE_IDENTITY() AS BIGINT);";

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
            const string query = "SELECT * FROM [User] ORDER BY [Username]";
            DataSet ds = dbHelper.ExecuteDataSet(query, CommandType.Text, []);
            return mapper.MapAll(ds.Tables[0]).ToList();
        }

        public User? GetById(Guid id)
        {
            const string query = "SELECT * FROM [User] WHERE [Id] = @Id";
            DataSet ds = dbHelper.ExecuteDataSet(query, CommandType.Text, [new SqlParameter("@Id", id)]);
            return mapper.MapToEntity(ds.Tables[0]);
        }

        public List<UserHistory> GetHistory(Guid userId)
        {
            // LEFT JOIN: la versión se muestra aunque el usuario que la generó ya no exista.
            const string query = @"
                SELECT h.*, u.[Username] AS [EventByUsername]
                FROM [User_History] h
                LEFT JOIN [User] u ON u.[Id] = h.[EventBy]
                WHERE h.[UserId] = @UserId
                ORDER BY h.[Version] ASC";

            var ds = dbHelper.ExecuteDataSet(query, CommandType.Text, [new SqlParameter("@UserId", userId)]);
            return historyMapper.MapAll(ds.Tables[0]).ToList();
        }

        public bool Update(User user, TipoCambioUsuario tipo, Guid eventBy, int? restauradaDesde = null)
        {
            // La condición sobre Version evita pisar el cambio de otro usuario hecho después de leer.
            const string query = @"
                UPDATE [User]
                SET [NickName] = @NickName, [FirstName] = @FirstName, [LastName] = @LastName,
                    [Pais] = @Pais, [Provincia] = @Provincia, [Direccion] = @Direccion,
                    [Version] = [Version] + 1,
                    [UpdatedAt] = SYSDATETIME(),
                    [UpdatedBy] = (SELECT [Username] FROM [User] WHERE [Id] = @EventBy)
                OUTPUT inserted.[Version]
                WHERE [Id] = @Id AND [Version] = @Version;";

            var actualizado = false;

            integridad.Escribir((connection, transaction) =>
            {
                using (var command = new SqlCommand(query, connection, transaction))
                {
                    command.Parameters.AddRange(CamposVersionados(user));
                    command.Parameters.Add(new SqlParameter("@Id", user.Id));
                    command.Parameters.Add(new SqlParameter("@Version", user.Version));
                    command.Parameters.Add(new SqlParameter("@EventBy", SqlDbType.UniqueIdentifier) { Value = eventBy });

                    using var reader = command.ExecuteReader();
                    if (!reader.Read()) return [];

                    user.Version = reader.GetInt32(0);
                }

                var historyId = RegistrarHistorial(connection, transaction, user.Id, tipo, restauradaDesde, eventBy);
                actualizado = true;

                return Tocadas(historyId);
            });

            return actualizado;
        }

        private static long RegistrarHistorial(SqlConnection connection, SqlTransaction transaction,
            Guid userId, TipoCambioUsuario tipo, int? restauradaDesde, Guid eventBy)
        {
            using var command = new SqlCommand(InsertHistory, connection, transaction);
            command.Parameters.Add(new SqlParameter("@Id", userId));
            command.Parameters.Add(new SqlParameter("@TipoCambio", tipo.ToString()));
            command.Parameters.Add(new SqlParameter("@RestauradaDesde", SqlDbType.Int)
                { Value = (object?)restauradaDesde ?? DBNull.Value });
            command.Parameters.Add(new SqlParameter("@EventBy", SqlDbType.UniqueIdentifier) { Value = eventBy });

            return Convert.ToInt64(command.ExecuteScalar());
        }

        private static IEnumerable<(EntidadVerificable, Dictionary<string, object?>?)> Tocadas(long historyId)
            =>
            [
                (EntidadesVerificables.UserHistory, new Dictionary<string, object?> { ["Id"] = historyId })
            ];

        private static SqlParameter[] CamposVersionados(User user)
            =>
            [
                new SqlParameter("@NickName", SqlDbType.NVarChar, 50) { Value = (object?)user.NickName ?? DBNull.Value },
                new SqlParameter("@FirstName", SqlDbType.NVarChar, 100) { Value = user.FirstName },
                new SqlParameter("@LastName", SqlDbType.NVarChar, 100) { Value = user.LastName },
                new SqlParameter("@Pais", SqlDbType.NVarChar, 100) { Value = (object?)user.Pais ?? DBNull.Value },
                new SqlParameter("@Provincia", SqlDbType.NVarChar, 100) { Value = (object?)user.Provincia ?? DBNull.Value },
                new SqlParameter("@Direccion", SqlDbType.NVarChar, 200) { Value = (object?)user.Direccion ?? DBNull.Value }
            ];

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
                "INSERT INTO [User] (Id, Username, PasswordHash, Salt, FirstName, LastName, Email, CreatedBy, " +
                "NickName, Pais, Provincia, Direccion, Version) " +
                "VALUES (@Id, @Username, @PasswordHash, @Salt, @FirstName, @LastName, @Email, @CreatedBy, " +
                "@NickName, @Pais, @Provincia, @Direccion, 1)";
            SqlParameter[] userParameters =
            [
                new SqlParameter("@Id", user.Id),
                new SqlParameter("@Username", user.Username),
                new SqlParameter("@PasswordHash", user.PasswordHash),
                new SqlParameter("@Salt", user.Salt),
                new SqlParameter("@FirstName", user.FirstName),
                new SqlParameter("@LastName", user.LastName),
                new SqlParameter("@Email", (object?)user.Email ?? DBNull.Value),
                new SqlParameter("@CreatedBy", (object?)user.CreatedBy ?? DBNull.Value),
                new SqlParameter("@NickName", (object?)user.NickName ?? DBNull.Value),
                new SqlParameter("@Pais", (object?)user.Pais ?? DBNull.Value),
                new SqlParameter("@Provincia", (object?)user.Provincia ?? DBNull.Value),
                new SqlParameter("@Direccion", (object?)user.Direccion ?? DBNull.Value)
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

            // T07: el rol asignado entra a [User_Permission] y la versión 1 a [User_History], tablas verificables.
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
                using (var reader = select.ExecuteReader())
                {
                    while (reader.Read())
                        claves.Add((EntidadesVerificables.UserPermission,
                            new Dictionary<string, object?> { ["UserId"] = user.Id, ["PermissionId"] = reader.GetInt32(0) }));
                }

                // Control de cambios: el alta es la versión 1; el propio usuario es quien la generó.
                var historyId = RegistrarHistorial(connection, transaction, user.Id, TipoCambioUsuario.Alta, null, user.Id);
                claves.AddRange(Tocadas(historyId));

                return claves;
            });
        }
    }
}
