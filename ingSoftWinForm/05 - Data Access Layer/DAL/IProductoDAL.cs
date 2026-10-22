using BE.Entity;
using BE.Enum;

namespace DAL
{
    /// <summary>
    /// T06b: productos y su historial. Cada escritura deja, en la misma transacción,
    /// una copia completa de la fila en [Producto_Historial] (quién, cuándo y cómo quedó).
    /// </summary>
    public interface IProductoDAL
    {
        List<Producto> GetAll();
        Producto? GetById(int id);
        Producto? GetByCodigo(string codigo);

        /// <summary>Versiones del producto, de la más nueva a la más vieja, con el username de quien la generó.</summary>
        List<ProductoHistorial> GetHistorial(int productoId);

        /// <summary>Alta como versión 1. Completa Id, Version y FechaModificacion del producto.</summary>
        void Insert(Producto producto, Guid eventBy);

        /// <summary>
        /// Guarda todos los campos del producto como una versión nueva (Version + 1) y la registra en el
        /// historial con <paramref name="operacion"/>. Devuelve false, sin escribir nada, si el producto
        /// ya no está en la versión que se leyó (otro usuario lo modificó antes).
        /// </summary>
        bool Update(Producto producto, OperacionProducto operacion, Guid eventBy);
    }
}
