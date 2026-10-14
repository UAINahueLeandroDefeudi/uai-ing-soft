namespace BE.Entity
{
    /// <summary>Dígito verificador vertical de una tabla: lo almacenado contra lo recalculado.</summary>
    public class DVV
    {
        public string Tabla { get; set; } = string.Empty;

        /// <summary>Valor guardado en [DigitoVerificador] (null si la tabla no tiene control).</summary>
        public string? Almacenado { get; set; }

        public string Calculado { get; set; } = string.Empty;

        public bool EsValido
            => Almacenado != null && string.Equals(Almacenado, Calculado, StringComparison.OrdinalIgnoreCase);
    }
}
