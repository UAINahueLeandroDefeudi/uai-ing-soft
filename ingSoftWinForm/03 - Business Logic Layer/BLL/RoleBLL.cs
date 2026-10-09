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
    /// T05: los resultados llevan la clave de la etiqueta del mensaje (msg.role.*), no el texto.
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
            if (name.Length == 0) return OperationResult.Fail("msg.role.nameRequired");
            if (name.Length > MaxRoleNameLength)
                return OperationResult.Fail("msg.role.nameTooLong", MaxRoleNameLength);
            if (roleDAL.GetByName(name) != null)
                return OperationResult.Fail("msg.role.exists");

            return Execute(NameEvent.CrearRol, $"Creación del rol '{name}'", () =>
            {
                var role = new Role { Name = name };
                roleDAL.Insert(role, SessionManager.GetInstance.User.Username);
            }, "msg.role.created");
        }

        public OperationResult DeleteRole(Role role)
        {
            if (!Authorize(PermissionCode.GestionarRoles, "eliminar un rol")) return Denied();

            if (role.Name == RoleName.Invitado)
                return OperationResult.Fail("msg.role.invitadoSystem");
            if (roleDAL.HasUsers(role.Id))
                return OperationResult.Fail("msg.role.hasUsers");

            return Execute(NameEvent.EliminarRol, $"Eliminación del rol '{role.Name}'",
                () => roleDAL.Delete(role.Id), "msg.role.deleted");
        }

        // ---- Permisos de un rol ----

        public OperationResult AddPermissionToRole(Role role, Permission permission)
        {
            if (!Authorize(PermissionCode.GestionarRoles, "agregar un permiso a un rol")) return Denied();

            if (role.Grants(permission.Code))
                return OperationResult.Fail("msg.role.alreadyGrants", role.Name, permission.Name);

            return Execute(NameEvent.AsignarPermisoRol,
                $"Se agregó el permiso '{permission.Code}' al rol '{role.Name}'", () =>
                {
                    roleDAL.AddPermission(role.Id, permission.Id);
                    role.AddPermission(permission);
                }, "msg.role.permAdded");
        }

        public OperationResult RemovePermissionFromRole(Role role, Permission permission)
        {
            if (!Authorize(PermissionCode.GestionarRoles, "quitar un permiso de un rol")) return Denied();

            if (!role.Permissions.Any(p => p.Code == permission.Code))
                return OperationResult.Fail("msg.role.permNotDirect", permission.Name, role.Name);

            return Execute(NameEvent.QuitarPermisoRol,
                $"Se quitó el permiso '{permission.Code}' del rol '{role.Name}'", () =>
                {
                    roleDAL.RemovePermission(role.Id, permission.Id);
                    role.RemovePermission(permission);
                }, "msg.role.permRemoved");
        }

        // ---- Roles de un usuario ----

        public OperationResult AssignRoleToUser(User user, Role role)
        {
            if (!Authorize(PermissionCode.AsignarRolesUsuario, "asignar un rol a un usuario")) return Denied();

            if (roleDAL.GetByUser(user.Id).Any(r => r.Id == role.Id))
                return OperationResult.Fail("msg.role.userHas", user.Username, role.Name);

            return Execute(NameEvent.AsignarRolUsuario,
                $"Se asignó el rol '{role.Name}' al usuario '{user.Username}'",
                () => roleDAL.AssignToUser(user.Id, role.Id), "msg.role.assigned");
        }

        public OperationResult RemoveRoleFromUser(User user, Role role)
        {
            if (!Authorize(PermissionCode.AsignarRolesUsuario, "quitar un rol a un usuario")) return Denied();

            var current = roleDAL.GetByUser(user.Id);
            if (current.All(r => r.Id != role.Id))
                return OperationResult.Fail("msg.role.userHasNot", user.Username, role.Name);

            // Evita que el último administrador se deje sin acceso a la gestión.
            if (user.Id == SessionManager.GetInstance.User.Id && role.Grants(PermissionCode.GestionarRoles))
                return OperationResult.Fail("msg.role.selfRemove");

            return Execute(NameEvent.QuitarRolUsuario,
                $"Se quitó el rol '{role.Name}' al usuario '{user.Username}'",
                () => roleDAL.RemoveFromUser(user.Id, role.Id), "msg.role.unassigned");
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

        private static OperationResult Denied() => OperationResult.Fail("msg.noPermiso");

        /// <summary>Ejecuta la acción; si la base falla lo deja en bitácora y devuelve un error genérico.</summary>
        private OperationResult Execute(NameEvent nameEvent, string detail, Action action, string okKey)
        {
            try
            {
                action();
                bitacoraBLL.RegistrarEvento(nameEvent, detail, Priority.Medium);
                return OperationResult.Ok(okKey);
            }
            catch (Exception ex)
            {
                bitacoraBLL.RegistrarError(NameEvent.ErrorSistema, $"{detail}: {ex.Message}", Priority.High);
                return OperationResult.Fail("msg.generic.error");
            }
        }
    }
}
