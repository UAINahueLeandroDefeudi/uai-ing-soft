using System.Net.Mail;
using BE.Entity;
using BE.Enum;
using DAL;
using Services;

namespace BLL
{
    /// <summary>
    /// Alta de usuarios abierta a cualquiera (T04): todo usuario que se registra por su
    /// cuenta recibe el rol 'invitado'. Los demás roles los asigna un administrador.
    /// </summary>
    public class UserBLL
    {
        private const int MinUsernameLength = 3;
        private const int MaxUsernameLength = 50;
        private const int MinPasswordLength = 8;

        private readonly IUserDAL userDAL;
        private readonly BitacoraBLL bitacoraBLL;

        public UserBLL() : this(new UserDAL(), new BitacoraBLL()) { }

        public UserBLL(IUserDAL userDAL, BitacoraBLL bitacoraBLL)
        {
            this.userDAL = userDAL;
            this.bitacoraBLL = bitacoraBLL;
        }

        public OperationResult Register(string username, string password, string confirmPassword,
            string firstName, string lastName, string? email)
        {
            username = username.Trim();
            firstName = firstName.Trim();
            lastName = lastName.Trim();
            email = string.IsNullOrWhiteSpace(email) ? null : email.Trim();

            var error = Validate(username, password, confirmPassword, firstName, lastName, email);
            if (error != null) return OperationResult.Fail(error);

            try
            {
                if (userDAL.GetByUsername(username) != null)
                    return OperationResult.Fail("Ya existe un usuario con ese nombre de usuario.");

                if (email != null && userDAL.EmailExists(email))
                    return OperationResult.Fail("Ya existe un usuario con ese email.");

                var salt = HashManager.GenerateSalt();
                var user = new User
                {
                    Username = username,
                    Salt = salt,
                    PasswordHash = HashManager.HashPassword(password, salt),
                    FirstName = firstName,
                    LastName = lastName,
                    Email = email,
                    IsActive = true,
                    CreatedBy = "registro"
                };

                userDAL.Insert(user, RoleName.Invitado);

                bitacoraBLL.RegistrarEvento(NameEvent.CrearUsuario,
                    $"Registro de usuario '{username}' con rol '{RoleName.Invitado}'", Priority.Low, user);

                return OperationResult.Ok("Usuario registrado. Ya podés iniciar sesión.");
            }
            catch (Exception ex)
            {
                bitacoraBLL.RegistrarError(NameEvent.ErrorSistema,
                    $"Falló el registro del usuario '{username}': {ex.Message}", Priority.High);

                return OperationResult.Fail("No se pudo registrar el usuario. Intente nuevamente.");
            }
        }

        private static string? Validate(string username, string password, string confirmPassword,
            string firstName, string lastName, string? email)
        {
            if (username.Length < MinUsernameLength || username.Length > MaxUsernameLength)
                return $"El usuario debe tener entre {MinUsernameLength} y {MaxUsernameLength} caracteres.";

            if (username.Any(char.IsWhiteSpace))
                return "El usuario no puede contener espacios.";

            if (password.Length < MinPasswordLength)
                return $"La contraseña debe tener al menos {MinPasswordLength} caracteres.";

            if (password != confirmPassword)
                return "Las contraseñas no coinciden.";

            if (firstName.Length == 0 || lastName.Length == 0)
                return "El nombre y el apellido son obligatorios.";

            if (email != null && !MailAddress.TryCreate(email, out _))
                return "El email no tiene un formato válido.";

            return null;
        }
    }
}
