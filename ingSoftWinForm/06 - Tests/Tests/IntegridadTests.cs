using BE.Entity;
using BE.Enum;
using BLL;
using DAL;
using Services;
using Xunit;
using static Tests.Samples;

namespace Tests
{
    /// <summary>T07: DAL en memoria con filas por tabla y los DV "almacenados".</summary>
    public class FakeDigitoVerificadorDAL : IDigitoVerificadorDAL
    {
        private readonly DigitoVerificador calculador = new();

        public Dictionary<string, List<Dictionary<string, object?>>> Tablas { get; } = new();
        public Dictionary<string, string> DVVs { get; } = new();
        public bool Caida { get; set; }

        public List<Dictionary<string, object?>> LeerFilas(EntidadVerificable entidad)
        {
            if (Caida) throw new InvalidOperationException("BD no disponible");
            return Tablas.TryGetValue(entidad.Tabla, out var filas) ? filas : [];
        }

        public string? LeerDVV(string tabla) => DVVs.GetValueOrDefault(tabla);

        /// <summary>Alta legítima: fila con su DVH y DVV actualizado (lo que hace DigitoVerificadorDAL.Escribir).</summary>
        public void AltaLegitima(EntidadVerificable entidad, int n)
        {
            var fila = entidad.Atributos.ToDictionary(a => a, a => (object?)$"{a}-{n}");
            fila["DVH"] = calculador.CalcularDVH(entidad, fila);

            if (!Tablas.TryGetValue(entidad.Tabla, out var filas)) Tablas[entidad.Tabla] = filas = [];
            filas.Add(fila);

            RecalcularDVV(entidad);
        }

        public void RecalcularDVV(EntidadVerificable entidad)
            => DVVs[entidad.Tabla] = calculador.CalcularDVV(entidad.Tabla,
                Tablas[entidad.Tabla].Select(f => (calculador.ClaveTexto(entidad, f), f["DVH"] as string)));
    }

    // Verificar() escribe el IntegridadManager estático: no puede correr en paralelo con quien lo lee.
    [Collection("Session")]
    public class IntegridadTests
    {
        // Entidad declarada solo acá: demuestra que el mecanismo no tiene código por tabla.
        private static readonly EntidadVerificable TablaNueva =
            new("TablaNueva", ["Codigo"], ["Codigo", "Nombre", "Apellido"]);

        public static IEnumerable<object[]> Entidades()
        {
            foreach (var e in EntidadesVerificables.Todas) yield return [e];
            yield return [TablaNueva];
        }

        private static (IntegridadBLL Bll, FakeDigitoVerificadorDAL Dal) Armar(EntidadVerificable entidad)
        {
            var dal = new FakeDigitoVerificadorDAL();
            for (var i = 1; i <= 3; i++) dal.AltaLegitima(entidad, i);
            return (new IntegridadBLL(dal, [entidad]), dal);
        }

        [Theory, MemberData(nameof(Entidades))]
        public void SinCambios_NoHayErrores(EntidadVerificable entidad)
        {
            var (bll, _) = Armar(entidad);

            Assert.True(bll.Verificar().EsValido);
        }

        [Theory, MemberData(nameof(Entidades))]
        public void ModificarAtributo_DetectaDVH(EntidadVerificable entidad)
        {
            var (bll, dal) = Armar(entidad);
            dal.Tablas[entidad.Tabla][1][entidad.Atributos.Last()] = "alterado";

            var errores = bll.Verificar().Errores;

            Assert.Single(errores, e => e.Tipo == TipoErrorIntegridad.DVH);
        }

        [Theory, MemberData(nameof(Entidades))]
        public void IntercambiarAtributosDeUnaFila_DetectaDVH(EntidadVerificable entidad)
        {
            // Mismos valores en otra posición de atributo: el índice del atributo entra al cálculo.
            var (bll, dal) = Armar(entidad);
            var fila = dal.Tablas[entidad.Tabla][0];
            var primero = entidad.Atributos[0];
            var ultimo = entidad.Atributos[^1];
            (fila[primero], fila[ultimo]) = (fila[ultimo], fila[primero]);

            Assert.Contains(bll.Verificar().Errores, e => e.Tipo == TipoErrorIntegridad.DVH);
        }

        [Theory, MemberData(nameof(Entidades))]
        public void AltaExterna_DetectaDVHyDVV(EntidadVerificable entidad)
        {
            var (bll, dal) = Armar(entidad);
            // Alta por fuera del sistema: sin DVH y sin actualizar el DVV.
            var intrusa = entidad.Atributos.ToDictionary(a => a, a => (object?)$"{a}-externo");
            intrusa["DVH"] = null;
            dal.Tablas[entidad.Tabla].Add(intrusa);

            var errores = bll.Verificar().Errores;

            Assert.Contains(errores, e => e.Tipo == TipoErrorIntegridad.DVH);
            Assert.Contains(errores, e => e.Tipo == TipoErrorIntegridad.DVV);
        }

        [Theory, MemberData(nameof(Entidades))]
        public void BajaExterna_DetectaDVV(EntidadVerificable entidad)
        {
            var (bll, dal) = Armar(entidad);
            dal.Tablas[entidad.Tabla].RemoveAt(0);

            Assert.Contains(bll.Verificar().Errores, e => e.Tipo == TipoErrorIntegridad.DVV);
        }

        [Theory, MemberData(nameof(Entidades))]
        public void IntercambioDeDVHEntreFilas_Detecta(EntidadVerificable entidad)
        {
            var (bll, dal) = Armar(entidad);
            var filas = dal.Tablas[entidad.Tabla];
            (filas[0]["DVH"], filas[1]["DVH"]) = (filas[1]["DVH"], filas[0]["DVH"]);

            Assert.Equal(2, bll.Verificar().Errores.Count(e => e.Tipo == TipoErrorIntegridad.DVH));
        }

        [Theory, MemberData(nameof(Entidades))]
        public void DVVAlterado_FilasIntactas_DetectaSoloDVV(EntidadVerificable entidad)
        {
            var (bll, dal) = Armar(entidad);
            dal.DVVs[entidad.Tabla] = new string('0', 64);

            var errores = bll.Verificar().Errores;

            Assert.Single(errores, e => e.Tipo == TipoErrorIntegridad.DVV);
            Assert.DoesNotContain(errores, e => e.Tipo == TipoErrorIntegridad.DVH);
        }

        [Theory, MemberData(nameof(Entidades))]
        public void AtacanteRecalculaDVV_PeroNoElDVH_SigueDetectando(EntidadVerificable entidad)
        {
            var (bll, dal) = Armar(entidad);
            dal.Tablas[entidad.Tabla][1][entidad.Atributos.Last()] = "alterado";
            dal.RecalcularDVV(entidad);

            var errores = bll.Verificar().Errores;

            Assert.Single(errores, e => e.Tipo == TipoErrorIntegridad.DVH);
            Assert.DoesNotContain(errores, e => e.Tipo == TipoErrorIntegridad.DVV);
        }

        [Theory, MemberData(nameof(Entidades))]
        public void FilaReemplazada_MismaCantidad_Detecta(EntidadVerificable entidad)
        {
            var (bll, dal) = Armar(entidad);
            var filas = dal.Tablas[entidad.Tabla];
            var intrusa = entidad.Atributos.ToDictionary(a => a, a => (object?)$"{a}-reemplazo");
            intrusa["DVH"] = null;
            filas.RemoveAt(0);
            filas.Add(intrusa);

            var errores = bll.Verificar().Errores;

            Assert.Contains(errores, e => e.Tipo == TipoErrorIntegridad.DVH);
            Assert.Contains(errores, e => e.Tipo == TipoErrorIntegridad.DVV);
        }

        [Theory, MemberData(nameof(Entidades))]
        public void TablaVaciada_DetectaDVV(EntidadVerificable entidad)
        {
            var (bll, dal) = Armar(entidad);
            dal.Tablas[entidad.Tabla].Clear();

            Assert.Contains(bll.Verificar().Errores, e => e.Tipo == TipoErrorIntegridad.DVV);
        }

        [Theory, MemberData(nameof(Entidades))]
        public void SinDVVRegistrado_DetectaDVV(EntidadVerificable entidad)
        {
            var (bll, dal) = Armar(entidad);
            dal.DVVs.Remove(entidad.Tabla);

            Assert.Contains(bll.Verificar().Errores, e => e.Tipo == TipoErrorIntegridad.DVV);
        }

        [Fact]
        public void CorrupcionEnUnaTabla_NoMarcaLasOtras()
        {
            var dal = new FakeDigitoVerificadorDAL();
            foreach (var entidad in EntidadesVerificables.Todas)
                for (var i = 1; i <= 3; i++) dal.AltaLegitima(entidad, i);
            var bll = new IntegridadBLL(dal, EntidadesVerificables.Todas);

            var afectada = EntidadesVerificables.UserHistory;
            dal.Tablas[afectada.Tabla][0][afectada.Atributos.Last()] = "alterado";

            var errores = bll.Verificar().Errores;

            Assert.NotEmpty(errores);
            Assert.All(errores, e => Assert.Equal(afectada.Tabla, e.Tabla));
        }

        [Fact]
        public void NuloYCadenaVacia_ProducenDVHDistintos()
        {
            var entidad = new EntidadVerificable("T", ["A"], ["A", "B"]);
            var calculador = new DigitoVerificador();

            var conNulo = calculador.CalcularDVH(entidad, new Dictionary<string, object?> { ["A"] = 1, ["B"] = null });
            var conVacio = calculador.CalcularDVH(entidad, new Dictionary<string, object?> { ["A"] = 1, ["B"] = "" });

            Assert.NotEqual(conNulo, conVacio);
        }

        [Fact]
        public void BDNoDisponible_NoSeInformaComoCorrupcion()
        {
            var dal = new FakeDigitoVerificadorDAL { Caida = true };

            var resultado = new IntegridadBLL(dal).Verificar();

            Assert.False(resultado.EsValido);
            Assert.True(resultado.SoloNoVerificable);
        }

        [Fact]
        public void Verificar_NoModificaLosDigitos()
        {
            var entidad = EntidadesVerificables.UserHistory;
            var (bll, dal) = Armar(entidad);
            dal.Tablas[entidad.Tabla][0]["Detail"] = "alterado";
            var dvhAntes = dal.Tablas[entidad.Tabla].Select(f => f["DVH"]).ToList();
            var dvvAntes = dal.DVVs[entidad.Tabla];

            bll.Verificar();
            bll.Verificar();

            Assert.Equal(dvhAntes, dal.Tablas[entidad.Tabla].Select(f => f["DVH"]).ToList());
            Assert.Equal(dvvAntes, dal.DVVs[entidad.Tabla]);
        }

        [Fact]
        public void FormatoCanonico_EsEstable()
        {
            // Si este valor cambia, sql/08 y C# dejaron de coincidir: revisar ambos.
            var entidad = new EntidadVerificable("T", ["A"], ["A", "B", "C"]);
            var fila = new Dictionary<string, object?> { ["A"] = 1, ["B"] = null, ["C"] = "hola" };

            var dvh = new DigitoVerificador().CalcularDVH(entidad, fila);

            Assert.Equal(64, dvh.Length);
            Assert.Equal(dvh, new DigitoVerificador().CalcularDVH(entidad, fila));
        }
    }

    /// <summary>T07: qué ve cada rol cuando la integridad falló (SessionManager es estático).</summary>
    [Collection("Session")]
    public class LoginIntegridadTests : IDisposable
    {
        public LoginIntegridadTests()
        {
            if (SessionManager.IsLoggedIn()) SessionManager.Logout();
        }

        public void Dispose()
        {
            if (SessionManager.IsLoggedIn()) SessionManager.Logout();
            IntegridadManager.Establecer(null);
        }

        private static SessionBLL ArmarLogin(string rol, bool integridadOk)
        {
            var salt = HashManager.GenerateSalt();
            var user = new User
            {
                Id = Guid.NewGuid(),
                Username = "u",
                FirstName = "N",
                LastName = "A",
                IsActive = true,
                Salt = salt,
                PasswordHash = HashManager.HashPassword("clave", salt)
            };

            var userDAL = new FakeUserDAL();
            userDAL.Users.Add(user);

            var roleDAL = new FakeRoleDAL();
            roleDAL.Roles.Add(RoleWith(rol, 1));
            roleDAL.UserRoles[user.Id] = [1];

            var resultado = new ResultadoIntegridad();
            if (!integridadOk)
                resultado.Errores.Add(new ErrorIntegridad { Tabla = "UserHistory", Clave = "1", Tipo = TipoErrorIntegridad.DVH });
            IntegridadManager.Establecer(resultado);

            return new SessionBLL(userDAL, new BitacoraBLL(new FakeBitacoraDAL()), roleDAL);
        }

        [Fact]
        public void IntegridadCorrecta_CualquierRolEntra()
        {
            var resultado = ArmarLogin(RoleName.Invitado, integridadOk: true).Login("u", "clave");

            Assert.Equal(LoginStatus.Success, resultado.Status);
        }

        [Fact]
        public void IntegridadIncorrecta_Administrador_Entra()
        {
            var resultado = ArmarLogin(RoleName.Administrador, integridadOk: false).Login("u", "clave");

            Assert.Equal(LoginStatus.Success, resultado.Status);
        }

        [Fact]
        public void IntegridadIncorrecta_OtroRol_RecibeErrorDeSistemaYNoAbreSesion()
        {
            var resultado = ArmarLogin(RoleName.Invitado, integridadOk: false).Login("u", "clave");

            Assert.Equal(LoginStatus.SystemError, resultado.Status);
            Assert.False(SessionManager.IsLoggedIn());
        }
    }
}
