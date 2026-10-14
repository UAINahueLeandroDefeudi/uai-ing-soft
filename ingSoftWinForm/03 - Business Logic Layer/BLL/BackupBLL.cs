using BE.Entity;
using BE.Enum;
using DAL;
using Services;

namespace BLL
{
    /// <summary>
    /// T07: recuperación manual. Solo un administrador con sesión abierta puede generar
    /// o restaurar un backup; la restauración no es automática ni parte del arranque.
    /// </summary>
    public class BackupBLL
    {
        private readonly IBackupDAL backupDAL;
        private readonly BitacoraBLL bitacoraBLL;
        private readonly IntegridadBLL integridadBLL;
        private readonly IDigitoVerificadorDAL actualDAL;
        private readonly DigitoVerificador calculador = new();

        public BackupBLL() : this(new BackupDAL(), new BitacoraBLL(), new IntegridadBLL(), new DigitoVerificadorDAL()) { }

        /// <param name="actualDAL">Lector de la base en uso, para comparar contra el backup.</param>
        public BackupBLL(IBackupDAL backupDAL, BitacoraBLL bitacoraBLL, IntegridadBLL integridadBLL, IDigitoVerificadorDAL actualDAL)
        {
            this.backupDAL = backupDAL;
            this.bitacoraBLL = bitacoraBLL;
            this.integridadBLL = integridadBLL;
            this.actualDAL = actualDAL;
        }

        public static bool EsAdministrador(User? user)
            => user != null && user.Roles.Any(p =>
                string.Equals(p.Name, RoleName.Administrador, StringComparison.OrdinalIgnoreCase));

        /// <summary>Genera &lt;NombreBD&gt;_yyyyMMdd_HHmmss.bkp. Solo con la base verificada: un backup corrupto no sirve.</summary>
        public OperationResult GenerarBackup()
        {
            return Ejecutar(NameEvent.GenerarBackup, () =>
            {
                if (IntegridadManager.HayProblemas)
                    return OperationResult.Fail("msg.backup.baseCorrupta");

                var carpeta = backupDAL.CarpetaBackups;
                Directory.CreateDirectory(carpeta);
                var archivo = Path.Combine(carpeta, $"{backupDAL.NombreBase}_{DateTime.Now:yyyyMMdd_HHmmss}.bkp");

                backupDAL.Backup(archivo);
                bitacoraBLL.RegistrarEvento(NameEvent.GenerarBackup, $"Backup generado: {Path.GetFileName(archivo)}", Priority.Medium);

                return OperationResult.Ok("msg.backup.generado", Path.GetFileName(archivo));
            });
        }

        /// <summary>Backups de la carpeta configurada, el más reciente primero.</summary>
        public List<string> ListarBackups()
        {
            var carpeta = backupDAL.CarpetaBackups;
            if (!Directory.Exists(carpeta)) return [];

            var prefijo = backupDAL.NombreBase + "_";
            return Directory.GetFiles(carpeta, "*.bkp")
                .Where(f => Path.GetFileName(f).StartsWith(prefijo, StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(f => Path.GetFileName(f), StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        /// <summary>
        /// Analiza el backup sin restaurarlo: su integridad y qué cambios de la base actual
        /// no contiene (desde <see cref="ResultadoValidacionBackup.PerdidaDesde"/>).
        /// Nunca modifica la base en uso ni <see cref="IntegridadManager"/>.
        /// </summary>
        public ResultadoValidacionBackup Validar(string rutaArchivo)
        {
            if (!EsAdministrador(SessionManager.IsLoggedIn() ? SessionManager.GetInstance.User : null))
            {
                bitacoraBLL.RegistrarError(NameEvent.AccesoNoAutorizado,
                    "Intento de validar backup sin ser administrador", Priority.High);
                return new ResultadoValidacionBackup { ErrorKey = "msg.noPermiso" };
            }

            try
            {
                return ValidarInterno(rutaArchivo);
            }
            catch (Exception ex)
            {
                bitacoraBLL.RegistrarError(NameEvent.ErrorSistema, $"Validar backup: {ex.Message}", Priority.High);
                return new ResultadoValidacionBackup { ErrorKey = "msg.generic.error" };
            }
        }

        private ResultadoValidacionBackup ValidarInterno(string rutaArchivo)
        {
            if (!ListarBackups().Contains(rutaArchivo, StringComparer.OrdinalIgnoreCase))
                return new ResultadoValidacionBackup { ErrorKey = "msg.backup.noExiste" };

            return backupDAL.ConBackupMontado(rutaArchivo, (info, backupLector) =>
            {
                var validacion = new ResultadoValidacionBackup { FechaBackup = info.FechaBackup };

                if (!string.Equals(info.BaseOrigen, backupDAL.NombreBase, StringComparison.OrdinalIgnoreCase))
                {
                    validacion.ErrorKey = "msg.backup.invalido";
                    return validacion;
                }

                validacion.Integridad = integridadBLL.Verificar(backupLector);
                if (!validacion.Integridad.EsValido) return validacion;

                foreach (var entidad in integridadBLL.Entidades)
                {
                    var enBackup = backupLector.LeerFilas(entidad)
                        .ToDictionary(f => calculador.ClaveTexto(entidad, f), f => f["DVH"] as string);
                    var actuales = actualDAL.LeerFilas(entidad);

                    var perdida = new PerdidaTabla { Tabla = entidad.Tabla };
                    foreach (var fila in actuales)
                    {
                        if (!enBackup.TryGetValue(calculador.ClaveTexto(entidad, fila), out var dvhBackup))
                            perdida.FilasQueSePierden++;
                        else if (!string.Equals(dvhBackup, fila["DVH"] as string, StringComparison.Ordinal))
                            perdida.FilasModificadas++;

                        if (fila.TryGetValue("EventAt", out var fecha) && fecha is DateTime evento
                            && (validacion.UltimoCambioActual == null || evento > validacion.UltimoCambioActual))
                            validacion.UltimoCambioActual = evento;
                    }

                    if (perdida.Total > 0) validacion.Perdidas.Add(perdida);
                }

                return validacion;
            });
        }

        /// <summary>
        /// Valida y restaura el backup, y vuelve a verificar; el resultado queda en <see cref="IntegridadManager"/>.
        /// Un backup alterado nunca se restaura. Si la base actual tiene cambios que el backup no
        /// contiene, no se restaura salvo que el administrador lo acepte (<paramref name="aceptaPerdida"/>).
        /// </summary>
        public OperationResult Restaurar(string rutaArchivo, bool aceptaPerdida = false)
        {
            return Ejecutar(NameEvent.RestaurarBackup, () =>
            {
                var validacion = ValidarInterno(rutaArchivo);
                if (validacion.ErrorKey != null)
                    return OperationResult.Fail(validacion.ErrorKey);

                if (!validacion.Integridad.EsValido)
                {
                    bitacoraBLL.RegistrarError(NameEvent.RestaurarBackup,
                        $"Backup rechazado por integridad: {Path.GetFileName(rutaArchivo)} ({validacion.Integridad.Errores.Count} diferencias)",
                        Priority.Critical);
                    return OperationResult.Fail("msg.backup.invalido");
                }

                if (validacion.HayPerdida && !aceptaPerdida)
                    return OperationResult.Fail("msg.backup.perdidaDatos",
                        validacion.PerdidaDesde.ToString("g"), validacion.TotalPerdido);

                backupDAL.Restaurar(rutaArchivo);

                var resultado = integridadBLL.Verificar();
                var perdida = validacion.HayPerdida
                    ? $" Aceptó perder {validacion.TotalPerdido} cambios posteriores al {validacion.PerdidaDesde:g}."
                    : string.Empty;
                bitacoraBLL.RegistrarEvento(NameEvent.RestaurarBackup,
                    $"Backup restaurado: {Path.GetFileName(rutaArchivo)} ({validacion.FechaBackup:g}). Integridad posterior: {(resultado.EsValido ? "correcta" : "con errores")}.{perdida}",
                    resultado.EsValido ? Priority.High : Priority.Critical);

                return resultado.EsValido
                    ? OperationResult.Ok("msg.backup.restaurado")
                    : OperationResult.Fail("msg.backup.restauradoConErrores");
            });
        }

        private OperationResult Ejecutar(NameEvent evento, Func<OperationResult> accion)
        {
            if (!EsAdministrador(SessionManager.IsLoggedIn() ? SessionManager.GetInstance.User : null))
            {
                bitacoraBLL.RegistrarError(NameEvent.AccesoNoAutorizado,
                    $"Intento de {evento} sin ser administrador", Priority.High);
                return OperationResult.Fail("msg.noPermiso");
            }

            try
            {
                return accion();
            }
            catch (Exception ex)
            {
                bitacoraBLL.RegistrarError(NameEvent.ErrorSistema, $"{evento}: {ex.Message}", Priority.High);
                return OperationResult.Fail("msg.generic.error");
            }
        }
    }
}
