using BE.Entity;

namespace DAL
{
    public interface IPermissionDAL
    {
        /// <summary>Todos los permisos del catálogo con su árbol Composite ya armado.</summary>
        List<Permission> GetAll();

        Permission? GetByCode(string code);
    }
}
