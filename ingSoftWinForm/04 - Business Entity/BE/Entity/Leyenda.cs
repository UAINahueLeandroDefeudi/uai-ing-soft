namespace BE.Entity
{
    /// <summary>
    /// Texto resuelto para una etiqueta. <see cref="Traducido"/> es false cuando el idioma
    /// activo no tiene la traducción y <see cref="Texto"/> viene del idioma por defecto:
    /// así la UI puede marcarlo (o no) sin saber cómo se resolvió.
    /// </summary>
    public class Leyenda
    {
        public string Clave { get; init; } = string.Empty;
        public string Texto { get; init; } = string.Empty;
        public bool Traducido { get; init; }

        /// <summary>false si la clave no existe en ningún idioma (Texto es la propia clave).</summary>
        public bool Existe { get; init; }
    }
}
