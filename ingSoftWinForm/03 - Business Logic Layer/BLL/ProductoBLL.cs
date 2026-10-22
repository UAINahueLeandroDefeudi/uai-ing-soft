using BE.Entity;
using BE.Enum;
using DAL;
using Services;

namespace BLL
{
    /// <summary>
    /// ABM de productos con control de cambios (T06b, permiso GESTIONAR_PRODUCTOS).
    /// Cada operación guarda una versión nueva con la copia completa del producto, quién la hizo
    /// y cuándo. Restaurar una versión vieja no reescribe el historial: la vuelve a aplicar como
    /// una versión nueva, así la traza queda completa.
    /// </summary>
    public class ProductoBLL
    {
        public const int MaxCodigoLength = 20;
        public const int MaxNombreLength = 100;
        public const int MaxDescripcionLength = 500;

        /// <summary>Tope de DECIMAL(18,2).</summary>
        private const decimal MaxPrecio = 9_999_999_999_999_999.99m;

        private readonly IProductoDAL productoDAL;
        private readonly BitacoraBLL bitacoraBLL;

        public ProductoBLL() : this(new ProductoDAL(), new BitacoraBLL()) { }

        public ProductoBLL(IProductoDAL productoDAL, BitacoraBLL bitacoraBLL)
        {
            this.productoDAL = productoDAL;
            this.bitacoraBLL = bitacoraBLL;
        }

        public List<Producto> GetAll() => productoDAL.GetAll();

        public List<ProductoHistorial> GetHistorial(Producto producto) => productoDAL.GetHistorial(producto.Id);

        public OperationResult Crear(string codigo, string nombre, string? descripcion, decimal precio, int stock)
        {
            if (!Authorize("crear un producto")) return Denied();

            var producto = new Producto();
            var invalido = Completar(producto, codigo, nombre, descripcion, precio, stock);
            if (invalido != null) return invalido;

            var duplicado = CodigoEnUso(producto);
            if (duplicado != null) return duplicado;

            return Execute(NameEvent.CrearProducto, $"Alta del producto '{producto.Codigo}'", () =>
            {
                productoDAL.Insert(producto, UsuarioActual());
                return true;
            }, "msg.producto.creado");
        }

        public OperationResult Modificar(Producto producto, string codigo, string nombre, string? descripcion,
            decimal precio, int stock)
        {
            if (!Authorize("modificar un producto")) return Denied();

            // Se trabaja sobre una copia: si la operación se rechaza, el objeto de la UI no cambia.
            var cambiado = Copiar(producto);
            var invalido = Completar(cambiado, codigo, nombre, descripcion, precio, stock);
            if (invalido != null) return invalido;

            if (MismosDatos(cambiado, producto)) return OperationResult.Fail("msg.producto.sinCambios");

            var duplicado = CodigoEnUso(cambiado);
            if (duplicado != null) return duplicado;

            return Execute(NameEvent.ModificarProducto, $"Modificación del producto '{cambiado.Codigo}'",
                () => productoDAL.Update(cambiado, OperacionProducto.Modificacion, UsuarioActual()),
                "msg.producto.actualizado");
        }

        public OperationResult DarDeBaja(Producto producto)
        {
            if (!Authorize("dar de baja un producto")) return Denied();
            if (!producto.Activo) return OperationResult.Fail("msg.producto.yaDadoDeBaja");

            var baja = Copiar(producto);
            baja.Activo = false;

            return Execute(NameEvent.BajaProducto, $"Baja del producto '{producto.Codigo}'",
                () => productoDAL.Update(baja, OperacionProducto.Baja, UsuarioActual()),
                "msg.producto.dadoDeBaja");
        }

        /// <summary>Vuelve a aplicar todos los datos de <paramref name="version"/> como una versión nueva.</summary>
        public OperationResult RestaurarVersion(Producto producto, ProductoHistorial version)
        {
            if (!Authorize("restaurar una versión de un producto")) return Denied();
            if (version.ProductoId != producto.Id) return OperationResult.Fail("msg.generic.error");
            if (version.Version == producto.Version) return OperationResult.Fail("msg.producto.versionActual");

            var restaurado = Copiar(producto);
            restaurado.Codigo = version.Codigo;
            restaurado.Nombre = version.Nombre;
            restaurado.Descripcion = version.Descripcion;
            restaurado.Precio = version.Precio;
            restaurado.Stock = version.Stock;
            restaurado.Activo = version.Activo;

            if (MismosDatos(restaurado, producto)) return OperationResult.Fail("msg.producto.sinCambios");

            // El código de aquella versión pudo haber pasado a otro producto.
            var duplicado = CodigoEnUso(restaurado);
            if (duplicado != null) return duplicado;

            return Execute(NameEvent.RestaurarVersionProducto,
                $"Producto '{restaurado.Codigo}' restaurado a la versión {version.Version}",
                () => productoDAL.Update(restaurado, OperacionProducto.Restauracion, UsuarioActual()),
                "msg.producto.restaurado", version.Version);
        }

        // ---- Soporte ----

        /// <summary>Valida y carga los datos editables. Devuelve el rechazo, o null si son válidos.</summary>
        private static OperationResult? Completar(Producto producto, string codigo, string nombre, string? descripcion,
            decimal precio, int stock)
        {
            codigo = codigo.Trim().ToUpperInvariant();
            nombre = nombre.Trim();
            descripcion = string.IsNullOrWhiteSpace(descripcion) ? null : descripcion.Trim();

            if (codigo.Length == 0 || codigo.Length > MaxCodigoLength)
                return OperationResult.Fail("msg.producto.codigoInvalido", MaxCodigoLength);
            if (nombre.Length == 0 || nombre.Length > MaxNombreLength)
                return OperationResult.Fail("msg.producto.nombreInvalido", MaxNombreLength);
            if (descripcion is { Length: > MaxDescripcionLength })
                return OperationResult.Fail("msg.producto.descripcionLarga", MaxDescripcionLength);
            if (precio < 0 || precio > MaxPrecio || decimal.Round(precio, 2) != precio)
                return OperationResult.Fail("msg.producto.precioInvalido");
            if (stock < 0) return OperationResult.Fail("msg.producto.stockInvalido");

            producto.Codigo = codigo;
            producto.Nombre = nombre;
            producto.Descripcion = descripcion;
            producto.Precio = precio;
            producto.Stock = stock;
            return null;
        }

        private OperationResult? CodigoEnUso(Producto producto)
        {
            try
            {
                var existente = productoDAL.GetByCodigo(producto.Codigo);
                return existente != null && existente.Id != producto.Id
                    ? OperationResult.Fail("msg.producto.codigoExiste", producto.Codigo)
                    : null;
            }
            catch (Exception ex)
            {
                RegistrarFalloSistema("Falló la consulta de productos", ex);
                return OperationResult.Fail("msg.generic.error");
            }
        }

        private static bool MismosDatos(Producto a, Producto b)
            => a.Codigo == b.Codigo && a.Nombre == b.Nombre && a.Descripcion == b.Descripcion
               && a.Precio == b.Precio && a.Stock == b.Stock && a.Activo == b.Activo;

        private static Producto Copiar(Producto p) => new()
        {
            Id = p.Id,
            Codigo = p.Codigo,
            Nombre = p.Nombre,
            Descripcion = p.Descripcion,
            Precio = p.Precio,
            Stock = p.Stock,
            Activo = p.Activo,
            Version = p.Version,
            FechaModificacion = p.FechaModificacion
        };

        /// <summary>EventBy del historial: el usuario de la sesión (Authorize ya garantizó que hay una).</summary>
        private static Guid UsuarioActual() => SessionManager.GetInstance.User.Id;

        private bool Authorize(string action)
        {
            if (SessionManager.HasPermission(PermissionCode.GestionarProductos)) return true;

            bitacoraBLL.RegistrarError(NameEvent.AccesoNoAutorizado,
                $"Intento no autorizado de {action} (requiere {PermissionCode.GestionarProductos})", Priority.High);

            return false;
        }

        private static OperationResult Denied() => OperationResult.Fail("msg.noPermiso");

        /// <summary>
        /// <paramref name="action"/> devuelve false si el producto cambió desde que se leyó:
        /// no se escribió nada y se pide recargar.
        /// </summary>
        private OperationResult Execute(NameEvent nameEvent, string detail, Func<bool> action, string okKey,
            params object[] okArgs)
        {
            try
            {
                if (!action()) return OperationResult.Fail("msg.producto.desactualizado");

                bitacoraBLL.RegistrarEvento(nameEvent, detail, Priority.Medium);
                return OperationResult.Ok(okKey, okArgs);
            }
            catch (Exception ex)
            {
                RegistrarFalloSistema(detail, ex);
                return OperationResult.Fail("msg.generic.error");
            }
        }

        private void RegistrarFalloSistema(string detail, Exception ex)
        {
            if (SessionManager.IsLoggedIn())
                bitacoraBLL.RegistrarError(NameEvent.ErrorSistema, $"{detail}: {ex.Message}", Priority.High);
            System.Diagnostics.Debug.WriteLine(ex);
        }
    }
}
