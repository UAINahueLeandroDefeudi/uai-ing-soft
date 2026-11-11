namespace BE.Enum
{
    /// <summary>
    /// Qué originó una versión de [User_History]. Se guarda por nombre (como los enums de la
    /// bitácora) para que el historial se lea con un SELECT suelto.
    /// </summary>
    public enum TipoCambioUsuario
    {
        Alta = 1,
        Modificacion = 2,
        Restauracion = 3
    }
}
