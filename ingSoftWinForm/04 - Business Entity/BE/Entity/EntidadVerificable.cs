namespace BE.Entity
{
    /// <summary>
    /// T07: descriptor de una tabla protegida con dígitos verificadores. Hacer verificable
    /// una tabla nueva es agregar una línea en <see cref="EntidadesVerificables"/>
    /// (y la columna DVH): el cálculo, la verificación y las escrituras no cambian.
    /// </summary>
    public class EntidadVerificable
    {
        /// <summary>Nombre de la tabla. Sale solo de este registro (nunca de la UI).</summary>
        public string Tabla { get; }

        /// <summary>Columnas que identifican la fila (PK).</summary>
        public IReadOnlyList<string> Clave { get; }

        /// <summary>
        /// Columnas que entran al DVH, en orden fijo y sin la columna DVH.
        /// El índice en la lista es la posición del atributo dentro de la entidad.
        /// </summary>
        public IReadOnlyList<string> Atributos { get; }

        /// <summary>
        /// Columna con la fecha propia de la fila (cuándo se escribió), para informar desde cuándo
        /// existe un registro alterado. Null si la tabla no guarda fecha.
        /// </summary>
        public string? ColumnaFecha { get; }

        public EntidadVerificable(string tabla, string[] clave, string[] atributos, string? columnaFecha = null)
        {
            Tabla = tabla;
            Clave = clave;
            Atributos = atributos;
            ColumnaFecha = columnaFecha;
        }

        /// <summary>Fecha propia de la fila, si la tabla la guarda.</summary>
        public DateTime? FechaDe(IReadOnlyDictionary<string, object?> fila)
            => ColumnaFecha != null && fila.TryGetValue(ColumnaFecha, out var fecha) && fecha is DateTime valor
                ? valor
                : null;
    }

    /// <summary>Registro de las tablas verificables del sistema.</summary>
    public static class EntidadesVerificables
    {
        public static readonly EntidadVerificable Producto = new(
            "Producto",
            ["Id"],
            ["Id", "Codigo", "Nombre", "Descripcion", "Precio", "Stock", "Activo", "Version", "FechaModificacion"],
            "FechaModificacion");

        /// <summary>T06b: copia completa del producto en cada versión, con quién (EventBy) y cuándo (EventAt).</summary>
        public static readonly EntidadVerificable ProductoHistorial = new(
            "Producto_Historial",
            ["Id"],
            ["Id", "ProductoId", "Version", "Codigo", "Nombre", "Descripcion", "Precio", "Stock", "Activo",
             "Operacion", "EventAt", "EventBy"],
            "EventAt");

        public static readonly EntidadVerificable UserPermission = new(
            "User_Permission",
            ["UserId", "PermissionId"],
            ["UserId", "PermissionId"]);

        /// <summary>Agregar acá cada tabla nueva a proteger.</summary>
        public static IReadOnlyList<EntidadVerificable> Todas { get; } = [Producto, ProductoHistorial, UserPermission];
    }
}
