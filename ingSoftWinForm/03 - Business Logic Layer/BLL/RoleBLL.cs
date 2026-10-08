using BE.Entity;
using BE.Enum;
using DAL;
using Services;

namespace BLL
{
    /// <summary>
    /// Gestión de roles (T04). ABM de roles y asignación/quita de permisos simples o
    /// compuestos a un rol; asignación/quita de roles a un usuario. Los permisos en
    /// sí son un catálogo fijo: no se crean, modifican ni eliminan.
    /// Cada operación valida el permiso de quien la ejecuta (defensa en profundidad:
    /// no depende de que la UI haya ocultado el botón) y queda en bitácora.
    /// </summary>
    public class RoleBLL
    {
        private const int MaxRoleNameLength = 50;

        private readonly IRoleDAL roleDAL;
        private readonly IPermissionDAL permissionDAL;
        private readonly IUserDAL userDAL;
        private readonly BitacoraBLL bitacoraBLL;

        public RoleBLL() : this(new RoleDAL(), new PermissionDAL(), new UserDAL(), new BitacoraBLL()) { }

        public RoleBLL(IRoleDAL roleDAL, IPermissionDAL permissionDAL, IUserDAL userDAL, BitacoraBLL bitacoraBLL)
        {
            this.roleDAL = roleDAL;
            this.permissionDAL = permissionDAL;
            this.userDAL = userDAL;
            this.bitacoraBLL = bitacoraBLL;
        }

        // ---- Consultas ----

        public List<Role> GetRoles() => roleDAL.GetAll();

        /// <summary>Catálogo completo de permisos (simples y compuestos) con su árbol armado.</summary>
        public List<Permission> GetPermissionCatalog() => permissionDAL.GetAll();

        public List<User> GetUsers() => userDAL.GetAll();

        public List<Role> GetUserRoles(User user) => roleDAL.GetByUser(user.Id);

        // ---- ABM de roles ----

        public OperationResult CreateRole(string name)
        {
            if (!Authorize(PermissionCode.GestionarRoles, "crear un rol")) return Denied();

            name = name.Trim();
            if (name.Length == 0) return OperationResult.Fail("El nombre del rol es obligatorio.");
            if (name.Length > MaxRoleNameLength)
                return OperationResult.Fail($"El nombre del rol no puede superar {MaxRoleNameLength} caracteres.");
            if (roleDAL.GetByName(name) != null)
                return OperationResult.Fail("Ya existe un rol con ese nombre.");

            return Execute(NameEvent.CrearRol, $"Creación del rol '{name}'", () =>
            {
                var role = new Role { Name = name };
                roleDAL.Insert(role, SessionManager.GetInstance.User.Username);
            }, "Rol creado.");
        }

        public OperationResult DeleteRole(Role role)
        {
            if (!Authorize(PermissionCode.GestionarRoles, "eliminar un rol")) return Denied();

            if (role.Name == RoleName.Invitado)
                return OperationResult.Fail("El rol 'invitado' es del sistema y no se puede eliminar.");
            if (roleDAL.HasUsers(role.Id))
                return OperationResult.Fail("El rol tiene usuarios asignados. Quitaselo a los usuarios antes de eliminarlo.");

            return Execute(NameEvent.EliminarRol, $"Eliminación del rol '{role.Name}'",
                () => roleDAL.Delete(role.Id), "Rol eliminado.");
        }

        // ---- Permisos de un rol ----

        public OperationResult AddPermissionToRole(Role role, Permission permission)
        {
            if (!Authorize(PermissionCode.GestionarRoles, "agregar un permiso a un rol")) return Denied();

            if (role.Grants(permission.Code))
                return OperationResult.Fail($"El rol '{role.Name}' ya otorga '{permission.Name}'.");

            return Execute(NameEvent.AsignarPermisoRol,
                $"Se agregó el permiso '{permission.Code}' al rol '{role.Name}'", () =>
                {
                    roleDAL.AddPermission(role.Id, permission.Id);
                    role.AddPermission(permission);
                }, "Permiso agregado al rol.");
        }

        public OperationResult RemovePermissionFromRole(Role role, Permission permission)
        {
            if (!Authorize(PermissionCode.GestionarRoles, "quitar un permiso de un rol")) return Denied();

            if (!role.Permissions.Any(p => p.Code == permission.Code))
                return OperationResult.Fail(
                    $"'{permission.Name}' no está asignado directamente al rol '{role.Name}' (forma parte de otro permiso compuesto).");

            return Execute(NameEvent.QuitarPermisoRol,
                $"Se quitó el permiso '{permission.Code}' del rol '{role.Name}'", () =>
                {
                    roleDAL.RemovePermission(role.Id, permission.Id);
                    role.RemovePermission(permission);
                }, "Permiso quitado del rol.");
        }

        // ---- Roles de un usuario ----

        public OperationResult AssignRoleToUser(User user, Role role)
        {
            if (!Authorize(PermissionCode.AsignarRolesUsuario, "asignar un rol a un usuario")) return Denied();

            if (roleDAL.GetByUser(user.Id).Any(r => r.Id == role.Id))
                return OperationResult.Fail($"'{user.Username}' ya tiene el rol '{role.Name}'.");

            return Execute(NameEvent.AsignarRolUsuario,
                $"Se asignó el rol '{role.Name}' al usuario '{user.Username}'",
                () => roleDAL.AssignToUser(user.Id, role.Id), "Rol asignado al usuario.");
        }

        public OperationResult RemoveRoleFromUser(User user, Role role)
        {
            if (!Authorize(PermissionCode.AsignarRolesUsuario, "quitar un rol a un usuario")) return Denied();

            var current = roleDAL.GetByUser(user.Id);
            if (current.All(r => r.Id != role.Id))
                return OperationResult.Fail($"'{user.Username}' no tiene el rol '{role.Name}'.");

            // Evita que el último administrador se deje sin acceso a la gestión.
            if (user.Id == SessionManager.GetInstance.User.Id && role.Grants(PermissionCode.GestionarRoles))
                return OperationResult.Fail("No podés quitarte a vos mismo un rol que otorga la gestión de roles.");

            return Execute(NameEvent.QuitarRolUsuario,
                $"Se quitó el rol '{role.Name}' al usuario '{user.Username}'",
                () => roleDAL.RemoveFromUser(user.Id, role.Id), "Rol quitado al usuario.");
        }

        // ---- Soporte ----

        /// <summary>
        /// Verifica que la sesión activa tenga el permiso. Si no, queda auditado como
        /// AccesoNoAutorizado (RNF-Seguridad-03).
        /// </summary>
        private bool Authorize(string permissionCode, string action)
        {
            if (SessionManager.HasPermission(permissionCode)) return true;

            bitacoraBLL.RegistrarError(NameEvent.AccesoNoAutorizado,
                $"Intento no autorizado de {action} (requiere {permissionCode})", Priority.High);

            return false;
        }

        private static OperationResult Denied()
            => OperationResult.Fail("No tenés permiso para realizar esta operación.");

        /// <summary>Ejecuta la acción; si la base falla lo deja en bitácora y devuelve un error genérico.</summary>
        private OperationResult Execute(NameEvent nameEvent, string detail, Action action, string okMessage)
        {
            try
            {
                action();
                bitacoraBLL.RegistrarEvento(nameEvent, detail, Priority.Medium);
                return OperationResult.Ok(okMessage);
            }
            catch (Exception ex)
            {
                bitacoraBLL.RegistrarError(NameEvent.ErrorSistema, $"{detail}: {ex.Message}", Priority.High);
                return OperationResult.Fail("No se pudo completar la operación. Intente nuevamente.");
            }
        }
    }
}
