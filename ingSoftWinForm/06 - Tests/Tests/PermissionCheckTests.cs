using BE.Entity;
using BE.Enum;
using Services;
using Xunit;
using static Tests.Samples;

namespace Tests
{
    /// <summary>
    /// SessionManager es un Singleton con estado estático: los tests que abren sesión
    /// comparten la colección "Session" para no correr en paralelo entre sí.
    /// </summary>
    [Collection("Session")]
    public class PermissionCheckTests : IDisposable
    {
        public PermissionCheckTests()
        {
            if (SessionManager.IsLoggedIn()) SessionManager.Logout();
        }

        public void Dispose()
        {
            if (SessionManager.IsLoggedIn()) SessionManager.Logout();
        }

        [Fact]
        public void User_HasPermission_Direct()
        {
            var user = UserWith("u", RoleWith("r", 1, Simple("A")));
            Assert.True(user.HasPermission("A"));
        }

        [Fact]
        public void User_HasPermission_NestedInCompound()
        {
            var user = UserWith("u", RoleWith("r", 1, Compound("BASIC", Simple("PROFILE"))));
            Assert.True(user.HasPermission("PROFILE"));
        }

        [Fact]
        public void User_HasPermission_AcrossMultipleRoles()
        {
            var user = UserWith("u", RoleWith("r1", 1, Simple("A")), RoleWith("r2", 2, Simple("B")));

            Assert.True(user.HasPermission("A"));
            Assert.True(user.HasPermission("B"));
        }

        [Fact]
        public void User_HasPermission_False_WhenAbsent()
        {
            var user = UserWith("u", RoleWith("r", 1, Simple("A")));
            Assert.False(user.HasPermission("B"));
        }

        [Fact]
        public void SessionManager_HasPermission_UsesSessionUser()
        {
            SessionManager.Login(UserWith("u", RoleWith("r", 1, Compound("BASIC", Simple("PROFILE")))));

            Assert.True(SessionManager.HasPermission("PROFILE"));
            Assert.False(SessionManager.HasPermission("ADMIN"));
        }

        [Fact]
        public void SessionManager_HasPermission_False_WithoutSession()
            => Assert.False(SessionManager.HasPermission("PROFILE"));

        [Fact]
        public void Bitacora_FlattensRolesAndPermissions()
        {
            var user = UserWith("u", RoleWith("client", 1, Compound("BASIC", Simple("PROFILE")), Simple("LANDING")));

            var registro = BitacoraManager.EventoBitacora(NameEvent.Login, "x", Priority.Low, user);

            Assert.Equal("Roles: client | Permisos: PROFILE, LANDING", registro.RolesPermisos);
        }
    }
}
