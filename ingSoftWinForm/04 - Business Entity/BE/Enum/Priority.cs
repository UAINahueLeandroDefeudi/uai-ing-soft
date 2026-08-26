namespace BE.Enum
{
    /// <summary>
    /// Criticidad de un registro de bitácora. Extiende a cinco niveles la
    /// criticidad "Baja / Media / Alta" de DER-login.md.
    /// </summary>
    public enum Priority
    {
        Low,
        Medium,
        High,
        Critical,
        Fatal
    }
}
