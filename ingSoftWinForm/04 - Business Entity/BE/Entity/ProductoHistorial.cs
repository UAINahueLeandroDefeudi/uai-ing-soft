using BE.Enum;

namespace BE.Entity
{
    /// <summary>
    /// T06b: fila de [Producto_Historial]. Es la copia completa del producto tal como quedó en esa
    /// versión (no guarda valor viejo / valor nuevo): lo que cambió se ve comparando dos versiones,
    /// y cualquier versión se puede volver a aplicar. Responde quién (EventBy), cuándo (EventAt) y qué.
    /// </summary>
    public class ProductoHistorial
    {
        public long Id { get; set; }
        public int ProductoId { get; set; }
        public int Version { get; set; }

        public string Codigo { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public decimal Precio { get; set; }
        public int Stock { get; set; }
        public bool Activo { get; set; }

        public OperacionProducto Operacion { get; set; }
        public DateTime EventAt { get; set; }

        /// <summary>Id del usuario que hizo el cambio.</summary>
        public Guid EventBy { get; set; }

        /// <summary>Username de <see cref="EventBy"/>, solo para mostrar (sale de un JOIN, no se guarda).</summary>
        public string EventByUsername { get; set; } = string.Empty;
    }
}
