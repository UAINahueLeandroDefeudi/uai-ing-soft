using System.Data;
using BE.Entity;
using BE.Mapper;
using Microsoft.Data.SqlClient;

namespace DAL
{
    /// <summary>
    /// Lectura del catálogo de permisos (sólo lectura: no hay ABM de permisos).
    /// Dos consultas y el árbol se arma en memoria: [Permission] y [Permission_Permission].
    /// </summary>
    public class PermissionDAL : IPermissionDAL
    {
        private readonly DatabaseHelper dbHelper;
        private readonly PermissionMapper mapper;

        public PermissionDAL()
        {
            dbHelper = new DatabaseHelper();
            mapper = new PermissionMapper();
        }

        public List<Permission> GetAll()
        {
            const string queryPermissions = "SELECT Id, Code, Name, IsCompound FROM [Permission] ORDER BY Id";
            const string queryRelations = "SELECT ParentId, ChildId FROM [Permission_Permission]";

            var catalog = mapper.MapAll(dbHelper.ExecuteDataSet(queryPermissions, CommandType.Text, []).Tables[0]).ToList();
            var relations = dbHelper.ExecuteDataSet(queryRelations, CommandType.Text, []).Tables[0];

            // Cada compuesto se llena a sí mismo recursivamente a partir del catálogo.
            foreach (var permission in catalog.Where(p => p.IsCompound))
                FillChildren(permission, catalog, relations);

            return catalog;
        }

        public Permission? GetByCode(string code)
            => GetAll().FirstOrDefault(p => p.Code == code);

        /// <summary>
        /// Recursivo: agrega los hijos de <paramref name="parent"/> y desciende por los que sean compuestos.
        /// Los hijos se toman del mismo catálogo (misma instancia), no de la base otra vez.
        /// </summary>
        private static void FillChildren(Permission parent, List<Permission> catalog, DataTable relations)
        {
            foreach (DataRow row in relations.Select($"ParentId = {parent.Id}"))
            {
                var child = catalog.FirstOrDefault(p => p.Id == (int)row["ChildId"]);
                if (child == null) continue;

                parent.Add(child);
                if (child.IsCompound) FillChildren(child, catalog, relations);
            }
        }
    }
}
