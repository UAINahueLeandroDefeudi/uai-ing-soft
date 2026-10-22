using BE.Entity;
using BE.Enum;
using BLL;
using DAL;
using Services;
using Xunit;
using static Tests.Samples;

namespace Tests
{
    /// <summary>T07: backups en memoria; cada "archivo" es un .bkp vacío con su contenido simulado.</summary>
    public class FakeBackupDAL : IBackupDAL, IDisposable
    {
        private readonly string carpeta = Path.Combine(Path.GetTempPath(), "bkp_" + Guid.NewGuid().ToString("N"));
        private readonly Dictionary<string, (InfoBackup Info, FakeDigitoVerificadorDAL Contenido)> backups = new();

        public FakeBackupDAL() => Directory.CreateDirectory(carpeta);

        public string NombreBase => "IF_DB";
        public string CarpetaBackups => carpeta;
        public List<string> Restaurados { get; } = new();
        public bool Dañado { get; set; }

        public string Agregar(string nombre, DateTime fecha, FakeDigitoVerificadorDAL contenido, string baseOrigen = "IF_DB")
        {
            var ruta = Path.Combine(carpeta, nombre);
            File.WriteAllBytes(ruta, []);
            backups[ruta] = (new InfoBackup(baseOrigen, fecha), contenido);
            return ruta;
        }

        public void Backup(string rutaArchivo) { }

        public void Restaurar(string rutaArchivo) => Restaurados.Add(rutaArchivo);

        public T ConBackupMontado<T>(string rutaArchivo, Func<InfoBackup, IDigitoVerificadorDAL, T> accion)
        {
            if (Dañado) throw new InvalidOperationException("Backup dañado");
            var (info, contenido) = backups[rutaArchivo];
            return accion(info, contenido);
        }

        public void Dispose() => Directory.Delete(carpeta, recursive: true);
    }

    [Collection("Session")]
    public class BackupTests : IDisposable
    {
        private static readonly EntidadVerificable Entidad = EntidadesVerificables.Producto;
        private static readonly DateTime FechaBackup = new(2026, 10, 1, 12, 0, 0);

        private readonly FakeBackupDAL backupDAL = new();
        private readonly FakeBitacoraDAL bitacora = new();
        private readonly FakeDigitoVerificadorDAL actual = new();
        private readonly FakeDigitoVerificadorDAL enBackup = new();

        public BackupTests()
        {
            if (SessionManager.IsLoggedIn()) SessionManager.Logout();
            SessionManager.Login(UserWith("admin", RoleWith(RoleName.Administrador, 1,
                Simple(PermissionCode.GestionarBackup), Simple(PermissionCode.RealizarBackup))));
            IntegridadManager.Establecer(null);

            // Mismo contenido en ambas: el backup es "al día".
            for (var i = 1; i <= 3; i++)
            {
                actual.AltaLegitima(Entidad, i);
                enBackup.AltaLegitima(Entidad, i);
            }
        }

        public void Dispose()
        {
            if (SessionManager.IsLoggedIn()) SessionManager.Logout();
            IntegridadManager.Establecer(null);
            backupDAL.Dispose();
        }

        private BackupBLL Armar()
        {
            var integridad = new IntegridadBLL(actual, [Entidad]);
            return new BackupBLL(backupDAL, new BitacoraBLL(bitacora), integridad, actual);
        }

        [Fact]
        public void BackupAlDia_Restaura()
        {
            var ruta = backupDAL.Agregar("IF_DB_20261001_120000.bkp", FechaBackup, enBackup);

            var resultado = Armar().Restaurar(ruta);

            Assert.True(resultado.Success);
            Assert.Equal([ruta], backupDAL.Restaurados);
        }

        [Fact]
        public void BackupConFilaModificada_NoSeRestaura()
        {
            enBackup.Tablas[Entidad.Tabla][0]["Nombre"] = "alterado";
            var ruta = backupDAL.Agregar("IF_DB_20261001_120000.bkp", FechaBackup, enBackup);

            var resultado = Armar().Restaurar(ruta, aceptaPerdida: true);

            Assert.False(resultado.Success);
            Assert.Equal("msg.backup.invalido", resultado.MessageKey);
            Assert.Empty(backupDAL.Restaurados);
        }

        [Fact]
        public void BackupConAltaExterna_NoSeRestaura()
        {
            var intrusa = Entidad.Atributos.ToDictionary(a => a, a => (object?)$"{a}-externo");
            intrusa["DVH"] = null;
            enBackup.Tablas[Entidad.Tabla].Add(intrusa);
            var ruta = backupDAL.Agregar("IF_DB_20261001_120000.bkp", FechaBackup, enBackup);

            var resultado = Armar().Restaurar(ruta, aceptaPerdida: true);

            Assert.Equal("msg.backup.invalido", resultado.MessageKey);
            Assert.Empty(backupDAL.Restaurados);
        }

        [Fact]
        public void BackupConBajaExterna_NoSeRestaura()
        {
            enBackup.Tablas[Entidad.Tabla].RemoveAt(0);
            var ruta = backupDAL.Agregar("IF_DB_20261001_120000.bkp", FechaBackup, enBackup);

            var resultado = Armar().Restaurar(ruta, aceptaPerdida: true);

            Assert.Equal("msg.backup.invalido", resultado.MessageKey);
            Assert.Empty(backupDAL.Restaurados);
        }

        [Fact]
        public void BackupViejo_SinAceptar_NoSeRestauraYInformaDesdeCuando()
        {
            // La base actual tiene 2 registros que el backup (consistente pero viejo) no contiene.
            actual.AltaLegitima(Entidad, 4);
            actual.AltaLegitima(Entidad, 5);
            var ruta = backupDAL.Agregar("IF_DB_20261001_120000.bkp", FechaBackup, enBackup);
            var bll = Armar();

            var validacion = bll.Validar(ruta);
            var resultado = bll.Restaurar(ruta);

            Assert.Null(validacion.ErrorKey);
            Assert.True(validacion.Integridad.EsValido);
            Assert.True(validacion.HayPerdida);
            Assert.Equal(FechaBackup, validacion.PerdidaDesde);
            Assert.Equal(2, validacion.Perdidas.Single(p => p.Tabla == Entidad.Tabla).FilasQueSePierden);
            Assert.Equal("msg.backup.perdidaDatos", resultado.MessageKey);
            Assert.Empty(backupDAL.Restaurados);
        }

        [Fact]
        public void BackupViejo_AceptandoLaPerdida_Restaura()
        {
            actual.AltaLegitima(Entidad, 4);
            var ruta = backupDAL.Agregar("IF_DB_20261001_120000.bkp", FechaBackup, enBackup);

            var resultado = Armar().Restaurar(ruta, aceptaPerdida: true);

            Assert.True(resultado.Success);
            Assert.Equal([ruta], backupDAL.Restaurados);
            Assert.Contains(bitacora.Registros, r => r.Detail.Contains("Aceptó perder 1"));
        }

        [Fact]
        public void FilaModificadaDespuesDelBackup_CuentaComoPerdida()
        {
            actual.Tablas[Entidad.Tabla][1]["Nombre"] = "cambio posterior";
            actual.Tablas[Entidad.Tabla][1]["DVH"] = new DigitoVerificador().CalcularDVH(Entidad, actual.Tablas[Entidad.Tabla][1]);
            actual.RecalcularDVV(Entidad);
            var ruta = backupDAL.Agregar("IF_DB_20261001_120000.bkp", FechaBackup, enBackup);

            var validacion = Armar().Validar(ruta);

            Assert.Equal(1, validacion.Perdidas.Single().FilasModificadas);
        }

        [Fact]
        public void Validar_NoCambiaElResultadoGlobalDeIntegridad()
        {
            var ruta = backupDAL.Agregar("IF_DB_20261001_120000.bkp", FechaBackup, enBackup);
            var previo = new ResultadoIntegridad();
            IntegridadManager.Establecer(previo);

            Armar().Validar(ruta);

            Assert.Same(previo, IntegridadManager.Resultado);
        }

        [Fact]
        public void BackupDeOtraBase_SeRechaza()
        {
            var ruta = backupDAL.Agregar("IF_DB_20261001_120000.bkp", FechaBackup, enBackup, baseOrigen: "OtraBase");

            var resultado = Armar().Restaurar(ruta, aceptaPerdida: true);

            Assert.Equal("msg.backup.invalido", resultado.MessageKey);
            Assert.Empty(backupDAL.Restaurados);
        }

        [Fact]
        public void ArchivoInexistente_NoSeRestaura()
        {
            var resultado = Armar().Restaurar(Path.Combine(Path.GetTempPath(), "IF_DB_no_existe.bkp"));

            Assert.Equal("msg.backup.noExiste", resultado.MessageKey);
            Assert.Empty(backupDAL.Restaurados);
        }

        [Fact]
        public void BackupDañado_ErrorGenericoYBitacora()
        {
            var ruta = backupDAL.Agregar("IF_DB_20261001_120000.bkp", FechaBackup, enBackup);
            backupDAL.Dañado = true;

            var resultado = Armar().Restaurar(ruta);

            Assert.Equal("msg.generic.error", resultado.MessageKey);
            Assert.Empty(backupDAL.Restaurados);
            Assert.NotEmpty(bitacora.Registros);
        }

        // ---- Detalle de diferencias contra el backup ----

        [Fact]
        public void FilaBorradaFueraDeLaApp_SeInformaComoSoloEnBackup()
        {
            actual.Tablas[Entidad.Tabla].RemoveAt(2);
            var ruta = backupDAL.Agregar("IF_DB_20261001_120000.bkp", FechaBackup, enBackup);

            var validacion = Armar().Validar(ruta);

            var dif = Assert.Single(validacion.Diferencias);
            Assert.Equal(TipoDiferencia.SoloEnBackup, dif.Tipo);
            Assert.Equal(Entidad.Tabla, dif.Tabla);
            Assert.Equal("Id-3", dif.Clave);
            Assert.Equal(1, validacion.Perdidas.Single().FilasQueSeRecuperan);
            Assert.False(validacion.HayPerdida);
        }

        [Fact]
        public void FilaModificadaFueraDeLaApp_InformaColumnaYValores()
        {
            actual.Tablas[Entidad.Tabla][1]["Nombre"] = "alterado";
            var ruta = backupDAL.Agregar("IF_DB_20261001_120000.bkp", FechaBackup, enBackup);

            var validacion = Armar().Validar(ruta);

            var dif = Assert.Single(validacion.Diferencias);
            Assert.Equal(TipoDiferencia.Modificada, dif.Tipo);
            Assert.Equal("Id-2", dif.Clave);
            Assert.True(dif.FilaActualAdulterada);
            var cambio = Assert.Single(dif.Cambios);
            Assert.Equal("Nombre", cambio.Columna);
            Assert.Equal("alterado", cambio.ValorActual);
            Assert.Equal("Nombre-2", cambio.ValorBackup);
        }

        [Fact]
        public void FilaModificadaLegitimamente_NoSeMarcaComoAdulterada()
        {
            actual.Tablas[Entidad.Tabla][1]["Nombre"] = "cambio posterior";
            actual.Tablas[Entidad.Tabla][1]["DVH"] = new DigitoVerificador().CalcularDVH(Entidad, actual.Tablas[Entidad.Tabla][1]);
            actual.RecalcularDVV(Entidad);
            var ruta = backupDAL.Agregar("IF_DB_20261001_120000.bkp", FechaBackup, enBackup);

            var dif = Assert.Single(Armar().Validar(ruta).Diferencias);

            Assert.False(dif.FilaActualAdulterada);
        }

        [Fact]
        public void AltaExterna_SeInformaComoSoloEnActualYAdulterada()
        {
            var intrusa = Entidad.Atributos.ToDictionary(a => a, a => (object?)$"{a}-externo");
            intrusa["DVH"] = null;
            actual.Tablas[Entidad.Tabla].Add(intrusa);
            var ruta = backupDAL.Agregar("IF_DB_20261001_120000.bkp", FechaBackup, enBackup);

            var dif = Assert.Single(Armar().Validar(ruta).Diferencias);

            Assert.Equal(TipoDiferencia.SoloEnActual, dif.Tipo);
            Assert.Equal("Id-externo", dif.Clave);
            Assert.True(dif.FilaActualAdulterada);
        }

        [Fact]
        public void DiferenciaConFecha_InformaElMomentoDelRegistro()
        {
            var fecha = new DateTime(2026, 9, 30, 22, 15, 0);
            var fila = actual.Tablas[Entidad.Tabla][0];
            fila[Entidad.ColumnaFecha!] = fecha;
            fila["Nombre"] = "alterado";
            var ruta = backupDAL.Agregar("IF_DB_20261001_120000.bkp", FechaBackup, enBackup);

            var dif = Assert.Single(Armar().Validar(ruta).Diferencias);

            Assert.Equal(fecha, dif.FechaRegistro);
        }

        [Fact]
        public void BackupIdentico_NoTieneDiferencias()
        {
            var ruta = backupDAL.Agregar("IF_DB_20261001_120000.bkp", FechaBackup, enBackup);

            var validacion = Armar().Validar(ruta);

            Assert.Empty(validacion.Diferencias);
            Assert.Empty(validacion.Perdidas);
        }

        [Fact]
        public void ConPermiso_GeneraBackup()
        {
            var resultado = Armar().GenerarBackup();

            Assert.True(resultado.Success);
        }

        [Fact]
        public void BaseCorrupta_NoGeneraBackup()
        {
            actual.Tablas[Entidad.Tabla].RemoveAt(2);

            var resultado = Armar().GenerarBackup();

            Assert.False(resultado.Success);
            Assert.Equal("msg.backup.baseCorrupta", resultado.MessageKey);
        }

        [Fact]
        public void AlteracionPosteriorALaVerificacionDeArranque_NoGeneraBackup()
        {
            // El resultado de arranque dice "íntegra", pero la base se alteró después: se reverifica.
            IntegridadManager.Establecer(new ResultadoIntegridad());
            actual.Tablas[Entidad.Tabla][0]["Nombre"] = "alterado";

            var resultado = Armar().GenerarBackup();

            Assert.Equal("msg.backup.baseCorrupta", resultado.MessageKey);
            Assert.True(IntegridadManager.HayProblemas);
        }

        [Fact]
        public void SinRealizarBackup_NoGeneraAunqueGestione()
        {
            SessionManager.Logout();
            SessionManager.Login(UserWith("gestor", RoleWith("gestor", 3, Simple(PermissionCode.GestionarBackup))));

            var resultado = Armar().GenerarBackup();

            Assert.Equal("msg.noPermiso", resultado.MessageKey);
            Assert.NotEmpty(bitacora.Registros);
        }

        [Fact]
        public void SinGestionarBackup_NoRestauraAunqueRealice()
        {
            SessionManager.Logout();
            SessionManager.Login(UserWith("operador", RoleWith("operador", 4, Simple(PermissionCode.RealizarBackup))));
            var ruta = backupDAL.Agregar("IF_DB_20261001_120000.bkp", FechaBackup, enBackup);

            var resultado = Armar().Restaurar(ruta);

            Assert.Equal("msg.noPermiso", resultado.MessageKey);
            Assert.Empty(backupDAL.Restaurados);
        }

        [Fact]
        public void SinSerAdministrador_NoValidaNiRestaura()
        {
            SessionManager.Logout();
            SessionManager.Login(UserWith("invitado", RoleWith(RoleName.Invitado, 2)));
            var ruta = backupDAL.Agregar("IF_DB_20261001_120000.bkp", FechaBackup, enBackup);
            var bll = Armar();

            Assert.Equal("msg.noPermiso", bll.Validar(ruta).ErrorKey);
            Assert.Equal("msg.noPermiso", bll.Restaurar(ruta, aceptaPerdida: true).MessageKey);
            Assert.Empty(backupDAL.Restaurados);
        }
    }
}
