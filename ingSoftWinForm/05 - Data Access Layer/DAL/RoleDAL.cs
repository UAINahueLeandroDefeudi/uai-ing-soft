using System.Data;
using BE.Entity;
using BE.Mapper;
using Microsoft.Data.SqlClient;

namespace DAL
{
    public class RoleDAL : IRoleDAL
    {
        private readonly DatabaseHelper dbHelper;
        private readonly RoleMapper mapper;
        private readonly IPermissionDAL permissionDAL;

        public RoleDAL() : this(new PermissionDAL()) { }

        public RoleDAL(IPermissionDAL permissionDAL)
        {
            dbHelper = new DatabaseHelper();
            mapper = new RoleMapper();
            this.permissionDAL = permissionDAL;
        }

        public List<Role> GetAll()
        {
            const string query = "SELECT * FROM [Role] ORDER BY Id";
            return Load(query, []);
        }

        public List<Role> GetByUser(Guid userId)
        {
            const string query =
                "SELECT r.* FROM [Role] r INNER JOIN [User_Role] ur ON ur.RoleId = r.Id WHERE ur.UserId = @UserId ORDER BY r.Id";
            SqlParameter[] parameters = [new SqlParameter("@UserId", userId)];

            return Load(query, parameters);
        }

        public Role? GetByName(string name)
        {
            const string query = "SELECT * FROM [Role] WHERE Name = @Name";
            SqlParameter[] parameters = [new SqlParameter("@Name", name)];

            return Load(query, parameters).FirstOrDefault();
        }

        public int Insert(Role role, string createdBy)
        {
            const string query =
                "INSERT INTO [Role] (Name, CreatedBy) VALUES (@Name, @CreatedBy); SELECT CAST(SCOPE_IDENTITY() AS INT);";
            SqlParameter[] parameters =
            [
                new SqlParameter("@Name", role.Name),
                new SqlParameter("@CreatedBy", createdBy)
            ];

            return Convert.ToInt32(dbHelper.ExecuteScalar(query, CommandType.Text, parameters));
        }

        public void Delete(int roleId)
        {
            // Role_Permission se va por ON DELETE CASCADE; User_Role no (la BLL lo valida antes).
            const string query = "DELETE FROM [Role] WHERE Id = @Id";
            SqlParameter[] parameters = [new SqlParameter("@Id", roleId)];

            dbHelper.ExecuteNonQuery(query, CommandType.Text, parameters);
        }

        public bool HasUsers(int roleId)
        {
            const string query = "SELECT COUNT(1) FROM [User_Role] WHERE RoleId = @RoleId";
            SqlParameter[] parameters = [new SqlParameter("@RoleId", roleId)];

            return Convert.ToInt32(dbHelper.ExecuteScalar(query, CommandType.Text, parameters)) > 0;
        }

        public void AddPermission(int roleId, int permissionId)
        {
            const string query =
                "IF NOT EXISTS (SELECT 1 FROM [Role_Permission] WHERE RoleId = @RoleId AND PermissionId = @PermissionId) " +
                "INSERT INTO [Role_Permission] (RoleId, PermissionId) VALUES (@RoleId, @PermissionId)";
            SqlParameter[] parameters =
            [
                new SqlParameter("@RoleId", roleId),
                new SqlParameter("@PermissionId", permissionId)
            ];

            dbHelper.ExecuteNonQuery(query, CommandType.Text, parameters);
        }

        public void RemovePermission(int roleId, int permissionId)
        {
            const string query = "DELETE FROM [Role_Permission] WHERE RoleId = @RoleId AND PermissionId = @PermissionId";
            SqlParameter[] parameters =
            [
                new SqlParameter("@RoleId", roleId),
                new SqlParameter("@PermissionId", permissionId)
            ];

            dbHelper.ExecuteNonQuery(query, CommandType.Text, parameters);
        }

        public void AssignToUser(Guid userId, int roleId)
        {
            const string query =
                "IF NOT EXISTS (SELECT 1 FROM [User_Role] WHERE UserId = @UserId AND RoleId = @RoleId) " +
                "INSERT INTO [User_Role] (UserId, RoleId) VALUES (@UserId, @RoleId)";
            SqlParameter[] parameters =
            [
                new SqlParameter("@UserId", userId),
                new SqlParameter("@RoleId", roleId)
            ];

            dbHelper.ExecuteNonQuery(query, CommandType.Text, parameters);
        }

        public void RemoveFromUser(Guid userId, int roleId)
        {
            const string query = "DELETE FROM [User_Role] WHERE UserId = @UserId AND RoleId = @RoleId";
            SqlParameter[] parameters =
            [
                new SqlParameter("@UserId", userId),
                new SqlParameter("@RoleId", roleId)
            ];

            dbHelper.ExecuteNonQuery(query, CommandType.Text, parameters);
        }

        /// <summary>Ejecuta la consulta de roles y les carga sus permisos desde el catálogo.</summary>
        private List<Role> Load(string query, SqlParameter[] parameters)
        {
            var roles = mapper.MapAll(dbHelper.ExecuteDataSet(query, CommandType.Text, parameters).Tables[0]).ToList();
            if (roles.Count == 0) return roles;

            var catalog = permissionDAL.GetAll();
            const string queryRolePermissions = "SELECT RoleId, PermissionId FROM [Role_Permission]";
            var rolePermissions = dbHelper.ExecuteDataSet(queryRolePermissions, CommandType.Text, []).Tables[0];

            foreach (var role in roles)
            {
                foreach (DataRow row in rolePermissions.Select($"RoleId = {role.Id}"))
                {
                    var permission = catalog.FirstOrDefault(p => p.Id == (int)row["PermissionId"]);
                    if (permission != null) role.AddPermission(permission);
                }
            }

            return roles;
        }
    }
}
