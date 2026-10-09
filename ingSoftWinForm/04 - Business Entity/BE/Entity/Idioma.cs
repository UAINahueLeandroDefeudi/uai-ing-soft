namespace BE.Entity
{
    /// <summary>
    /// Idioma de la interfaz (T05). Exactamente uno es el idioma por defecto (hoy 'es'):
    /// es el que se muestra cuando a otro idioma le falta una traducción.
    /// </summary>
    public class Idioma
    {
        public int Id { get; set; }

        /// <summary>Código corto y estable ('es', 'en', 'pt'...).</summary>
        public string Codigo { get; set; } = string.Empty;

        public string Nombre { get; set; } = string.Empty;
        public bool EsDefault { get; set; }
        public bool Activo { get; set; }

        public override string ToString() => Nombre;
    }
}
