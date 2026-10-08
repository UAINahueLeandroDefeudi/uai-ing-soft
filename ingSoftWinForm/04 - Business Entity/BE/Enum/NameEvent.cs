namespace BE.Enum
{
    /// <summary>
    /// Acción que originó el registro de bitácora. Los valores son explícitos
    /// para que los ordinales queden estables aunque se agreguen eventos nuevos.
    /// </summary>
    public enum NameEvent
    {
        Login = 1,
        Logout = 2,
        CrearUsuario = 3,
        ModificarUsuario = 4,
        EliminarUsuario = 5,
        CambiarPassword = 6,
        AccesoNoAutorizado = 7,
        ErrorSistema = 8,
        CrearRol = 9,
        EliminarRol = 10,
        AsignarPermisoRol = 11,
        QuitarPermisoRol = 12,
        AsignarRolUsuario = 13,
        QuitarRolUsuario = 14
    }
}
