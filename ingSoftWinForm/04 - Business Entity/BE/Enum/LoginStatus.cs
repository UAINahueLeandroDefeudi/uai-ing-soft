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
        SessionAlreadyOpen
    }
}
