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

        public Dictionary<string, DateTime> FechasCalculo { get; } = new();

        public DateTime? LeerFechaCalculo(string tabla)
            => FechasCalculo.TryGetValue(tabla, out var fecha) ? fecha : null;

        /// <summary>
        /// Alta legítima: fila con su DVH y DVV actualizado (lo que hace DigitoVerificadorDAL.Escribir).
        /// Igual que la DAL real, no recalcula el DVV de una tabla ya adulterada: lo daría por válido.
        /// </summary>
        public void AltaLegitima(EntidadVerificable entidad, int n)
        {
            var fila = entidad.Atributos.ToDictionary(a => a, a => (object?)$"{a}-{n}");
            Alta(entidad, fila);
        }

        public void Alta(EntidadVerificable entidad, Dictionary<string, object?> fila)
        {
            fila["DVH"] = calculador.CalcularDVH(entidad, fila);

            if (!Tablas.TryGetValue(entidad.Tabla, out var filas)) Tablas[entidad.Tabla] = filas = [];

            var adulterada = calculador.EstaAdulterada(entidad.Tabla, DVVs.GetValueOrDefault(entidad.Tabla),
                filas.Select(f => (calculador.ClaveTexto(entidad, f), f["DVH"] as string)));

            filas.Add(fila);

            if (!adulterada) RecalcularDVV(entidad);
        }

        public void RecalcularDVV(EntidadVerificable entidad)
        {
            DVVs[entidad.Tabla] = calculador.CalcularDVV(entidad.Tabla,
                Tablas[entidad.Tabla].Select(f => (calculador.ClaveTexto(entidad, f), f["DVH"] as string)));
            FechasCalculo[entidad.Tabla] = DateTime.Now;
        }
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

            var afectada = EntidadesVerificables.Producto;
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
            var entidad = EntidadesVerificables.Producto;
            var (bll, dal) = Armar(entidad);
            dal.Tablas[entidad.Tabla][0]["Nombre"] = "alterado";
            var dvhAntes = dal.Tablas[entidad.Tabla].Select(f => f["DVH"]).ToList();
            var dvvAntes = dal.DVVs[entidad.Tabla];

            bll.Verificar();
            bll.Verificar();

            Assert.Equal(dvhAntes, dal.Tablas[entidad.Tabla].Select(f => f["DVH"]).ToList());
            Assert.Equal(dvvAntes, dal.DVVs[entidad.Tabla]);
        }

        // ---- Regresión: una escritura legítima no debe "lavar" una alteración previa ----

        [Fact]
        public void BorradoDeLaUltimaFila_LuegoDeUnaEscrituraLegitima_SigueDetectado()
        {
            // Caso reportado: se borra la última fila de Producto fuera de la app y después el propio
            // ABM inserta una fila. La inserción no puede recalcular el DVV sobre lo que quedó.
            var entidad = EntidadesVerificables.Producto;
            var (bll, dal) = Armar(entidad);
            dal.Tablas[entidad.Tabla].RemoveAt(2);

            dal.AltaLegitima(entidad, 10);
            dal.AltaLegitima(entidad, 11);

            var errores = bll.Verificar().Errores;
            Assert.Contains(errores, e => e.Tabla == entidad.Tabla && e.Tipo == TipoErrorIntegridad.DVV);
            Assert.DoesNotContain(errores, e => e.Tipo == TipoErrorIntegridad.DVH);
        }

        [Fact]
        public void AltaExternaPrevia_LuegoDeUnaEscrituraLegitima_SigueDetectada()
        {
            var entidad = EntidadesVerificables.Producto;
            var (bll, dal) = Armar(entidad);
            var intrusa = entidad.Atributos.ToDictionary(a => a, a => (object?)$"{a}-externo");
            intrusa["DVH"] = null;
            dal.Tablas[entidad.Tabla].Add(intrusa);

            dal.AltaLegitima(entidad, 10);

            var errores = bll.Verificar().Errores;
            Assert.Contains(errores, e => e.Tipo == TipoErrorIntegridad.DVV);
            Assert.Contains(errores, e => e.Tipo == TipoErrorIntegridad.DVH);
        }

        [Fact]
        public void EscriturasLegitimas_SobreTablaIntegra_NoGeneranFalsosPositivos()
        {
            var entidad = EntidadesVerificables.Producto;
            var (bll, dal) = Armar(entidad);

            for (var i = 10; i < 15; i++) dal.AltaLegitima(entidad, i);

            Assert.True(bll.Verificar().EsValido);
        }

        [Fact]
        public void TablaVaciaSinDVV_NoSeConsideraAdulterada()
        {
            var calculador = new DigitoVerificador();

            Assert.False(calculador.EstaAdulterada("T", null, []));
        }

        [Fact]
        public void TablaConFilasSinDVV_EstaAdulterada()
        {
            var calculador = new DigitoVerificador();

            Assert.True(calculador.EstaAdulterada("T", null, [("1", "ABC")]));
        }

        [Fact]
        public void EstaAdulterada_CoincideConLoQueVerificaIntegridad()
        {
            var entidad = EntidadesVerificables.Producto;
            var (_, dal) = Armar(entidad);
            var calculador = new DigitoVerificador();
            (string Clave, string? DVH)[] Filas() => dal.Tablas[entidad.Tabla]
                .Select(f => (calculador.ClaveTexto(entidad, f), f["DVH"] as string)).ToArray();

            Assert.False(calculador.EstaAdulterada(entidad.Tabla, dal.DVVs[entidad.Tabla], Filas()));

            dal.Tablas[entidad.Tabla].RemoveAt(0);

            Assert.True(calculador.EstaAdulterada(entidad.Tabla, dal.DVVs[entidad.Tabla], Filas()));
        }

        // ---- Detalle del diagnóstico ----

        [Fact]
        public void ErrorDVV_InformaCuandoSeEscribioLegitimamentePorUltimaVez()
        {
            var entidad = EntidadesVerificables.Producto;
            var (bll, dal) = Armar(entidad);
            var ultima = new DateTime(2026, 10, 1, 9, 30, 0);
            dal.FechasCalculo[entidad.Tabla] = ultima;
            dal.Tablas[entidad.Tabla].RemoveAt(2);

            var error = bll.Verificar().Errores.Single(e => e.Tipo == TipoErrorIntegridad.DVV);

            Assert.Equal(entidad.Tabla, error.Tabla);
            Assert.Equal(ultima, error.UltimoCalculoValido);
        }

        [Fact]
        public void ErrorDVH_InformaLaFilaYLaFechaDelRegistro()
        {
            var entidad = EntidadesVerificables.Producto;
            var (bll, dal) = Armar(entidad);
            var fecha = new DateTime(2026, 9, 20, 18, 5, 0);
            dal.Tablas[entidad.Tabla][1][entidad.ColumnaFecha!] = fecha;
            dal.Tablas[entidad.Tabla][1]["DVH"] = new DigitoVerificador().CalcularDVH(entidad, dal.Tablas[entidad.Tabla][1]);
            dal.RecalcularDVV(entidad);
            dal.Tablas[entidad.Tabla][1]["Nombre"] = "alterado";

            var error = bll.Verificar().Errores.Single(e => e.Tipo == TipoErrorIntegridad.DVH);

            Assert.Equal("Id-2", error.Clave);
            Assert.Equal(fecha, error.FechaRegistro);
            Assert.NotNull(error.UltimoCalculoValido);
        }

        [Fact]
        public void Resultado_InformaCuandoSeDetecto()
        {
            var entidad = EntidadesVerificables.Producto;
            var (bll, _) = Armar(entidad);
            var antes = DateTime.Now;

            var resultado = bll.Verificar();

            Assert.InRange(resultado.FechaVerificacion, antes, DateTime.Now);
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
            // Solo el rol administrador sembrado trae los permisos de backup.
            roleDAL.Roles.Add(string.Equals(rol, RoleName.Administrador, StringComparison.OrdinalIgnoreCase)
                ? RoleWith(rol, 1, Simple(PermissionCode.GestionarBackup), Simple(PermissionCode.RealizarBackup))
                : RoleWith(rol, 1));
            roleDAL.UserRoles[user.Id] = [1];

            var resultado = new ResultadoIntegridad();
            if (!integridadOk)
                resultado.Errores.Add(new ErrorIntegridad { Tabla = "Producto", Clave = "1", Tipo = TipoErrorIntegridad.DVH });
            IntegridadManager.Establecer(resultado);

            return new SessionBLL(userDAL, new BitacoraBLL(new FakeBitacoraDAL()), roleDAL);
        }

        /// <summary>Login con reverificación real contra un DAL falso de la base (el arranque dijo "íntegra").</summary>
        private static SessionBLL ArmarLoginConBase(string rol, FakeDigitoVerificadorDAL baseDatos)
        {
            ArmarLogin(rol, integridadOk: true);

            var salt = HashManager.GenerateSalt();
            var user = new User
            {
                Id = Guid.NewGuid(), Username = "u", FirstName = "N", LastName = "A", IsActive = true,
                Salt = salt, PasswordHash = HashManager.HashPassword("clave", salt)
            };
            var userDAL = new FakeUserDAL();
            userDAL.Users.Add(user);

            var roleDAL = new FakeRoleDAL();
            roleDAL.Roles.Add(string.Equals(rol, RoleName.Administrador, StringComparison.OrdinalIgnoreCase)
                ? RoleWith(rol, 1, Simple(PermissionCode.GestionarBackup), Simple(PermissionCode.RealizarBackup))
                : RoleWith(rol, 1));
            roleDAL.UserRoles[user.Id] = [1];

            return new SessionBLL(userDAL, new BitacoraBLL(new FakeBitacoraDAL()), roleDAL,
                integridadBLL: new IntegridadBLL(baseDatos, [EntidadesVerificables.UserPermission]));
        }

        private static FakeDigitoVerificadorDAL BaseIntegra()
        {
            var baseDatos = new FakeDigitoVerificadorDAL();
            for (var i = 1; i <= 3; i++) baseDatos.AltaLegitima(EntidadesVerificables.UserPermission, i);
            return baseDatos;
        }

        [Fact]
        public void FilaBorradaDeUserPermissionConLaAppAbierta_SeDetectaAlIniciarSesion()
        {
            // Caso reportado: la verificación de arranque dio "íntegra", se borra una fila de
            // User_Permission con la ventana de login abierta y recién después se inicia sesión.
            var baseDatos = BaseIntegra();
            var sesion = ArmarLoginConBase(RoleName.Invitado, baseDatos);
            baseDatos.Tablas[EntidadesVerificables.UserPermission.Tabla].RemoveAt(2);

            var resultado = sesion.Login("u", "clave");

            Assert.Equal(LoginStatus.SystemError, resultado.Status);
            Assert.True(IntegridadManager.HayProblemas);
            Assert.Contains(IntegridadManager.Resultado!.Errores,
                e => e.Tabla == "User_Permission" && e.Tipo == TipoErrorIntegridad.DVV);
        }

        [Fact]
        public void FilaBorradaConLaAppAbierta_ElAdministradorEntraYQuedaMarcadoElProblema()
        {
            var baseDatos = BaseIntegra();
            var sesion = ArmarLoginConBase(RoleName.Administrador, baseDatos);
            baseDatos.Tablas[EntidadesVerificables.UserPermission.Tabla].RemoveAt(0);

            var resultado = sesion.Login("u", "clave");

            Assert.Equal(LoginStatus.Success, resultado.Status);
            Assert.True(IntegridadManager.HayProblemas);
        }

        [Fact]
        public void BaseIntegra_ReverificarAlLogin_NoBloquea()
        {
            var sesion = ArmarLoginConBase(RoleName.Invitado, BaseIntegra());

            var resultado = sesion.Login("u", "clave");

            Assert.Equal(LoginStatus.Success, resultado.Status);
            Assert.False(IntegridadManager.HayProblemas);
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
