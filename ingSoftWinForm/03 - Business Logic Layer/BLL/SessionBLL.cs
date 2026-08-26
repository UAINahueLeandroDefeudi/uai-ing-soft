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

        public SessionBLL() : this(new UserDAL(), new BitacoraBLL()) { }

        public SessionBLL(IUserDAL userDAL) : this(userDAL, new BitacoraBLL()) { }

        public SessionBLL(IUserDAL userDAL, BitacoraBLL bitacoraBLL)
        {
            this.userDAL = userDAL;
            this.bitacoraBLL = bitacoraBLL;
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

                return LoginResult.Fail(LoginStatus.InvalidCredentials);
            }

            if (!user.IsActive)
            {
                bitacoraBLL.RegistrarError(NameEvent.Login,
                    "Intento de inicio de sesión de un usuario dado de baja",
                    Priority.Medium, user);

                return LoginResult.Fail(LoginStatus.UserInactive);
            }

            if (user.IsBlocked)
            {
                bitacoraBLL.RegistrarError(NameEvent.Login,
                    "Intento de inicio de sesión de un usuario bloqueado",
                    Priority.High, user);

                return LoginResult.Fail(LoginStatus.UserBlocked);
            }

            if (!HashManager.VerifyPassword(password, user.Salt, user.PasswordHash))
                return RegistrarFallo(user);

            userDAL.ResetFailedAttempts(user.Id);

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

                return LoginResult.Fail(LoginStatus.SessionAlreadyOpen);
            }

            bitacoraBLL.RegistrarEvento(NameEvent.Login,
                "Inicio de sesión exitoso", Priority.Low, user);

            return LoginResult.Ok(user);
        }

        public void Logout()
        {
            // El usuario se toma ANTES del Logout: después ya no hay sesión de dónde sacarlo.
            var user = CurrentUser;

            SessionManager.Logout();

            bitacoraBLL.RegistrarEvento(NameEvent.Logout,
                "Cierre de sesión", Priority.Low, user);
        }

        public bool IsLoggedIn => SessionManager.IsLoggedIn();

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

            if (intentos >= MaxFailedAttempts)
            {
                userDAL.Block(user.Username);

                bitacoraBLL.RegistrarError(NameEvent.Login,
                    $"Usuario bloqueado automáticamente por alcanzar {MaxFailedAttempts} intentos fallidos",
                    Priority.Critical, user);

                return LoginResult.Fail(LoginStatus.UserBlocked);
            }

            return LoginResult.Fail(LoginStatus.InvalidCredentials);
        }
    }
}
