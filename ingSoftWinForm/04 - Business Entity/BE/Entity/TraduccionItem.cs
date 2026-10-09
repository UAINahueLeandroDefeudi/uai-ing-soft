namespace BE.Entity
{
    /// <summary>
    /// Fila de la administración de idiomas: una etiqueta con su texto por defecto y su
    /// traducción al idioma elegido (null si todavía no está traducida).
    /// </summary>
    public class TraduccionItem
    {
        public int IdEtiqueta { get; set; }
        public string Clave { get; set; } = string.Empty;
        public string TextoDefault { get; set; } = string.Empty;
        public string? Texto { get; set; }

        public bool Traducido => !string.IsNullOrWhiteSpace(Texto);
    }
}
