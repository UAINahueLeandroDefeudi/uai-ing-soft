using System.Data;
using BE.Base;
using BE.Entity;
using BE.Enum;

namespace BE.Mapper
{
    public class ProductoMapper : BaseMapper<Producto>
    {
        public override Producto MapToEntity(DataRow row)
        {
            return new Producto
            {
                Id = (int)row["Id"],
                Codigo = (string)row["Codigo"],
                Nombre = (string)row["Nombre"],
                Descripcion = row["Descripcion"] as string,
                Precio = (decimal)row["Precio"],
                Stock = (int)row["Stock"],
                Activo = (bool)row["Activo"],
                Version = (int)row["Version"],
                FechaModificacion = (DateTime)row["FechaModificacion"]
            };
        }
    }

    public class ProductoHistorialMapper : BaseMapper<ProductoHistorial>
    {
        public override ProductoHistorial MapToEntity(DataRow row)
        {
            return new ProductoHistorial
            {
                Id = (long)row["Id"],
                ProductoId = (int)row["ProductoId"],
                Version = (int)row["Version"],
                Codigo = (string)row["Codigo"],
                Nombre = (string)row["Nombre"],
                Descripcion = row["Descripcion"] as string,
                Precio = (decimal)row["Precio"],
                Stock = (int)row["Stock"],
                Activo = (bool)row["Activo"],
                Operacion = System.Enum.Parse<OperacionProducto>((string)row["Operacion"]),
                EventAt = (DateTime)row["EventAt"],
                EventBy = (Guid)row["EventBy"],
                EventByUsername = row["EventByUsername"] as string ?? string.Empty
            };
        }
    }
}
