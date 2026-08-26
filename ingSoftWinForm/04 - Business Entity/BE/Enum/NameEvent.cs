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
        ErrorSistema = 8
    }
}
