using BE.Entity;
using BE.Enum;
using DAL;
using Services;

namespace BLL
{
    /// <summary>
    /// Iniciar sesión: valida credenciales, aplica la política de
    /// bloqueo por intentos fallidos y abre la sesión única.
    /// </summary>
    public class SessionBLL
    {
        /// <summary>RNF-Seguridad-02: bloqueo automático a los 3 intentos fallidos.</summary>
        private const int MaxFailedAttempts = 3;

        private readonly IUserDAL userDAL;
        private readonly BitacoraBLL bitacoraBLL;
        private readonly IRoleDAL roleDAL;
        private readonly IUserHistoryDAL? historyDAL;
        private readonly IntegridadBLL? integridadBLL;

        public SessionBLL() : this(new UserDAL(), new BitacoraBLL(), new RoleDAL(), new UserHistoryDAL(), new IntegridadBLL()) { }

        public SessionBLL(IUserDAL userDAL) : this(userDAL, new BitacoraBLL(), new RoleDAL()) { }

        public SessionBLL(IUserDAL userDAL, BitacoraBLL bitacoraBLL) : this(userDAL, bitacoraBLL, new RoleDAL()) { }

        /// <summary>
        /// historyDAL null = sin historial (tests con fakes no tocan la BD).
        /// integridadBLL null = se usa el último resultado de <see cref="IntegridadManager"/> sin reverificar.
        /// </summary>
        public SessionBLL(IUserDAL userDAL, BitacoraBLL bitacoraBLL, IRoleDAL roleDAL,
            IUserHistoryDAL? historyDAL = null, IntegridadBLL? integridadBLL = null)
        {
            this.userDAL = userDAL;
            this.bitacoraBLL = bitacoraBLL;
            this.roleDAL = roleDAL;
            this.historyDAL = historyDAL;
            this.integridadBLL = integridadBLL;
        }

        public LoginResult Login(string username, string password)
        {
            var user = userDAL.GetByUsername(username);

            // Usuario inexistente devuelve lo mismo que contraseña incorrecta:
            // el mensaje no debe revelar si el usuario existe (CU-01, FA-1 paso 4c).
            if (user == null)
            {
                // No hay User que estampar: el username tipeado va en el detalle.
                // La contraseña no se registra nunca.
                bitacoraBLL.RegistrarError(NameEvent.Login,
                    $"Intento de inicio de sesión con un usuario inexistente: '{username}'",
                    Priority.Medium);
                RegistrarHistorial(null, username, "LoginFailed", false, 0, "usuario inexistente");

                return LoginResult.Fail(LoginStatus.InvalidCredentials);
            }

            if (!user.IsActive)
            {
                bitacoraBLL.RegistrarError(NameEvent.Login,
                    "Intento de inicio de sesión de un usuario dado de baja",
                    Priority.Medium, user);
                RegistrarHistorial(user, "LoginFailed", false, "usuario dado de baja");

                return LoginResult.Fail(LoginStatus.UserInactive);
            }

            if (user.IsBlocked)
            {
                bitacoraBLL.RegistrarError(NameEvent.Login,
                    "Intento de inicio de sesión de un usuario bloqueado",
                    Priority.High, user);
                RegistrarHistorial(user, "LoginFailed", false, "usuario bloqueado");

                return LoginResult.Fail(LoginStatus.UserBlocked);
            }

            if (!HashManager.VerifyPassword(password, user.Salt, user.PasswordHash))
                return RegistrarFallo(user);

            userDAL.ResetFailedAttempts(user.Id);

            // T04: los roles (con su Composite de permisos) se cargan ANTES de abrir la sesión,
            // así la bitácora y el menú ya los ven. HasPermission los recorre recursivamente.
            user.Permissions = roleDAL.GetByUser(user.Id);

            // T07: la base pudo alterarse después de la verificación de arranque (p. ej. con la ventana de
            // login abierta): se verifica de nuevo justo antes de decidir si se abre la sesión.
            ReverificarIntegridad();

            // T07: con la integridad comprometida solo entra quien puede gestionar backups (para ver el
            // diagnóstico y restaurar). El resto recibe un error genérico y no se revela el motivo.
            if (IntegridadManager.HayProblemas)
            {
                if (!BackupBLL.PuedeGestionarBackup(user))
                {
                    bitacoraBLL.RegistrarError(NameEvent.ErrorIntegridad,
                        "Inicio de sesión rechazado: la verificación de integridad de la base falló",
                        Priority.Critical, user);
                    RegistrarHistorial(user, "LoginFailed", false, "error de sistema");

                    return LoginResult.Fail(LoginStatus.SystemError);
                }

                bitacoraBLL.RegistrarError(NameEvent.ErrorIntegridad,
                    $"Usuario con permiso de backup ingresa con la integridad de la base comprometida ({IntegridadManager.Resultado!.Errores.Count} errores)",
                    Priority.Critical, user);
            }

            try
            {
                SessionManager.Login(user);
            }
            catch (InvalidOperationException)
            {
                // FA-4: ya había una sesión abierta. No se abre una segunda.
                bitacoraBLL.RegistrarError(NameEvent.Login,
                    "Se intentó abrir una segunda sesión habiendo una activa",
                    Priority.High, user);
                RegistrarHistorial(user, "LoginFailed", false, "sesión ya abierta");

                return LoginResult.Fail(LoginStatus.SessionAlreadyOpen);
            }

            bitacoraBLL.RegistrarEvento(NameEvent.Login,
                "Inicio de sesión exitoso", Priority.Low, user);
            RegistrarHistorial(user, "Login", true, null, failedAttempts: 0);

            return LoginResult.Ok(user);
        }

        /// <summary>Si la verificación en sí falla (BD caída) se conserva el último resultado: el login ya falla por su cuenta.</summary>
        private void ReverificarIntegridad()
        {
            if (integridadBLL == null) return;

            try
            {
                integridadBLL.Verificar();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Integridad: no se pudo reverificar al iniciar sesión: {ex.Message}");
            }
        }

        public void Logout()
        {
            // El usuario se toma ANTES del Logout: después ya no hay sesión de dónde sacarlo.
            var user = CurrentUser;

            SessionManager.Logout();

            bitacoraBLL.RegistrarEvento(NameEvent.Logout,
                "Cierre de sesión", Priority.Low, user);

            if (user != null) RegistrarHistorial(user, "Logout", true, null, failedAttempts: 0);
        }

        /// <summary>
        /// El historial es secundario: si falla (BD caída, tabla ausente) no debe
        /// romper el login ni el logout, que ya quedaron en la bitácora.
        /// </summary>
        private void RegistrarHistorial(User user, string eventType, bool success, string? detail,
            int? failedAttempts = null)
            => RegistrarHistorial(user.Id, user.Username, eventType, success,
                failedAttempts ?? user.FailedAttempts, detail);

        private void RegistrarHistorial(Guid? userId, string username, string eventType, bool success,
            int failedAttempts, string? detail)
        {
            if (historyDAL == null) return;

            try
            {
                historyDAL.Insert(userId, username, eventType, success, failedAttempts, detail);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"UserHistory: no se pudo registrar {eventType}: {ex.Message}");
            }
        }

        public bool IsLoggedIn => SessionManager.IsLoggedIn();

        /// <summary>T04: la UI consulta permisos por acá, sin depender del SessionManager.</summary>
        public bool HasPermission(string permissionCode) => SessionManager.HasPermission(permissionCode);

        /// <summary>
        /// Usuario de la sesión activa. La UI lo pide por acá para no depender
        /// del SessionManager (capa de servicios).
        /// </summary>
        public User? CurrentUser => SessionManager.IsLoggedIn() ? SessionManager.GetInstance.User : null;

        /// <summary>
        /// FA-1 / FA-2: suma el intento fallido y bloquea al llegar al tope.
        /// </summary>
        private LoginResult RegistrarFallo(User user)
        {
            // El contador nuevo se calcula ANTES de llamar a la DAL: así el resultado no
            // depende de si la DAL actualiza además la entidad que tenemos en memoria.
            var intentos = user.FailedAttempts + 1;

            userDAL.IncrementFailedAttempts(user.Username);

            bitacoraBLL.RegistrarError(NameEvent.Login,
                $"Credencial inválida. Intento fallido {intentos} de {MaxFailedAttempts}",
                Priority.Medium, user);
            RegistrarHistorial(user, "LoginFailed", false, "contraseña incorrecta", intentos);

            if (intentos >= MaxFailedAttempts)
            {
                userDAL.Block(user.Username);
                RegistrarHistorial(user, "Blocked", false,
                    $"bloqueo automático por {MaxFailedAttempts} intentos fallidos", intentos);

                bitacoraBLL.RegistrarError(NameEvent.Login,
                    $"Usuario bloqueado automáticamente por alcanzar {MaxFailedAttempts} intentos fallidos",
                    Priority.Critical, user);

                return LoginResult.Fail(LoginStatus.UserBlocked);
            }

            return LoginResult.Fail(LoginStatus.InvalidCredentials);
        }
    }
}
