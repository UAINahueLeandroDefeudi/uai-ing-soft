namespace BE.Entity
{
    public enum TipoErrorIntegridad
    {
        /// <summary>Una fila no coincide con su DVH (modificada, intercambiada o sin DVH).</summary>
        DVH,

        /// <summary>El conjunto de filas de la tabla no coincide con su DVV (alta o baja externa).</summary>
        DVV,

        /// <summary>No se pudo verificar (BD caída, tabla o columna ausente): no implica corrupción.</summary>
        NoVerificable
    }

    public class ErrorIntegridad
    {
        public string Tabla { get; set; } = string.Empty;
        public string Clave { get; set; } = string.Empty;
        public TipoErrorIntegridad Tipo { get; set; }

        /// <summary>Fecha propia del registro (EventAt) cuando la tabla la tiene; el registro es anterior a la alteración.</summary>
        public DateTime? FechaRegistro { get; set; }

        /// <summary>
        /// Última vez que la aplicación recalculó el DVV de la tabla (escritura legítima): la alteración
        /// ocurrió después de esa fecha. null si no se pudo leer.
        /// </summary>
        public DateTime? UltimoCalculoValido { get; set; }
    }

    /// <summary>Diagnóstico de la verificación de integridad al iniciar la aplicación.</summary>
    public class ResultadoIntegridad
    {
        /// <summary>Momento en que se hizo la verificación (la alteración se detectó a esta hora).</summary>
        public DateTime FechaVerificacion { get; set; } = DateTime.Now;

        public List<ErrorIntegridad> Errores { get; } = new();

        public bool EsValido => Errores.Count == 0;

        /// <summary>true si el único problema es no haber podido verificar.</summary>
        public bool SoloNoVerificable
            => Errores.Count > 0 && Errores.All(e => e.Tipo == TipoErrorIntegridad.NoVerificable);
    }
}
