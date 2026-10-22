namespace BE.Enum
{
    /// <summary>
    /// Qué operación originó una versión de [Producto_Historial]. Se guarda por nombre
    /// (como los enums de la bitácora) para que el historial se lea con un SELECT suelto.
    /// </summary>
    public enum OperacionProducto
    {
        Alta = 1,
        Modificacion = 2,
        Baja = 3,
        Restauracion = 4
    }
}
