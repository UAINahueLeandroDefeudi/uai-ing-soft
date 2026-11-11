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
    /// T05: los resultados llevan la clave de la etiqueta del mensaje (msg.user.*), no el texto.
    ///
    /// Control de cambios (permiso GESTIONAR_USUARIOS): los seis campos NickName, FirstName, LastName,
    /// Pais, Provincia y Direccion se versionan en [User_History]. Cada modificación guarda una versión
    /// nueva con la copia de esos campos, quién la hizo y cuándo. Restaurar una versión vieja no reescribe
    /// el historial: la vuelve a aplicar como una versión nueva, así la traza queda completa.
    /// </summary>
    public class UserBLL
    {
        private const int MinUsernameLength = 3;
        private const int MaxUsernameLength = 50;
        private const int MinPasswordLength = 8;

        public const int MaxNickNameLength = 50;
        public const int MaxNombreLength = 100;
        public const int MaxPaisLength = 100;
        public const int MaxProvinciaLength = 100;
        public const int MaxDireccionLength = 200;

        /// <summary>Campos versionados, en el orden en que se muestran.</summary>
        public static readonly IReadOnlyList<string> CamposVersionados =
            ["NickName", "FirstName", "LastName", "Pais", "Provincia", "Direccion"];

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
            if (error != null) return OperationResult.Fail(error.Value.Key, error.Value.Args);

            try
            {
                if (userDAL.GetByUsername(username) != null)
                    return OperationResult.Fail("msg.user.usernameExists");

                if (email != null && userDAL.EmailExists(email))
                    return OperationResult.Fail("msg.user.emailExists");

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

                return OperationResult.Ok("msg.user.registered");
            }
            catch (Exception ex)
            {
                bitacoraBLL.RegistrarError(NameEvent.ErrorSistema,
                    $"Falló el registro del usuario '{username}': {ex.Message}", Priority.High);

                return OperationResult.Fail("msg.user.registerFailed");
            }
        }

        // ---- Control de cambios ----

        public List<User> GetAll() => userDAL.GetAll();

        /// <summary>Versiones del usuario, de la más vieja a la más nueva.</summary>
        public List<UserHistory> GetHistorial(Guid userId) => userDAL.GetHistory(userId);

        /// <summary>
        /// Guarda los seis campos versionados de <paramref name="user"/> (con el Id y la Version con la que se
        /// leyó) como una versión nueva. Rechaza datos inválidos, "sin cambios" y "desactualizado" (otro usuario
        /// lo modificó antes). Si se guarda, <paramref name="user"/> queda con los datos normalizados y la nueva Version.
        /// </summary>
        public OperationResult Modificar(User user)
        {
            if (!Authorize("modificar un usuario")) return Denied();

            // Se trabaja sobre una copia: si la operación se rechaza, el objeto de la UI no cambia.
            var cambiado = CopiarVersionado(user);
            var invalido = Completar(cambiado);
            if (invalido != null) return invalido;

            User? guardado;
            try
            {
                guardado = userDAL.GetById(user.Id);
            }
            catch (Exception ex)
            {
                RegistrarFalloSistema("Falló la consulta del usuario", ex);
                return OperationResult.Fail("msg.generic.error");
            }

            if (guardado == null) return OperationResult.Fail("msg.generic.error");
            if (guardado.Version != user.Version) return OperationResult.Fail("msg.usuario.desactualizado");
            if (MismosDatos(cambiado, guardado)) return OperationResult.Fail("msg.usuario.sinCambios");

            var resultado = Execute(NameEvent.ModificarUsuario,
                $"Modificación del usuario '{guardado.Username}'",
                () => userDAL.Update(cambiado, TipoCambioUsuario.Modificacion, UsuarioActual()),
                "msg.usuario.actualizado");

            if (resultado.Success) AplicarVersionado(user, cambiado);
            return resultado;
        }

        /// <summary>
        /// Vuelve a aplicar los seis campos de <paramref name="version"/> como una versión nueva
        /// (TipoCambio = Restauracion, RestauradaDesde = el número de aquella versión).
        /// <paramref name="user"/> es el usuario tal como se leyó (con su Version vigente).
        /// </summary>
        public OperationResult RestaurarVersion(User user, UserHistory version)
        {
            if (!Authorize("restaurar una versión de un usuario")) return Denied();
            if (version.UserId != user.Id) return OperationResult.Fail("msg.generic.error");
            if (version.Version == user.Version) return OperationResult.Fail("msg.usuario.versionActual");

            var restaurado = CopiarVersionado(user);
            restaurado.NickName = version.NickName;
            restaurado.FirstName = version.FirstName;
            restaurado.LastName = version.LastName;
            restaurado.Pais = version.Pais;
            restaurado.Provincia = version.Provincia;
            restaurado.Direccion = version.Direccion;

            if (MismosDatos(restaurado, user)) return OperationResult.Fail("msg.usuario.sinCambios");

            var resultado = Execute(NameEvent.RestaurarVersionUsuario,
                $"Usuario '{user.Username}' restaurado a la versión {version.Version}",
                () => userDAL.Update(restaurado, TipoCambioUsuario.Restauracion, UsuarioActual(), version.Version),
                "msg.usuario.restaurado", version.Version);

            if (resultado.Success) AplicarVersionado(user, restaurado);
            return resultado;
        }

        /// <summary>
        /// Compara dos versiones campo por campo, en el orden de <see cref="CamposVersionados"/>. Sin versión
        /// anterior (la primera) no hay con qué comparar: ningún campo figura como cambiado.
        /// Nulo y vacío se consideran iguales.
        /// </summary>
        public static IReadOnlyList<CambioCampo> CompararVersiones(UserHistory? anterior, UserHistory actual)
        {
            CambioCampo Comparar(string campo, Func<UserHistory, string?> valor)
            {
                var antes = anterior == null ? null : valor(anterior);
                var ahora = valor(actual);
                return new CambioCampo(campo, antes, ahora, anterior != null && !MismoTexto(antes, ahora));
            }

            return
            [
                Comparar("NickName", h => h.NickName),
                Comparar("FirstName", h => h.FirstName),
                Comparar("LastName", h => h.LastName),
                Comparar("Pais", h => h.Pais),
                Comparar("Provincia", h => h.Provincia),
                Comparar("Direccion", h => h.Direccion)
            ];
        }

        /// <summary>
        /// Qué informar en la columna "Detalle" de <paramref name="actual"/>: Alta para la primera versión,
        /// la versión restaurada para una restauración, o los campos que difieren de la versión anterior.
        /// </summary>
        public static DetalleVersion DescribirVersion(UserHistory? anterior, UserHistory actual)
        {
            if (actual.TipoCambio == TipoCambioUsuario.Alta)
                return new DetalleVersion(TipoDetalleVersion.Alta, [], null);

            if (actual.TipoCambio == TipoCambioUsuario.Restauracion)
                return new DetalleVersion(TipoDetalleVersion.Restauracion, [], actual.RestauradaDesde);

            var campos = CompararVersiones(anterior, actual).Where(c => c.Cambio).Select(c => c.Campo).ToList();
            return campos.Count == 0
                ? new DetalleVersion(TipoDetalleVersion.Ninguno, [], null)
                : new DetalleVersion(TipoDetalleVersion.Modificacion, campos, null);
        }

        // ---- Soporte del control de cambios ----

        /// <summary>Valida, normaliza y carga los campos editables. Devuelve el rechazo, o null si son válidos.</summary>
        private static OperationResult? Completar(User user)
        {
            var nick = Opcional(user.NickName);
            var nombre = (user.FirstName ?? string.Empty).Trim();
            var apellido = (user.LastName ?? string.Empty).Trim();
            var pais = Opcional(user.Pais);
            var provincia = Opcional(user.Provincia);
            var direccion = Opcional(user.Direccion);

            if (nick is { Length: > MaxNickNameLength })
                return OperationResult.Fail("msg.usuario.nickInvalido", MaxNickNameLength);
            if (nombre.Length == 0 || nombre.Length > MaxNombreLength)
                return OperationResult.Fail("msg.usuario.nombreInvalido", MaxNombreLength);
            if (apellido.Length == 0 || apellido.Length > MaxNombreLength)
                return OperationResult.Fail("msg.usuario.apellidoInvalido", MaxNombreLength);
            if (pais is { Length: > MaxPaisLength })
                return OperationResult.Fail("msg.usuario.paisInvalido", MaxPaisLength);
            if (provincia is { Length: > MaxProvinciaLength })
                return OperationResult.Fail("msg.usuario.provinciaInvalida", MaxProvinciaLength);
            if (direccion is { Length: > MaxDireccionLength })
                return OperationResult.Fail("msg.usuario.direccionInvalida", MaxDireccionLength);

            user.NickName = nick;
            user.FirstName = nombre;
            user.LastName = apellido;
            user.Pais = pais;
            user.Provincia = provincia;
            user.Direccion = direccion;
            return null;
        }

        private static string? Opcional(string? texto) => string.IsNullOrWhiteSpace(texto) ? null : texto.Trim();

        private static bool MismoTexto(string? a, string? b)
            => string.Equals(Opcional(a), Opcional(b), StringComparison.Ordinal);

        private static bool MismosDatos(User a, User b)
            => MismoTexto(a.NickName, b.NickName) && MismoTexto(a.FirstName, b.FirstName)
               && MismoTexto(a.LastName, b.LastName) && MismoTexto(a.Pais, b.Pais)
               && MismoTexto(a.Provincia, b.Provincia) && MismoTexto(a.Direccion, b.Direccion);

        /// <summary>Copia con lo que la DAL necesita para versionar: identidad, Version y los seis campos.</summary>
        private static User CopiarVersionado(User u) => new()
        {
            Id = u.Id,
            Username = u.Username,
            NickName = u.NickName,
            FirstName = u.FirstName,
            LastName = u.LastName,
            Pais = u.Pais,
            Provincia = u.Provincia,
            Direccion = u.Direccion,
            Version = u.Version
        };

        private static void AplicarVersionado(User destino, User origen)
        {
            destino.NickName = origen.NickName;
            destino.FirstName = origen.FirstName;
            destino.LastName = origen.LastName;
            destino.Pais = origen.Pais;
            destino.Provincia = origen.Provincia;
            destino.Direccion = origen.Direccion;
            destino.Version = origen.Version;
        }

        /// <summary>EventBy del historial: el usuario de la sesión (Authorize ya garantizó que hay una).</summary>
        private static Guid UsuarioActual() => SessionManager.GetInstance.User.Id;

        private bool Authorize(string action)
        {
            if (SessionManager.HasPermission(PermissionCode.GestionarUsuarios)) return true;

            bitacoraBLL.RegistrarError(NameEvent.AccesoNoAutorizado,
                $"Intento no autorizado de {action} (requiere {PermissionCode.GestionarUsuarios})", Priority.High);

            return false;
        }

        private static OperationResult Denied() => OperationResult.Fail("msg.noPermiso");

        /// <summary>
        /// <paramref name="action"/> devuelve false si el usuario cambió desde que se leyó:
        /// no se escribió nada y se pide recargar.
        /// </summary>
        private OperationResult Execute(NameEvent nameEvent, string detail, Func<bool> action, string okKey,
            params object[] okArgs)
        {
            try
            {
                if (!action()) return OperationResult.Fail("msg.usuario.desactualizado");

                bitacoraBLL.RegistrarEvento(nameEvent, detail, Priority.Medium);
                return OperationResult.Ok(okKey, okArgs);
            }
            catch (Exception ex)
            {
                RegistrarFalloSistema(detail, ex);
                return OperationResult.Fail("msg.generic.error");
            }
        }

        private void RegistrarFalloSistema(string detail, Exception ex)
        {
            if (SessionManager.IsLoggedIn())
                bitacoraBLL.RegistrarError(NameEvent.ErrorSistema, $"{detail}: {ex.Message}", Priority.High);
            System.Diagnostics.Debug.WriteLine(ex);
        }

        private static (string Key, object[] Args)? Validate(string username, string password, string confirmPassword,
            string firstName, string lastName, string? email)
        {
            if (username.Length < MinUsernameLength || username.Length > MaxUsernameLength)
                return ("msg.user.usernameLength", [MinUsernameLength, MaxUsernameLength]);

            if (username.Any(char.IsWhiteSpace))
                return ("msg.user.usernameSpaces", []);

            if (password.Length < MinPasswordLength)
                return ("msg.user.passwordLength", [MinPasswordLength]);

            if (password != confirmPassword)
                return ("msg.user.passwordMismatch", []);

            if (firstName.Length == 0 || lastName.Length == 0)
                return ("msg.user.namesRequired", []);

            if (email != null && !MailAddress.TryCreate(email, out _))
                return ("msg.user.emailInvalid", []);

            return null;
        }
    }
}
