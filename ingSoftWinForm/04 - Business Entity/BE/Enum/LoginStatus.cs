namespace BE.Enum
{
    /// <summary>
    /// Resultado posible del CU-01 Iniciar sesión.
    /// Equivale al MotivoRechazo de DC-login.md.
    /// </summary>
    public enum LoginStatus
    {
        Success,
        InvalidCredentials,
        UserBlocked,
        UserInactive,
        SessionAlreadyOpen,

        /// <summary>T07: falló la verificación de integridad y el usuario no es administrador.</summary>
        SystemError
    }
}
