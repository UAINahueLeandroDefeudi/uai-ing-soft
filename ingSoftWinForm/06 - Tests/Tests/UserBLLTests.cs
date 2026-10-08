using BE.Entity;
using BE.Enum;
using BLL;
using Services;
using Xunit;
using static Tests.Samples;

namespace Tests
{
    [Collection("Session")]
    public class UserBLLTests : IDisposable
    {
        private readonly FakeUserDAL userDAL = new();
        private readonly FakeRoleDAL roleDAL = new();
        private readonly FakeBitacoraDAL bitacoraDAL = new();
        private readonly UserBLL userBLL;

        public UserBLLTests()
        {
            if (SessionManager.IsLoggedIn()) SessionManager.Logout();
            userBLL = new UserBLL(userDAL, new BitacoraBLL(bitacoraDAL));
        }

        public void Dispose()
        {
            if (SessionManager.IsLoggedIn()) SessionManager.Logout();
        }

        [Fact]
        public void Register_Valid_CreatesUserWithInvitadoRoleAndHashedPassword()
        {
            var result = userBLL.Register("pepe", "Clave1234", "Clave1234", "Jose", "Perez", "pepe@if.local");

            Assert.True(result.Success);
            var (user, role) = Assert.Single(userDAL.Inserted);
            Assert.Equal(RoleName.Invitado, role);
            Assert.True(HashManager.VerifyPassword("Clave1234", user.Salt, user.PasswordHash));
            Assert.Contains(bitacoraDAL.Registros, b => b.NameEvent == NameEvent.CrearUsuario);
        }

        [Fact]
        public void Register_DuplicateUsername_Fails()
        {
            userDAL.Users.Add(UserWith("pepe"));

            var result = userBLL.Register("pepe", "Clave1234", "Clave1234", "Jose", "Perez", null);

            Assert.False(result.Success);
            Assert.Empty(userDAL.Inserted);
        }

        [Fact]
        public void Register_DuplicateEmail_Fails()
        {
            var existing = UserWith("otro");
            existing.Email = "pepe@if.local";
            userDAL.Users.Add(existing);

            Assert.False(userBLL.Register("pepe", "Clave1234", "Clave1234", "Jose", "Perez", "pepe@if.local").Success);
        }

        [Theory]
        [InlineData("pepe", "corta", "corta", "Jose", "Perez", null)]
        [InlineData("pepe", "Clave1234", "Otra12345", "Jose", "Perez", null)]
        [InlineData("pe", "Clave1234", "Clave1234", "Jose", "Perez", null)]
        [InlineData("pe pe", "Clave1234", "Clave1234", "Jose", "Perez", null)]
        [InlineData("pepe", "Clave1234", "Clave1234", "", "Perez", null)]
        [InlineData("pepe", "Clave1234", "Clave1234", "Jose", "Perez", "no-es-mail")]
        public void Register_Invalid_Fails(string username, string pwd, string confirm, string first, string last, string? email)
        {
            Assert.False(userBLL.Register(username, pwd, confirm, first, last, email).Success);
            Assert.Empty(userDAL.Inserted);
        }

        [Fact]
        public void Login_LoadsRolesBeforeOpeningSession()
        {
            var salt = HashManager.GenerateSalt();
            var user = new User
            {
                Id = Guid.NewGuid(), Username = "ana", FirstName = "Ana", LastName = "L", IsActive = true,
                Salt = salt, PasswordHash = HashManager.HashPassword("Clave1234", salt)
            };
            userDAL.Users.Add(user);

            var rol = RoleWith("client", 3, Compound("BASIC", Simple(PermissionCode.VerMiPerfil)));
            roleDAL.Roles.Add(rol);
            roleDAL.AssignToUser(user.Id, rol.Id);

            var sessionBLL = new SessionBLL(userDAL, new BitacoraBLL(bitacoraDAL), roleDAL);
            var result = sessionBLL.Login("ana", "Clave1234");

            Assert.True(result.IsValid);
            Assert.True(sessionBLL.HasPermission(PermissionCode.VerMiPerfil));
            Assert.False(sessionBLL.HasPermission(PermissionCode.GestionarRoles));
            Assert.Contains("client", bitacoraDAL.Registros.Last().RolesPermisos);
        }
    }
}
