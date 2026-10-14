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
    }

    /// <summary>Diagnóstico de la verificación de integridad al iniciar la aplicación.</summary>
    public class ResultadoIntegridad
    {
        public List<ErrorIntegridad> Errores { get; } = new();

        public bool EsValido => Errores.Count == 0;

        /// <summary>true si el único problema es no haber podido verificar.</summary>
        public bool SoloNoVerificable
            => Errores.Count > 0 && Errores.All(e => e.Tipo == TipoErrorIntegridad.NoVerificable);
    }
}
