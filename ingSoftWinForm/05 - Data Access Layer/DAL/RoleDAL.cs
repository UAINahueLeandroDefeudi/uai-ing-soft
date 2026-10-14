using System.Data;
using BE.Entity;
using Microsoft.Data.SqlClient;

namespace DAL
{
    /// <summary>
    /// ABM de roles sobre el Composite (Modelo I). Un rol es una fila de [Permission]
    /// con IsFamily = 1 e IsRol = 1. Los permisos simples (hojas) son un catálogo fijo:
    /// ninguna operación de esta clase los crea, modifica ni elimina.
    /// </summary>
    public class RoleDAL : IRoleDAL
    {
        private readonly DatabaseHelper dbHelper;
        private readonly IPermissionDAL permissionDAL;

        public RoleDAL() : this(new PermissionDAL()) { }

        public RoleDAL(IPermissionDAL permissionDAL)
        {
            dbHelper = new DatabaseHelper();
            this.permissionDAL = permissionDAL;
        }

        public List<Permission> GetAll()
            => permissionDAL.GetAll().OfType<RolePermission>().ToList<Permission>();

        public List<Permission> GetByUser(Guid userId)
        {
            const string query = "SELECT PermissionId FROM [User_Permission] WHERE UserId = @UserId";
            SqlParameter[] parameters = [new SqlParameter("@UserId", userId)];

            var ids = dbHelper.ExecuteDataSet(query, CommandType.Text, parameters).Tables[0]
                .Rows.Cast<DataRow>()
                .Select(row => (int)row["PermissionId"])
                .ToHashSet();

            // Mismas instancias del catálogo: el árbol ya viene armado.
            return GetAll().Where(r => ids.Contains(r.Id)).ToList();
        }

        public Permission? GetByName(string name)
            => GetAll().FirstOrDefault(r => string.Equals(r.Name, name, StringComparison.OrdinalIgnoreCase));

        public int Insert(Permission role)
        {
            const string query =
                "INSERT INTO [Permission] (Code, Name, IsFamily, IsRol) VALUES (@Code, @Name, 1, 1); " +
                "SELECT CAST(SCOPE_IDENTITY() AS INT);";
            SqlParameter[] parameters =
            [
                new SqlParameter("@Code", role.Code),
                new SqlParameter("@Name", role.Name)
            ];

            return Convert.ToInt32(dbHelper.ExecuteScalar(query, CommandType.Text, parameters));
        }

        public void Delete(int roleId)
        {
            // La condición IsRol = 1 impide borrar una hoja o una familia que no es rol.
            // Primero se sueltan sus relaciones (como padre y como hijo en otro rol).
            const string deleteRelations =
                "DELETE FROM [Permission_Permission] WHERE ParentId = @Id OR ChildId = @Id";
            const string deleteRole = "DELETE FROM [Permission] WHERE Id = @Id AND IsRol = 1";
            SqlParameter[] relationParameters = [new SqlParameter("@Id", roleId)];
            SqlParameter[] roleParameters = [new SqlParameter("@Id", roleId)];

            // Si el rol no es rol, la primera sentencia no debe haber borrado nada: se valida antes.
            const string guard =
                "IF NOT EXISTS (SELECT 1 FROM [Permission] WHERE Id = @Id AND IsRol = 1) " +
                "THROW 50002, 'El permiso indicado no es un rol', 1;";
            SqlParameter[] guardParameters = [new SqlParameter("@Id", roleId)];

            dbHelper.ExecuteTransaction(
            [
                (guard, guardParameters),
                (deleteRelations, relationParameters),
                (deleteRole, roleParameters)
            ]);
        }

        public bool HasUsers(int roleId)
        {
            const string query = "SELECT COUNT(1) FROM [User_Permission] WHERE PermissionId = @RoleId";
            SqlParameter[] parameters = [new SqlParameter("@RoleId", roleId)];

            return Convert.ToInt32(dbHelper.ExecuteScalar(query, CommandType.Text, parameters)) > 0;
        }

        public void AddPermission(int roleId, int permissionId)
        {
            const string query =
                "IF NOT EXISTS (SELECT 1 FROM [Permission_Permission] WHERE ParentId = @RoleId AND ChildId = @PermissionId) " +
                "INSERT INTO [Permission_Permission] (ParentId, ChildId) VALUES (@RoleId, @PermissionId)";
            SqlParameter[] parameters =
            [
                new SqlParameter("@RoleId", roleId),
                new SqlParameter("@PermissionId", permissionId)
            ];

            dbHelper.ExecuteNonQuery(query, CommandType.Text, parameters);
        }

        public void RemovePermission(int roleId, int permissionId)
        {
            const string query = "DELETE FROM [Permission_Permission] WHERE ParentId = @RoleId AND ChildId = @PermissionId";
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
                "IF NOT EXISTS (SELECT 1 FROM [User_Permission] WHERE UserId = @UserId AND PermissionId = @RoleId) " +
                "INSERT INTO [User_Permission] (UserId, PermissionId) VALUES (@UserId, @RoleId)";
            SqlParameter[] parameters =
            [
                new SqlParameter("@UserId", userId),
                new SqlParameter("@RoleId", roleId)
            ];

            dbHelper.ExecuteNonQuery(query, CommandType.Text, parameters);
        }

        public void RemoveFromUser(Guid userId, int roleId)
        {
            const string query = "DELETE FROM [User_Permission] WHERE UserId = @UserId AND PermissionId = @RoleId";
            SqlParameter[] parameters =
            [
                new SqlParameter("@UserId", userId),
                new SqlParameter("@RoleId", roleId)
            ];

            dbHelper.ExecuteNonQuery(query, CommandType.Text, parameters);
        }
    }
}
