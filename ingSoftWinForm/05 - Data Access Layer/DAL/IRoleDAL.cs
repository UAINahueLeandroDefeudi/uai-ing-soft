using BE.Entity;

namespace DAL
{
    public interface IRoleDAL
    {
        /// <summary>Todos los roles con sus permisos (Composite incluido) ya cargados.</summary>
        List<Role> GetAll();

        /// <summary>Roles de un usuario, con sus permisos cargados.</summary>
        List<Role> GetByUser(Guid userId);

        Role? GetByName(string name);

        /// <summary>Crea el rol y devuelve su Id.</summary>
        int Insert(Role role, string createdBy);

        void Delete(int roleId);

        bool HasUsers(int roleId);

        void AddPermission(int roleId, int permissionId);

        void RemovePermission(int roleId, int permissionId);

        void AssignToUser(Guid userId, int roleId);

        void RemoveFromUser(Guid userId, int roleId);
    }
}
