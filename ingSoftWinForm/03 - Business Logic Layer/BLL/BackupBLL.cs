using BE.Entity;
using BE.Enum;
using DAL;
using Services;

namespace BLL
{
    /// <summary>
    /// T07: recuperación manual. Generar un backup exige REALIZAR_BACKUP y
    /// restaurar/validar exige GESTIONAR_BACKUP, siempre con sesión abierta; la restauración no es automática ni parte del arranque.
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

        /// <summary>Restaurar y listar backups, y entrar con la integridad comprometida.</summary>
        public static bool PuedeGestionarBackup(User? user)
            => user != null && user.HasPermission(PermissionCode.GestionarBackup);

        public static bool PuedeRealizarBackup(User? user)
            => user != null && user.HasPermission(PermissionCode.RealizarBackup);

        private static User? UsuarioActual => SessionManager.IsLoggedIn() ? SessionManager.GetInstance.User : null;

        /// <summary>Genera &lt;NombreBD&gt;_yyyyMMdd_HHmmss.bkp. Solo con la base verificada: un backup corrupto no sirve.</summary>
        public OperationResult GenerarBackup()
        {
            return Ejecutar(NameEvent.GenerarBackup, PermissionCode.RealizarBackup, () =>
            {
                // Se verifica en este momento: el resultado de arranque puede estar viejo y un backup
                // de una base alterada nunca debe generarse.
                if (integridadBLL.Verificar().Errores.Count > 0)
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
            if (!PuedeGestionarBackup(UsuarioActual)) return [];

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
            if (!PuedeGestionarBackup(UsuarioActual))
            {
                bitacoraBLL.RegistrarError(NameEvent.AccesoNoAutorizado,
                    "Intento de validar backup sin permiso para gestionar backups", Priority.High);
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
                        .ToDictionary(f => calculador.ClaveTexto(entidad, f));
                    var actuales = actualDAL.LeerFilas(entidad);
                    var clavesActuales = new HashSet<string>(StringComparer.Ordinal);

                    var perdida = new PerdidaTabla { Tabla = entidad.Tabla };
                    foreach (var fila in actuales)
                    {
                        var clave = calculador.ClaveTexto(entidad, fila);
                        clavesActuales.Add(clave);
                        var adulterada = !string.Equals(fila["DVH"] as string, calculador.CalcularDVH(entidad, fila),
                            StringComparison.OrdinalIgnoreCase);

                        if (!enBackup.TryGetValue(clave, out var filaBackup))
                        {
                            perdida.FilasQueSePierden++;
                            validacion.Diferencias.Add(new DiferenciaBackup
                            {
                                Tabla = entidad.Tabla,
                                Clave = clave,
                                Tipo = TipoDiferencia.SoloEnActual,
                                FechaRegistro = FechaDe(fila),
                                FilaActualAdulterada = adulterada
                            });
                        }
                        else
                        {
                            var dvhDistinto = !string.Equals(filaBackup["DVH"] as string, fila["DVH"] as string,
                                StringComparison.Ordinal);
                            if (dvhDistinto) perdida.FilasModificadas++;

                            // Una fila alterada sin tocar su DVH tiene el mismo DVH que el backup: igual se informa.
                            var cambios = CambiosEntre(entidad, fila, filaBackup);
                            if (dvhDistinto || adulterada || cambios.Count > 0)
                                validacion.Diferencias.Add(Modificada(entidad, clave, fila, filaBackup, cambios, adulterada));
                        }

                        var fecha = FechaDe(fila);
                        if (fecha != null && (validacion.UltimoCambioActual == null || fecha > validacion.UltimoCambioActual))
                            validacion.UltimoCambioActual = fecha;
                    }

                    // Lo que el backup tiene y la base actual no: típicamente un borrado hecho fuera de la aplicación.
                    foreach (var (clave, filaBackup) in enBackup.Where(p => !clavesActuales.Contains(p.Key)))
                    {
                        perdida.FilasQueSeRecuperan++;
                        validacion.Diferencias.Add(new DiferenciaBackup
                        {
                            Tabla = entidad.Tabla,
                            Clave = clave,
                            Tipo = TipoDiferencia.SoloEnBackup,
                            FechaRegistro = FechaDe(filaBackup)
                        });
                    }

                    if (perdida.Total > 0 || perdida.FilasQueSeRecuperan > 0) validacion.Perdidas.Add(perdida);
                }

                return validacion;
            });
        }

        private static DateTime? FechaDe(IReadOnlyDictionary<string, object?> fila)
            => fila.TryGetValue("EventAt", out var fecha) && fecha is DateTime evento ? evento : null;

        /// <summary>Detalle columna por columna; si ninguna difiere, lo que cambió es el propio DVH.</summary>
        private static List<CambioColumna> CambiosEntre(EntidadVerificable entidad,
            Dictionary<string, object?> actual, Dictionary<string, object?> backup)
        {
            var cambios = new List<CambioColumna>();

            foreach (var columna in entidad.Atributos)
            {
                var valorActual = DigitoVerificador.Texto(actual.GetValueOrDefault(columna));
                var valorBackup = DigitoVerificador.Texto(backup.GetValueOrDefault(columna));
                if (string.Equals(valorActual, valorBackup, StringComparison.Ordinal)) continue;

                cambios.Add(new CambioColumna { Columna = columna, ValorActual = valorActual, ValorBackup = valorBackup });
            }

            return cambios;
        }

        private static DiferenciaBackup Modificada(EntidadVerificable entidad, string clave,
            Dictionary<string, object?> actual, Dictionary<string, object?> backup,
            List<CambioColumna> cambios, bool adulterada)
        {
            var diferencia = new DiferenciaBackup
            {
                Tabla = entidad.Tabla,
                Clave = clave,
                Tipo = TipoDiferencia.Modificada,
                FechaRegistro = FechaDe(actual) ?? FechaDe(backup),
                FilaActualAdulterada = adulterada
            };
            diferencia.Cambios.AddRange(cambios);

            if (diferencia.Cambios.Count == 0)
                diferencia.Cambios.Add(new CambioColumna
                {
                    Columna = "DVH",
                    ValorActual = actual.GetValueOrDefault("DVH") as string,
                    ValorBackup = backup.GetValueOrDefault("DVH") as string
                });

            return diferencia;
        }

        /// <summary>
        /// Valida y restaura el backup, y vuelve a verificar; el resultado queda en <see cref="IntegridadManager"/>.
        /// Un backup alterado nunca se restaura. Si la base actual tiene cambios que el backup no
        /// contiene, no se restaura salvo que el administrador lo acepte (<paramref name="aceptaPerdida"/>).
        /// </summary>
        public OperationResult Restaurar(string rutaArchivo, bool aceptaPerdida = false)
        {
            return Ejecutar(NameEvent.RestaurarBackup, PermissionCode.GestionarBackup, () =>
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

        private OperationResult Ejecutar(NameEvent evento, string permiso, Func<OperationResult> accion)
        {
            if (UsuarioActual?.HasPermission(permiso) != true)
            {
                bitacoraBLL.RegistrarError(NameEvent.AccesoNoAutorizado,
                    $"Intento de {evento} sin el permiso {permiso}", Priority.High);
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
