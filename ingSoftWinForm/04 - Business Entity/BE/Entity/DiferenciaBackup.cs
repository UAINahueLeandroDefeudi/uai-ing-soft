namespace BE.Entity
{
    public enum TipoDiferencia
    {
        /// <summary>Existe en la base actual y no en el backup: se perdería al restaurar.</summary>
        SoloEnActual,

        /// <summary>Existe en el backup y no en la base actual: se recuperaría (típicamente, un borrado externo).</summary>
        SoloEnBackup,

        /// <summary>Existe en ambos con distinto contenido.</summary>
        Modificada
    }

    /// <summary>Una columna que difiere entre la base actual y el backup.</summary>
    public class CambioColumna
    {
        public string Columna { get; set; } = string.Empty;
        public string? ValorActual { get; set; }
        public string? ValorBackup { get; set; }
    }

    /// <summary>
    /// T07: una fila que difiere entre la base actual y el backup que se quiere restaurar,
    /// con el máximo detalle disponible: tabla, registro, columnas y fecha.
    /// </summary>
    public class DiferenciaBackup
    {
        public string Tabla { get; set; } = string.Empty;

        /// <summary>Clave de la fila (valores de la PK unidos por ':').</summary>
        public string Clave { get; set; } = string.Empty;

        public TipoDiferencia Tipo { get; set; }

        /// <summary>Columnas con distinto valor (solo para <see cref="TipoDiferencia.Modificada"/>).</summary>
        public List<CambioColumna> Cambios { get; } = new();

        /// <summary>Fecha del registro (EventAt) del lado que la tenga; null si la tabla no la guarda.</summary>
        public DateTime? FechaRegistro { get; set; }

        /// <summary>La fila actual no coincide con su propio DVH: es la fila adulterada fuera de la aplicación.</summary>
        public bool FilaActualAdulterada { get; set; }
    }
}
