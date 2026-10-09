using BE.Entity;
using BE.Enum;
using BLL;
using Services;
using Xunit;
using static Tests.Samples;

namespace Tests
{
    [Collection("Session")]
    public class IdiomaBLLTests : IDisposable
    {
        private const int Es = 1;
        private const int En = 2;

        private readonly FakeIdiomaDAL dal = new();
        private readonly FakeBitacoraDAL bitacoraDAL = new();
        private readonly IdiomaBLL bll;

        public IdiomaBLLTests()
        {
            if (SessionManager.IsLoggedIn()) SessionManager.Logout();

            dal.Texto("btn.ok", Es, "Aceptar");
            dal.Texto("btn.ok", En, "OK");
            dal.Texto("btn.cancel", Es, "Cancelar");           // sin traducción al inglés
            dal.Texto("msg.hello", Es, "Hola {0}");
            dal.Texto("msg.hello", En, "Hello {0}");

            bll = new IdiomaBLL(dal, new BitacoraBLL(bitacoraDAL));
        }

        public void Dispose()
        {
            if (SessionManager.IsLoggedIn()) SessionManager.Logout();
        }

        private static User Admin()
        {
            var admin = UserWith("admin",
                RoleWith("administrador", 1, Simple(PermissionCode.GestionarIdiomas)));
            admin.Id = Guid.NewGuid();
            return admin;
        }

        // ---- Traducción y fallback ----

        [Fact]
        public void Traducir_DefaultLanguage_IsTranslated()
        {
            var leyenda = bll.Traducir("btn.ok");

            Assert.Equal("Aceptar", leyenda.Texto);
            Assert.True(leyenda.Traducido);
            Assert.True(leyenda.Existe);
        }

        [Fact]
        public void Traducir_TranslatedLabel_UsesActiveLanguage()
        {
            bll.CambiarIdioma("en");

            var leyenda = bll.Traducir("btn.ok");

            Assert.Equal("OK", leyenda.Texto);
            Assert.True(leyenda.Traducido);
        }

        [Fact]
        public void Traducir_MissingTranslation_FallsBackToDefaultAndFlagsIt()
        {
            bll.CambiarIdioma("en");

            var leyenda = bll.Traducir("btn.cancel");

            Assert.Equal("Cancelar", leyenda.Texto);
            Assert.False(leyenda.Traducido);
            Assert.True(leyenda.Existe);
        }

        [Fact]
        public void Traducir_UnknownKey_ReturnsKeyAndDoesNotExist()
        {
            var leyenda = bll.Traducir("no.existe");

            Assert.Equal("no.existe", leyenda.Texto);
            Assert.False(leyenda.Existe);
        }

        [Fact]
        public void Traducir_FormatsArguments()
        {
            bll.CambiarIdioma("en");

            Assert.Equal("Hello Ana", bll.Traducir("msg.hello", "Ana").Texto);
        }

        [Fact]
        public void Traducir_BadFormatText_ReturnsRawTextInsteadOfThrowing()
        {
            dal.Texto("msg.bad", Es, "Llave { rota");

            Assert.Equal("Llave { rota", bll.Traducir("msg.bad", "x").Texto);
        }

        [Fact]
        public void Traducir_OperationResult_UsesItsKeyAndArgs()
        {
            var resultado = OperationResult.Ok("msg.hello", "Luis");

            Assert.Equal("Hola Luis", bll.Traducir(resultado).Texto);
        }

        [Fact]
        public void CambiarIdioma_LoadsTextsOncePerLanguage_NotPerLabel()
        {
            bll.CambiarIdioma("en");
            var antes = dal.GetTextosCalls;

            for (var i = 0; i < 20; i++) bll.Traducir("btn.ok");

            Assert.Equal(antes, dal.GetTextosCalls);
        }

        // ---- Observer ----

        [Fact]
        public void CambiarIdioma_NotifiesEverySuscriberOnce()
        {
            var a = new SuscriberEspia();
            var b = new SuscriberEspia();
            bll.suscribe(a);
            bll.suscribe(b);

            var resultado = bll.CambiarIdioma("en");

            Assert.True(resultado.Success);
            Assert.Equal(["en"], a.Recibidos);
            Assert.Equal(["en"], b.Recibidos);
        }

        [Fact]
        public void Unsuscribed_IsNotNotified()
        {
            var a = new SuscriberEspia();
            var b = new SuscriberEspia();
            bll.suscribe(a);
            bll.suscribe(b);
            bll.unsuscribe(a);

            bll.CambiarIdioma("en");

            Assert.Empty(a.Recibidos);
            Assert.Single(b.Recibidos);
        }

        [Fact]
        public void Suscribe_Twice_Throws()
        {
            var a = new SuscriberEspia();
            bll.suscribe(a);

            Assert.Throws<Exception>(() => bll.suscribe(a));
        }

        [Fact]
        public void Unsuscribe_NotSubscribed_Throws()
            => Assert.Throws<Exception>(() => bll.unsuscribe(new SuscriberEspia()));

        [Fact]
        public void Suscribers_IsReadOnlyView()
        {
            var a = new SuscriberEspia();
            bll.suscribe(a);

            Assert.Same(a, Assert.Single(bll.suscribers));
        }

        [Fact]
        public void Notify_SuscriberThatUnsuscribesItself_DoesNotBreakTheLoop()
        {
            var otro = new SuscriberEspia();
            bll.suscribe(new AutoDesuscripcion(bll));
            bll.suscribe(otro);

            bll.CambiarIdioma("en");

            Assert.Single(otro.Recibidos);
        }

        private class AutoDesuscripcion(IdiomaBLL publisher) : BE.Observer.ISuscriberIdioma
        {
            public void Actualizar(Idioma idiomaActivo) => publisher.unsuscribe(this);
        }

        // ---- Cambio de idioma ----

        [Fact]
        public void CambiarIdioma_Unknown_FailsAndDoesNotNotify()
        {
            var a = new SuscriberEspia();
            bll.suscribe(a);

            var resultado = bll.CambiarIdioma("xx");

            Assert.False(resultado.Success);
            Assert.Empty(a.Recibidos);
        }

        [Fact]
        public void CambiarIdioma_Inactive_Fails()
        {
            dal.GetByCodigo("en")!.Activo = false;

            Assert.False(bll.CambiarIdioma("en").Success);
        }

        [Fact]
        public void CambiarIdioma_LoggedIn_PersistsInUserAndAudits()
        {
            var user = Admin();
            SessionManager.Login(user);

            bll.CambiarIdioma("en");

            Assert.Equal(En, dal.IdiomaPorUsuario[user.Id]);
            Assert.Equal(En, user.IdIdioma);
            Assert.Contains(bitacoraDAL.Registros, b => b.NameEvent == NameEvent.CambiarIdioma);
        }

        [Fact]
        public void CambiarIdioma_NotLoggedIn_DoesNotPersist()
        {
            var resultado = bll.CambiarIdioma("en");

            Assert.True(resultado.Success);
            Assert.Empty(dal.IdiomaPorUsuario);
        }

        [Fact]
        public void AplicarIdiomaDe_UserWithSavedLanguage_UsesIt()
        {
            var user = Admin();
            user.IdIdioma = En;

            bll.AplicarIdiomaDe(user);

            Assert.Equal("en", bll.IdiomaActivo!.Codigo);
        }

        [Fact]
        public void AplicarIdiomaDe_UserWithoutSavedLanguage_KeepsCurrentOne()
        {
            bll.CambiarIdioma("en");

            bll.AplicarIdiomaDe(Admin());

            Assert.Equal("en", bll.IdiomaActivo!.Codigo);
        }

        // ---- Administración ----

        [Fact]
        public void CrearIdioma_WithoutPermission_IsDeniedAndAudited()
        {
            SessionManager.Login(UserWith("invitado", RoleWith("invitado", 2, Simple(PermissionCode.VerMiPerfil))));

            var resultado = bll.CrearIdioma("pt", "Português");

            Assert.False(resultado.Success);
            Assert.Equal("msg.noPermiso", resultado.MessageKey);
            Assert.DoesNotContain(dal.Idiomas, i => i.Codigo == "pt");
            Assert.Contains(bitacoraDAL.Registros, b => b.NameEvent == NameEvent.AccesoNoAutorizado);
        }

        [Fact]
        public void CrearIdioma_AsAdmin_CreatesActiveNonDefaultAndAudits()
        {
            SessionManager.Login(Admin());

            var resultado = bll.CrearIdioma(" PT ", " Português ");

            Assert.True(resultado.Success);
            var pt = Assert.Single(dal.Idiomas, i => i.Codigo == "pt");
            Assert.Equal("Português", pt.Nombre);
            Assert.True(pt.Activo);
            Assert.False(pt.EsDefault);
            Assert.Contains(bitacoraDAL.Registros, b => b.NameEvent == NameEvent.CrearIdioma);
        }

        [Theory]
        [InlineData("")]
        [InlineData("p")]
        [InlineData("portugues")]
        [InlineData("p1")]
        public void CrearIdioma_InvalidCode_Fails(string codigo)
        {
            SessionManager.Login(Admin());

            Assert.Equal("msg.idioma.codigoInvalido", bll.CrearIdioma(codigo, "X").MessageKey);
        }

        [Fact]
        public void CrearIdioma_DuplicateCode_Fails()
        {
            SessionManager.Login(Admin());

            Assert.Equal("msg.idioma.yaExiste", bll.CrearIdioma("en", "Otro").MessageKey);
        }

        [Fact]
        public void ActualizarIdioma_DeactivatingDefault_Fails()
        {
            SessionManager.Login(Admin());

            var resultado = bll.ActualizarIdioma(dal.GetDefault()!, "Español", activoNuevo: false);

            Assert.Equal("msg.idioma.defaultNoSeDesactiva", resultado.MessageKey);
        }

        [Fact]
        public void GuardarTraduccion_AsAdmin_SavesAndFlagsAsTranslated()
        {
            SessionManager.Login(Admin());
            var en = dal.GetByCodigo("en")!;
            var item = bll.GetTraducciones(en).Single(t => t.Clave == "btn.cancel");
            Assert.False(item.Traducido);

            var resultado = bll.GuardarTraduccion(en, item, "Cancel");

            Assert.True(resultado.Success);
            Assert.True(item.Traducido);
            Assert.Equal("Cancel", dal.Textos[("btn.cancel", En)]);
            Assert.Contains(bitacoraDAL.Registros, b => b.NameEvent == NameEvent.ActualizarTraduccion);
        }

        [Fact]
        public void GuardarTraduccion_EmptyText_RemovesTranslation()
        {
            SessionManager.Login(Admin());
            var en = dal.GetByCodigo("en")!;
            var item = bll.GetTraducciones(en).Single(t => t.Clave == "btn.ok");

            bll.GuardarTraduccion(en, item, "  ");

            Assert.False(item.Traducido);
            Assert.False(dal.Textos.ContainsKey(("btn.ok", En)));
        }

        [Fact]
        public void GuardarTraduccion_OnActiveLanguage_RefreshesAndNotifies()
        {
            SessionManager.Login(Admin());
            bll.CambiarIdioma("en");
            var espia = new SuscriberEspia();
            bll.suscribe(espia);
            var en = dal.GetByCodigo("en")!;
            var item = bll.GetTraducciones(en).Single(t => t.Clave == "btn.cancel");

            bll.GuardarTraduccion(en, item, "Cancel");

            Assert.Equal("Cancel", bll.Traducir("btn.cancel").Texto);
            Assert.True(bll.Traducir("btn.cancel").Traducido);
            Assert.Single(espia.Recibidos);
        }

        [Fact]
        public void GuardarTraduccion_WithoutPermission_IsDenied()
        {
            var en = dal.GetByCodigo("en")!;
            var item = bll.GetTraducciones(en).First();

            var resultado = bll.GuardarTraduccion(en, item, "x");

            Assert.Equal("msg.noPermiso", resultado.MessageKey);
        }
    }
}
