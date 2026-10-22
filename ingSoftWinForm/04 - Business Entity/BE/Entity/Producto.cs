namespace BE.Entity
{
    /// <summary>
    /// Entidad de negocio genérica (T06b): cada alta, modificación, baja o restauración deja una
    /// copia completa de la fila en <see cref="ProductoHistorial"/>. La baja es lógica (Activo = false).
    /// </summary>
    public class Producto
    {
        public int Id { get; set; }

        /// <summary>Código único y visible del producto (p. ej. SKU).</summary>
        public string Codigo { get; set; } = string.Empty;

        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public decimal Precio { get; set; }
        public int Stock { get; set; }
        public bool Activo { get; set; } = true;

        /// <summary>Número de la última versión registrada en el historial.</summary>
        public int Version { get; set; }

        public DateTime FechaModificacion { get; set; }

        public override string ToString() => $"{Codigo} - {Nombre}";
    }
}
