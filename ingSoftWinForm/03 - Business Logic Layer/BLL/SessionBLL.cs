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

        public SessionBLL() : this(new UserDAL()) { }

        public SessionBLL(IUserDAL userDAL)
        {
            this.userDAL = userDAL;
        }

        public LoginResult Login(string username, string password)
        {
            var user = userDAL.GetByUsername(username);

            // Usuario inexistente devuelve lo mismo que contraseña incorrecta:
            // el mensaje no debe revelar si el usuario existe (CU-01, FA-1 paso 4c).
            if (user == null)
                return LoginResult.Fail(LoginStatus.InvalidCredentials);

            if (!user.IsActive)
                return LoginResult.Fail(LoginStatus.UserInactive);

            if (user.IsBlocked)
                return LoginResult.Fail(LoginStatus.UserBlocked);

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
                return LoginResult.Fail(LoginStatus.SessionAlreadyOpen);
            }

            return LoginResult.Ok(user);
        }

        public void Logout() => SessionManager.Logout();

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

            if (intentos >= MaxFailedAttempts)
            {
                userDAL.Block(user.Username);
                return LoginResult.Fail(LoginStatus.UserBlocked);
            }

            return LoginResult.Fail(LoginStatus.InvalidCredentials);
        }
    }
}
