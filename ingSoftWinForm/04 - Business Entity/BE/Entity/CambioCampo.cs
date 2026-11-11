namespace BE.Entity
{
    /// <summary>
    /// Un campo versionado del usuario al comparar dos versiones de [User_History].
    /// <see cref="Campo"/> es el identificador del campo (NickName, FirstName, LastName, Pais, Provincia,
    /// Direccion); la UI lo traduce. <see cref="Cambio"/> es true si el valor difiere del de la versión anterior.
    /// </summary>
    public sealed record CambioCampo(string Campo, string? ValorAnterior, string? ValorActual, bool Cambio);

    /// <summary>
    /// Qué decir en la columna "Detalle" de una versión. <see cref="Tipo"/> indica el caso y la UI arma el texto
    /// traducido: Alta, Modificó: &lt;campos&gt;, Restauración de la versión N, o vacío.
    /// </summary>
    public sealed record DetalleVersion(TipoDetalleVersion Tipo, IReadOnlyList<string> Campos, int? RestauradaDesde);

    public enum TipoDetalleVersion
    {
        /// <summary>Sin nada para informar (modificación sin diferencias detectables).</summary>
        Ninguno = 0,
        Alta = 1,
        Modificacion = 2,
        Restauracion = 3
    }
}
