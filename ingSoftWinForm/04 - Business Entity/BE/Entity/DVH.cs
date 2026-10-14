namespace BE.Entity
{
    /// <summary>Dígito verificador horizontal de una fila: lo almacenado contra lo recalculado.</summary>
    public class DVH
    {
        public string Tabla { get; set; } = string.Empty;

        /// <summary>Texto de la clave de la fila (valores de la PK unidos por ':').</summary>
        public string Clave { get; set; } = string.Empty;

        /// <summary>Valor guardado en la columna DVH (null si la fila nunca lo tuvo).</summary>
        public string? Almacenado { get; set; }

        public string Calculado { get; set; } = string.Empty;

        public bool EsValido
            => Almacenado != null && string.Equals(Almacenado, Calculado, StringComparison.OrdinalIgnoreCase);
    }
}
