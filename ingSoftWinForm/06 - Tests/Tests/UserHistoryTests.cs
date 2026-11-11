using System;
using System.Collections.Generic;
using System.Linq;
using BE.Entity;
using BE.Enum;
using BLL;
using Services;
using Xunit;
using static Tests.Samples;

namespace Tests
{
    [Collection("Session")]
    public class UserHistoryTests : IDisposable
    {
        private readonly FakeUserDAL userDAL = new();
        private readonly FakeBitacoraDAL bitacoraDAL = new();
        private readonly UserBLL userBLL;

        public UserHistoryTests()
        {
            if (SessionManager.IsLoggedIn()) SessionManager.Logout();
            userBLL = new UserBLL(userDAL, new BitacoraBLL(bitacoraDAL));
        }

        public void Dispose()
        {
            if (SessionManager.IsLoggedIn()) SessionManager.Logout();
        }

        public static TheoryData<string, string, string> CamposInvalidos => new()
        {
            { "NickName", new string('n', 51), "msg.usuario.nickInvalido" },
            { "FirstName", "", "msg.usuario.nombreInvalido" },
            { "FirstName", new string('n', 101), "msg.usuario.nombreInvalido" },
            { "LastName", "  ", "msg.usuario.apellidoInvalido" },
            { "Pais", new string('p', 101), "msg.usuario.paisInvalido" },
            { "Provincia", new string('p', 101), "msg.usuario.provinciaInvalida" },
            { "Direccion", new string('d', 201), "msg.usuario.direccionInvalida" },
        };

        private User LoginAdmin()
        {
            var admin = UserWith("admin", RoleWith("admin", 1, Simple(PermissionCode.GestionarUsuarios)));
            SessionManager.Login(admin);
            return admin;
        }

        private User Seed() =>
            userDAL.Sembrar(new User { Username = "lu", FirstName = "Lujan", LastName = "Netri", NickName = "lu_05" });

        private User Fresh(Guid id) => userDAL.GetById(id)!;

        [Fact]
        public void Register_CreaUnaSolaFilaDeHistorialVersion1Alta()
        {
            var result = userBLL.Register("pepe", "Clave1234", "Clave1234", "Jose", "Perez", null);

            Assert.True(result.Success);
            var user = Assert.Single(userDAL.Users);
            var historial = userBLL.GetHistorial(user.Id);
            var fila = Assert.Single(historial);
            Assert.Equal(1, fila.Version);
            Assert.Equal(TipoCambioUsuario.Alta, fila.TipoCambio);
        }

        [Fact]
        public void Modificar_NickNameYPais_ActualizaVersionYCreaFilaModificacion()
        {
            var seeded = Seed();
            var admin = LoginAdmin();
            var copia = Fresh(seeded.Id);
            copia.NickName = "lu_nuevo";
            copia.Pais = "Argentina";

            var result = userBLL.Modificar(copia);

            Assert.True(result.Success);
            Assert.Equal("msg.usuario.actualizado", result.MessageKey);
            Assert.Equal(2, copia.Version);

            var stored = Fresh(seeded.Id);
            Assert.Equal(2, stored.Version);
            Assert.Equal("lu_nuevo", stored.NickName);
            Assert.Equal("Argentina", stored.Pais);

            var fila = Assert.Single(userBLL.GetHistorial(seeded.Id).Where(h => h.Version == 2));
            Assert.Equal(TipoCambioUsuario.Modificacion, fila.TipoCambio);
            Assert.Equal("lu_nuevo", fila.NickName);
            Assert.Equal("Argentina", fila.Pais);
            Assert.Equal(admin.Id, fila.EventBy);
        }

        [Fact]
        public void Modificar_SinCambios_DevuelveSinCambiosYNoAgregaFila()
        {
            var seeded = Seed();
            LoginAdmin();
            var antes = userBLL.GetHistorial(seeded.Id).Count;

            var result = userBLL.Modificar(Fresh(seeded.Id));

            Assert.False(result.Success);
            Assert.Equal("msg.usuario.sinCambios", result.MessageKey);
            Assert.Equal(antes, userBLL.GetHistorial(seeded.Id).Count);
        }

        [Fact]
        public void Modificar_CopiaDesactualizada_DevuelveDesactualizado()
        {
            var seeded = Seed();
            LoginAdmin();
            var copiaA = Fresh(seeded.Id);
            var copiaB = Fresh(seeded.Id); // Version 1
            copiaA.NickName = "A";
            Assert.True(userBLL.Modificar(copiaA).Success); // now v2

            copiaB.NickName = "B";
            var result = userBLL.Modificar(copiaB);

            Assert.False(result.Success);
            Assert.Equal("msg.usuario.desactualizado", result.MessageKey);
            Assert.Equal(2, userBLL.GetHistorial(seeded.Id).Count);
        }

        [Fact]
        public void Modificar_CambioConcurrenteEnDAL_DevuelveDesactualizadoSinFila()
        {
            var seeded = Seed();
            LoginAdmin();
            var copia = Fresh(seeded.Id);
            copia.NickName = "X";
            userDAL.SimularCambioConcurrente = true;

            var result = userBLL.Modificar(copia);

            Assert.False(result.Success);
            Assert.Equal("msg.usuario.desactualizado", result.MessageKey);
            Assert.Single(userBLL.GetHistorial(seeded.Id));
        }

        [Fact]
        public void RestaurarVersion_CreaNuevaVersionCopiandoDatosAntiguos()
        {
            var seeded = Seed();
            LoginAdmin();
            var v1 = userBLL.GetHistorial(seeded.Id).Single(h => h.Version == 1);

            var c2 = Fresh(seeded.Id);
            c2.NickName = "dos";
            Assert.True(userBLL.Modificar(c2).Success); // v2

            var c3 = Fresh(seeded.Id);
            c3.Pais = "Chile";
            Assert.True(userBLL.Modificar(c3).Success); // v3

            var antes = userBLL.GetHistorial(seeded.Id);
            Assert.Equal(3, antes.Count);
            var snapshotAntes = antes
                .Select(h => (h.Version, h.TipoCambio, h.NickName, h.FirstName, h.LastName, h.Pais, h.Provincia, h.Direccion, h.RestauradaDesde))
                .ToList();

            var result = userBLL.RestaurarVersion(Fresh(seeded.Id), v1);

            Assert.True(result.Success);
            Assert.Equal("msg.usuario.restaurado", result.MessageKey);

            var despues = userBLL.GetHistorial(seeded.Id);
            Assert.Equal(4, despues.Count);

            var nueva = despues.Single(h => h.Version == 4);
            Assert.Equal(TipoCambioUsuario.Restauracion, nueva.TipoCambio);
            Assert.Equal(1, nueva.RestauradaDesde);
            Assert.Equal(v1.NickName, nueva.NickName);
            Assert.Equal(v1.FirstName, nueva.FirstName);
            Assert.Equal(v1.LastName, nueva.LastName);
            Assert.Equal(v1.Pais, nueva.Pais);
            Assert.Equal(v1.Provincia, nueva.Provincia);
            Assert.Equal(v1.Direccion, nueva.Direccion);
            Assert.Equal(4, Fresh(seeded.Id).Version);

            var primerasTres = despues.Take(3)
                .Select(h => (h.Version, h.TipoCambio, h.NickName, h.FirstName, h.LastName, h.Pais, h.Provincia, h.Direccion, h.RestauradaDesde))
                .ToList();
            Assert.Equal(snapshotAntes, primerasTres);
        }

        [Fact]
        public void RestaurarVersion_VersionActual_DevuelveVersionActual()
        {
            var seeded = Seed();
            LoginAdmin();
            var c2 = Fresh(seeded.Id);
            c2.NickName = "dos";
            Assert.True(userBLL.Modificar(c2).Success); // v2

            var actual = userBLL.GetHistorial(seeded.Id).Single(h => h.Version == 2);
            var result = userBLL.RestaurarVersion(Fresh(seeded.Id), actual);

            Assert.False(result.Success);
            Assert.Equal("msg.usuario.versionActual", result.MessageKey);
            Assert.Equal(2, userBLL.GetHistorial(seeded.Id).Count);
        }

        [Fact]
        public void RestaurarVersion_DatosIgualesAlActual_DevuelveSinCambios()
        {
            var seeded = Seed();
            LoginAdmin();
            var b = Fresh(seeded.Id);
            b.NickName = "B";
            Assert.True(userBLL.Modificar(b).Success); // v2 (B)

            var a = Fresh(seeded.Id);
            a.NickName = "lu_05";
            Assert.True(userBLL.Modificar(a).Success); // v3 (A again)

            var v1 = userBLL.GetHistorial(seeded.Id).Single(h => h.Version == 1);
            var result = userBLL.RestaurarVersion(Fresh(seeded.Id), v1);

            Assert.False(result.Success);
            Assert.Equal("msg.usuario.sinCambios", result.MessageKey);
            Assert.Equal(3, userBLL.GetHistorial(seeded.Id).Count);
        }

        [Fact]
        public void RestaurarVersion_VersionDeOtroUsuario_DevuelveErrorGenerico()
        {
            var seeded = Seed();
            var otro = userDAL.Sembrar(new User { Username = "otro", FirstName = "Otro", LastName = "Usuario", NickName = "otro" });
            LoginAdmin();
            var versionOtro = userBLL.GetHistorial(otro.Id).Single();

            var result = userBLL.RestaurarVersion(Fresh(seeded.Id), versionOtro);

            Assert.False(result.Success);
            Assert.Equal("msg.generic.error", result.MessageKey);
            Assert.Single(userBLL.GetHistorial(seeded.Id));
        }

        [Fact]
        public void SinPermiso_ModificarYRestaurar_DevuelveNoPermisoSinAgregarFilas()
        {
            var seeded = Seed();
            var invitado = UserWith("invitado", RoleWith("invitado", 2, Simple(PermissionCode.VerMiPerfil)));
            SessionManager.Login(invitado);
            var antes = userBLL.GetHistorial(seeded.Id).Count;

            var copia = Fresh(seeded.Id);
            copia.NickName = "cambio";
            var rModificar = userBLL.Modificar(copia);
            Assert.False(rModificar.Success);
            Assert.Equal("msg.noPermiso", rModificar.MessageKey);

            var v1 = userBLL.GetHistorial(seeded.Id).Single();
            var rRestaurar = userBLL.RestaurarVersion(Fresh(seeded.Id), v1);
            Assert.False(rRestaurar.Success);
            Assert.Equal("msg.noPermiso", rRestaurar.MessageKey);

            Assert.Equal(antes, userBLL.GetHistorial(seeded.Id).Count);
        }

        [Fact]
        public void CompararVersiones_MantieneOrdenYMarcaSoloCambiosReales()
        {
            var anterior = new UserHistory
            {
                NickName = "a", FirstName = "F", LastName = "L", Pais = null, Provincia = "P", Direccion = "D1",
                EventByUsername = "admin"
            };
            var actual = new UserHistory
            {
                NickName = "b", FirstName = "F", LastName = "L", Pais = "", Provincia = "P", Direccion = "D2",
                EventByUsername = "admin"
            };

            var filas = UserBLL.CompararVersiones(anterior, actual);

            Assert.Equal(
                new[] { "NickName", "FirstName", "LastName", "Pais", "Provincia", "Direccion" },
                filas.Select(f => f.Campo).ToArray());

            var nick = filas.Single(f => f.Campo == "NickName");
            Assert.True(nick.Cambio);
            Assert.Equal("a", nick.ValorAnterior);
            Assert.Equal("b", nick.ValorActual);

            Assert.False(filas.Single(f => f.Campo == "FirstName").Cambio);
            Assert.False(filas.Single(f => f.Campo == "LastName").Cambio);
            Assert.False(filas.Single(f => f.Campo == "Pais").Cambio); // null vs "" son iguales
            Assert.False(filas.Single(f => f.Campo == "Provincia").Cambio);
            Assert.True(filas.Single(f => f.Campo == "Direccion").Cambio);
        }

        [Fact]
        public void CompararVersiones_AnteriorNull_NoMarcaCambiosYValorAnteriorNull()
        {
            var actual = new UserHistory
            {
                NickName = "b", FirstName = "F", LastName = "L", Pais = "Chile", Provincia = "P", Direccion = "D",
                EventByUsername = "admin"
            };

            var filas = UserBLL.CompararVersiones(null, actual);

            Assert.Equal(6, filas.Count);
            Assert.All(filas, f => Assert.False(f.Cambio));
            Assert.All(filas, f => Assert.Null(f.ValorAnterior));
        }

        [Fact]
        public void DescribirVersion_Alta_DevuelveTipoAlta()
        {
            var actual = new UserHistory { Version = 1, TipoCambio = TipoCambioUsuario.Alta, EventByUsername = "admin" };

            var detalle = UserBLL.DescribirVersion(null, actual);

            Assert.Equal(TipoDetalleVersion.Alta, detalle.Tipo);
        }

        [Fact]
        public void DescribirVersion_Modificacion_ListaSoloCamposCambiados()
        {
            var anterior = new UserHistory
            {
                Version = 1, TipoCambio = TipoCambioUsuario.Alta, NickName = "a", Pais = null,
                FirstName = "F", LastName = "L", EventByUsername = "admin"
            };
            var actual = new UserHistory
            {
                Version = 2, TipoCambio = TipoCambioUsuario.Modificacion, NickName = "b", Pais = "Chile",
                FirstName = "F", LastName = "L", EventByUsername = "admin"
            };

            var detalle = UserBLL.DescribirVersion(anterior, actual);

            Assert.Equal(TipoDetalleVersion.Modificacion, detalle.Tipo);
            Assert.Equal(new[] { "NickName", "Pais" }, detalle.Campos.ToArray());
        }

        [Fact]
        public void DescribirVersion_Restauracion_IncluyeRestauradaDesde()
        {
            var anterior = new UserHistory { Version = 3, TipoCambio = TipoCambioUsuario.Modificacion, NickName = "x", EventByUsername = "admin" };
            var actual = new UserHistory
            {
                Version = 4, TipoCambio = TipoCambioUsuario.Restauracion, RestauradaDesde = 2, NickName = "y",
                EventByUsername = "admin"
            };

            var detalle = UserBLL.DescribirVersion(anterior, actual);

            Assert.Equal(TipoDetalleVersion.Restauracion, detalle.Tipo);
            Assert.Equal(2, detalle.RestauradaDesde);
        }

        [Theory]
        [MemberData(nameof(CamposInvalidos))]
        public void Modificar_ValidacionFalla_DevuelveClaveYNoAgregaFila(string campo, string valor, string claveEsperada)
        {
            var seeded = Seed();
            LoginAdmin();
            var copia = Fresh(seeded.Id);

            switch (campo)
            {
                case "NickName": copia.NickName = valor; break;
                case "FirstName": copia.FirstName = valor; break;
                case "LastName": copia.LastName = valor; break;
                case "Pais": copia.Pais = valor; break;
                case "Provincia": copia.Provincia = valor; break;
                case "Direccion": copia.Direccion = valor; break;
                default: throw new ArgumentException($"Campo desconocido: {campo}", nameof(campo));
            }

            var result = userBLL.Modificar(copia);

            Assert.False(result.Success);
            Assert.Equal(claveEsperada, result.MessageKey);
            Assert.Single(userBLL.GetHistorial(seeded.Id));
        }
    }
}
