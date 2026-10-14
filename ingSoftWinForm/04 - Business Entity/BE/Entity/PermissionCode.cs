namespace BE.Entity
{
    /// <summary>
    /// Códigos de los permisos simples sembrados en sql/04_init_create_roles_permission.sql.
    /// El catálogo es fijo (no hay ABM de permisos), así que el código puede referenciarlos.
    /// </summary>
    public static class PermissionCode
    {
        public const string VerMiPerfil = "VER_MI_PERFIL";
        public const string CerrarSesion = "CERRAR_SESION";
        public const string VerInicio = "VER_INICIO";
        public const string VerBitacora = "VER_BITACORA";
        public const string GestionarRoles = "GESTIONAR_ROLES";
        public const string AsignarRolesUsuario = "ASIGNAR_ROLES_USUARIO";
        public const string GestionarIdiomas = "GESTIONAR_IDIOMAS";

        /// <summary>T07: restaurar/validar backups y ver el diagnóstico de integridad.</summary>
        public const string GestionarBackup = "GESTIONAR_BACKUP";

        /// <summary>T07: generar un backup.</summary>
        public const string RealizarBackup = "REALIZAR_BACKUP";
    }

    /// <summary>Nombres de los roles sembrados que el sistema referencia.</summary>
    public static class RoleName
    {
        /// <summary>Rol que recibe todo usuario que se registra por su cuenta.</summary>
        public const string Invitado = "invitado";

        /// <summary>T07: rol que recibe el diagnóstico de integridad y puede restaurar un backup.</summary>
        public const string Administrador = "administrador";
    }
}
