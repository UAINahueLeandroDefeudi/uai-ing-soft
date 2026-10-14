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

        public EntidadVerificable(string tabla, string[] clave, string[] atributos)
        {
            Tabla = tabla;
            Clave = clave;
            Atributos = atributos;
        }
    }

    /// <summary>Registro de las tablas verificables del sistema.</summary>
    public static class EntidadesVerificables
    {
        public static readonly EntidadVerificable UserHistory = new(
            "UserHistory",
            ["Id"],
            ["Id", "UserId", "Username", "EventType", "Success", "FailedAttempts",
             "FieldName", "OldValue", "NewValue", "Detail", "EventAt"]);

        public static readonly EntidadVerificable UserPermission = new(
            "User_Permission",
            ["UserId", "PermissionId"],
            ["UserId", "PermissionId"]);

        /// <summary>Agregar acá cada tabla nueva a proteger.</summary>
        public static IReadOnlyList<EntidadVerificable> Todas { get; } = [UserHistory, UserPermission];
    }
}
