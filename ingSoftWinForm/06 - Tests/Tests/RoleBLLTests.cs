using BE.Entity;
using BE.Enum;
using BLL;
using Services;
using Xunit;
using static Tests.Samples;

namespace Tests
{
    [Collection("Session")]
    public class RoleBLLTests : IDisposable
    {
        private readonly FakeRoleDAL roleDAL = new();
        private readonly FakePermissionDAL permissionDAL = new();
        private readonly FakeUserDAL userDAL = new();
        private readonly FakeBitacoraDAL bitacoraDAL = new();
        private readonly RoleBLL bll;
        private readonly User admin;

        public RoleBLLTests()
        {
            if (SessionManager.IsLoggedIn()) SessionManager.Logout();

            bll = new RoleBLL(roleDAL, permissionDAL, userDAL, new BitacoraBLL(bitacoraDAL));
            admin = UserWith("admin",
                RoleWith("administrador", 1, Simple(PermissionCode.GestionarRoles), Simple(PermissionCode.AsignarRolesUsuario)));
            admin.Id = Guid.NewGuid();
        }

        public void Dispose()
        {
            if (SessionManager.IsLoggedIn()) SessionManager.Logout();
        }

        [Fact]
        public void CreateRole_WithoutPermission_IsDeniedAndAudited()
        {
            SessionManager.Login(UserWith("invitado", RoleWith("invitado", 2, Simple(PermissionCode.VerMiPerfil))));

            var result = bll.CreateRole("nuevo");

            Assert.False(result.Success);
            Assert.Empty(roleDAL.Roles);
            Assert.Contains(bitacoraDAL.Registros, b => b.NameEvent == NameEvent.AccesoNoAutorizado);
        }

        [Fact]
        public void CreateRole_AsAdmin_Succeeds()
        {
            SessionManager.Login(admin);

            var result = bll.CreateRole("  auditor ");

            Assert.True(result.Success);
            Assert.Equal("auditor", Assert.Single(roleDAL.Roles).Name);
            Assert.Contains(bitacoraDAL.Registros, b => b.NameEvent == NameEvent.CrearRol);
        }

        [Fact]
        public void CreateRole_DuplicateName_Fails()
        {
            SessionManager.Login(admin);
            roleDAL.Roles.Add(RoleWith("auditor", 5));

            Assert.False(bll.CreateRole("auditor").Success);
        }

        [Fact]
        public void CreateRole_EmptyName_Fails()
        {
            SessionManager.Login(admin);
            Assert.False(bll.CreateRole("   ").Success);
        }

        [Fact]
        public void DeleteRole_Invitado_IsBlocked()
        {
            SessionManager.Login(admin);
            var invitado = RoleWith(RoleName.Invitado, 2);
            roleDAL.Roles.Add(invitado);

            Assert.False(bll.DeleteRole(invitado).Success);
            Assert.Single(roleDAL.Roles);
        }

        [Fact]
        public void DeleteRole_WithUsers_IsBlocked()
        {
            SessionManager.Login(admin);
            var rol = RoleWith("client", 3);
            roleDAL.Roles.Add(rol);
            roleDAL.AssignToUser(Guid.NewGuid(), rol.Id);

            Assert.False(bll.DeleteRole(rol).Success);
            Assert.Single(roleDAL.Roles);
        }

        [Fact]
        public void DeleteRole_WithoutUsers_Succeeds()
        {
            SessionManager.Login(admin);
            var rol = RoleWith("temporal", 4);
            roleDAL.Roles.Add(rol);

            Assert.True(bll.DeleteRole(rol).Success);
            Assert.Empty(roleDAL.Roles);
        }

        [Fact]
        public void AddPermissionToRole_AlreadyGrantedThroughCompound_Fails()
        {
            SessionManager.Login(admin);
            var rol = RoleWith("client", 3, Compound("BASIC", Simple("PROFILE")));

            Assert.False(bll.AddPermissionToRole(rol, Simple("PROFILE")).Success);
        }

        [Fact]
        public void AddPermissionToRole_New_Succeeds()
        {
            SessionManager.Login(admin);
            var rol = RoleWith("client", 3);

            Assert.True(bll.AddPermissionToRole(rol, Simple("LANDING")).Success);
            Assert.True(rol.Grants("LANDING"));
        }

        [Fact]
        public void RemovePermissionFromRole_NotDirect_Fails()
        {
            SessionManager.Login(admin);
            var rol = RoleWith("client", 3, Compound("BASIC", Simple("PROFILE")));

            Assert.False(bll.RemovePermissionFromRole(rol, Simple("PROFILE")).Success);
            Assert.True(rol.Grants("PROFILE"));
        }

        [Fact]
        public void RemovePermissionFromRole_Direct_Succeeds()
        {
            SessionManager.Login(admin);
            var rol = RoleWith("client", 3, Simple("LANDING"));

            Assert.True(bll.RemovePermissionFromRole(rol, Simple("LANDING")).Success);
            Assert.False(rol.Grants("LANDING"));
        }

        [Fact]
        public void AssignRoleToUser_ThenDuplicate_Fails()
        {
            SessionManager.Login(admin);
            var rol = RoleWith("client", 3);
            roleDAL.Roles.Add(rol);
            var target = UserWith("pepe");
            target.Id = Guid.NewGuid();

            Assert.True(bll.AssignRoleToUser(target, rol).Success);
            Assert.False(bll.AssignRoleToUser(target, rol).Success);
        }

        [Fact]
        public void RemoveRoleFromUser_OwnAdminRole_IsBlocked()
        {
            SessionManager.Login(admin);
            var rolAdmin = admin.Roles[0];
            roleDAL.Roles.Add(rolAdmin);
            roleDAL.AssignToUser(admin.Id, rolAdmin.Id);

            Assert.False(bll.RemoveRoleFromUser(admin, rolAdmin).Success);
        }
    }
}
