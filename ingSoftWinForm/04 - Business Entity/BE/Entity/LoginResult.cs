using BE.Enum;

namespace BE.Entity
{
    /// <summary>
    /// Lo que devuelve SessionBLL.Login. Se usa un resultado en vez de excepciones
    /// porque una credencial mal tipeada es un caso de negocio esperable, no excepcional.
    /// </summary>
    public class LoginResult
    {
        public LoginStatus Status { get; set; }
        public User? User { get; set; }

        public bool IsValid => Status == LoginStatus.Success;

        public static LoginResult Ok(User user)
            => new LoginResult { Status = LoginStatus.Success, User = user };

        public static LoginResult Fail(LoginStatus status)
            => new LoginResult { Status = status };
    }
}
