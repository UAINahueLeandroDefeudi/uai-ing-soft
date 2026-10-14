using BE.Entity;

namespace DAL
{
    /// <summary>
    /// Roles (Modelo I): un rol es un Permission compuesto con IsRol = 1. No hay tabla
    /// de roles: viven en [Permission], sus hijos en [Permission_Permission] y su
    /// asignación a usuarios en [User_Permission].
    /// </summary>
    public interface IRoleDAL
    {
        /// <summary>Todos los roles con su árbol de permisos (Composite) ya cargado.</summary>
        List<Permission> GetAll();

        /// <summary>Roles de un usuario, con su árbol cargado.</summary>
        List<Permission> GetByUser(Guid userId);

        Permission? GetByName(string name);

        /// <summary>Crea el rol (familia con IsRol = 1) y devuelve su Id.</summary>
        int Insert(Permission role);

        /// <summary>Sólo borra roles: una hoja o una familia que no es rol no se toca.</summary>
        void Delete(int roleId);

        bool HasUsers(int roleId);

        /// <summary>Agrega un hijo (hoja, familia u otro rol) al rol.</summary>
        void AddPermission(int roleId, int permissionId);

        void RemovePermission(int roleId, int permissionId);

        void AssignToUser(Guid userId, int roleId);

        void RemoveFromUser(Guid userId, int roleId);
    }
}
