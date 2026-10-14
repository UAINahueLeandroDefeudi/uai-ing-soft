using System.Data;
using BE.Entity;
using BE.Mapper;

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
            const string queryPermissions = "SELECT Id, Code, Name, IsFamily, IsRol FROM [Permission] ORDER BY Id";
            const string queryRelations = "SELECT ParentId, ChildId FROM [Permission_Permission]";

            var catalog = mapper.MapAll(dbHelper.ExecuteDataSet(queryPermissions, CommandType.Text, []).Tables[0]).ToList();
            var relations = dbHelper.ExecuteDataSet(queryRelations, CommandType.Text, []).Tables[0];

            // Padre -> hijos, con las mismas instancias del catálogo (no se vuelve a la base).
            var byId = catalog.ToDictionary(p => p.Id);
            var childrenByParent = relations.Rows.Cast<DataRow>()
                .Where(row => byId.ContainsKey((int)row["ChildId"]))
                .ToLookup(row => (int)row["ParentId"], row => byId[(int)row["ChildId"]]);

            // Polimorfismo: la hoja ignora la llamada y el compuesto agrega sus hijos.
            foreach (var permission in catalog)
                permission.LoadChildren(childrenByParent);

            return catalog;
        }

        public Permission? GetByCode(string code)
            => GetAll().FirstOrDefault(p => p.Code == code);
    }
}
