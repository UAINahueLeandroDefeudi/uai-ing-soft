using BE.Entity;
using BE.Enum;
using BLL;
using Services;
using Xunit;
using static Tests.Samples;

namespace Tests
{
    /// <summary>T06b: control de cambios de productos (historial con la fila completa por versión).</summary>
    [Collection("Session")]
    public class ProductoBLLTests : IDisposable
    {
        private readonly FakeProductoDAL dal = new();
        private readonly FakeBitacoraDAL bitacoraDAL = new();
        private readonly ProductoBLL bll;
        private readonly User admin;

        public ProductoBLLTests()
        {
            if (SessionManager.IsLoggedIn()) SessionManager.Logout();

            admin = UserWith("admin", RoleWith("administrador", 1, Simple(PermissionCode.GestionarProductos)));
            admin.Id = Guid.NewGuid();
            SessionManager.Login(admin);

            bll = new ProductoBLL(dal, new BitacoraBLL(bitacoraDAL));
        }

        public void Dispose()
        {
            if (SessionManager.IsLoggedIn()) SessionManager.Logout();
        }

        private Producto Crear(string codigo = "p-1", decimal precio = 10.50m, int stock = 5)
        {
            Assert.True(bll.Crear(codigo, "Yerba", "1 kg", precio, stock).Success);
            return dal.GetAll().Single(p => p.Codigo == codigo.ToUpperInvariant());
        }

        // ---- Alta ----

        [Fact]
        public void Crear_GuardaVersion1ConLaFilaCompletaQuienYCuando()
        {
            var antes = DateTime.Now;
            var producto = Crear();

            Assert.Equal("P-1", producto.Codigo);
            Assert.Equal(1, producto.Version);

            var version = Assert.Single(dal.Historial);
            Assert.Equal(OperacionProducto.Alta, version.Operacion);
            Assert.Equal(1, version.Version);
            Assert.Equal(admin.Id, version.EventBy);
            Assert.InRange(version.EventAt, antes, DateTime.Now);
            Assert.Equal(("P-1", "Yerba", "1 kg", 10.50m, 5, true),
                (version.Codigo, version.Nombre, version.Descripcion, version.Precio, version.Stock, version.Activo));
            Assert.Contains(bitacoraDAL.Registros, r => r.NameEvent == NameEvent.CrearProducto);
        }

        [Fact]
        public void Crear_CodigoRepetido_SeRechaza()
        {
            Crear("p-1");

            var resultado = bll.Crear("P-1", "Otro", null, 1, 1);

            Assert.False(resultado.Success);
            Assert.Equal("msg.producto.codigoExiste", resultado.MessageKey);
            Assert.Single(dal.Productos);
        }

        [Theory]
        [InlineData("", "Nombre", 1, 1, "msg.producto.codigoInvalido")]
        [InlineData("P-1", " ", 1, 1, "msg.producto.nombreInvalido")]
        [InlineData("P-1", "Nombre", -1, 1, "msg.producto.precioInvalido")]
        [InlineData("P-1", "Nombre", 1.005, 1, "msg.producto.precioInvalido")]
        [InlineData("P-1", "Nombre", 1, -1, "msg.producto.stockInvalido")]
        public void Crear_DatosInvalidos_NoGuardaNada(string codigo, string nombre, double precio, int stock, string clave)
        {
            var resultado = bll.Crear(codigo, nombre, null, (decimal)precio, stock);

            Assert.False(resultado.Success);
            Assert.Equal(clave, resultado.MessageKey);
            Assert.Empty(dal.Productos);
            Assert.Empty(dal.Historial);
        }

        // ---- Modificación y baja ----

        [Fact]
        public void Modificar_AgregaUnaVersionNuevaYConservaLaAnterior()
        {
            var producto = Crear(precio: 10m);

            var resultado = bll.Modificar(producto, "P-1", "Yerba", "1 kg", 12.25m, 5);

            Assert.True(resultado.Success);
            Assert.Equal(2, dal.GetById(producto.Id)!.Version);
            var historial = dal.GetHistorial(producto.Id);
            Assert.Equal(new[] { 2, 1 }, historial.Select(h => h.Version));
            Assert.Equal(OperacionProducto.Modificacion, historial[0].Operacion);
            Assert.Equal(12.25m, historial[0].Precio);
            Assert.Equal(10m, historial[1].Precio);
        }

        [Fact]
        public void Modificar_SinCambios_NoGeneraVersion()
        {
            var producto = Crear();

            var resultado = bll.Modificar(producto, "p-1", "Yerba", "1 kg", 10.50m, 5);

            Assert.Equal("msg.producto.sinCambios", resultado.MessageKey);
            Assert.Single(dal.Historial);
        }

        [Fact]
        public void Modificar_ProductoCambiadoPorOtroUsuario_NoPisaNada()
        {
            var producto = Crear();
            dal.SimularCambioConcurrente = true;

            var resultado = bll.Modificar(producto, "P-1", "Yerba", "1 kg", 99m, 5);

            Assert.Equal("msg.producto.desactualizado", resultado.MessageKey);
            Assert.Single(dal.Historial);
            Assert.DoesNotContain(bitacoraDAL.Registros, r => r.NameEvent == NameEvent.ModificarProducto);
        }

        [Fact]
        public void Modificar_RechazadoPorValidacion_NoCambiaElObjetoDeLaUI()
        {
            var producto = Crear();

            bll.Modificar(producto, "P-1", "Otro nombre", null, -5m, 5);

            Assert.Equal("Yerba", producto.Nombre);
        }

        [Fact]
        public void DarDeBaja_EsLogicaYQuedaEnElHistorial()
        {
            var producto = Crear();

            Assert.True(bll.DarDeBaja(producto).Success);

            var guardado = dal.GetById(producto.Id)!;
            Assert.False(guardado.Activo);
            Assert.Equal(OperacionProducto.Baja, dal.GetHistorial(producto.Id)[0].Operacion);
            Assert.Equal("msg.producto.yaDadoDeBaja", bll.DarDeBaja(guardado).MessageKey);
        }

        // ---- Restaurar una versión anterior ----

        [Fact]
        public void RestaurarVersion_AplicaLaFotoViejaComoVersionNueva()
        {
            var producto = Crear(precio: 10m, stock: 5);
            bll.Modificar(producto, "P-1", "Yerba premium", "1 kg", 15m, 3);
            producto = dal.GetById(producto.Id)!;
            bll.DarDeBaja(producto);
            producto = dal.GetById(producto.Id)!;
            var version1 = dal.GetHistorial(producto.Id).Single(h => h.Version == 1);

            var resultado = bll.RestaurarVersion(producto, version1);

            Assert.True(resultado.Success);
            var restaurado = dal.GetById(producto.Id)!;
            Assert.Equal(4, restaurado.Version);
            Assert.Equal(("Yerba", 10m, 5, true),
                (restaurado.Nombre, restaurado.Precio, restaurado.Stock, restaurado.Activo));

            // El historial no se reescribe: las cuatro versiones siguen ahí.
            var historial = dal.GetHistorial(producto.Id);
            Assert.Equal(new[] { 4, 3, 2, 1 }, historial.Select(h => h.Version));
            Assert.Equal(OperacionProducto.Restauracion, historial[0].Operacion);
            Assert.Equal("Yerba premium", historial[2].Nombre);
        }

        [Fact]
        public void RestaurarVersion_LaVersionActual_SeRechaza()
        {
            var producto = Crear();
            var actual = dal.GetHistorial(producto.Id).Single();

            Assert.Equal("msg.producto.versionActual", bll.RestaurarVersion(producto, actual).MessageKey);
        }

        [Fact]
        public void RestaurarVersion_CodigoTomadoPorOtroProducto_SeRechaza()
        {
            var producto = Crear("A");
            bll.Modificar(producto, "B", "Yerba", "1 kg", 10.50m, 5);
            Crear("A");
            producto = dal.GetAll().Single(p => p.Codigo == "B");
            var version1 = dal.GetHistorial(producto.Id).Single(h => h.Version == 1);

            var resultado = bll.RestaurarVersion(producto, version1);

            Assert.Equal("msg.producto.codigoExiste", resultado.MessageKey);
        }

        // ---- Permisos ----

        [Fact]
        public void SinPermiso_SeRechazaYQuedaEnBitacora()
        {
            SessionManager.Logout();
            SessionManager.Login(UserWith("invitado", RoleWith("invitado", 2, Simple(PermissionCode.VerMiPerfil))));

            var resultado = bll.Crear("P-1", "Yerba", null, 1, 1);

            Assert.Equal("msg.noPermiso", resultado.MessageKey);
            Assert.Empty(dal.Productos);
            Assert.Contains(bitacoraDAL.Registros, r => r.NameEvent == NameEvent.AccesoNoAutorizado);
        }
    }
}
