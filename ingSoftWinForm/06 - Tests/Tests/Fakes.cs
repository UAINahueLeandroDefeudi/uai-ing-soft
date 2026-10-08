using BE.Entity;
using BE.Enum;
using DAL;

namespace Tests
{
    /// <summary>DAL en memoria: los tests de BLL no tocan la base.</summary>
    public class FakeBitacoraDAL : IBitacoraDAL
    {
        public List<Bitacora> Registros { get; } = new();

        public int Insert(Bitacora bitacora) { Registros.Add(bitacora); return Registros.Count; }
        public List<Bitacora> GetAll() => Registros;
        public List<Bitacora> GetByFilter(DateTime from, DateTime to, BitacoraType? type, NameEvent? nameEvent, Priority? priority)
            => Registros;
    }

    public class FakeUserDAL : IUserDAL
    {
        public List<User> Users { get; } = new();
        public List<(User User, string Role)> Inserted { get; } = new();

        public User? GetByUsername(string username) => Users.FirstOrDefault(u => u.Username == username);
        public List<User> GetAll() => Users;
        public bool EmailExists(string email) => Users.Any(u => u.Email == email);
        public bool Block(string username) => true;
        public void IncrementFailedAttempts(string username) { }
        public void ResetFailedAttempts(Guid id) { }

        public void Insert(User user, string roleName)
        {
            Users.Add(user);
            Inserted.Add((user, roleName));
        }
    }

    public class FakePermissionDAL : IPermissionDAL
    {
        public List<Permission> Catalog { get; } = new();

        public List<Permission> GetAll() => Catalog;
        public Permission? GetByCode(string code) => Catalog.FirstOrDefault(p => p.Code == code);
    }

    public class FakeRoleDAL : IRoleDAL
    {
        private int nextId = 100;

        public List<Role> Roles { get; } = new();
        public Dictionary<Guid, List<int>> UserRoles { get; } = new();

        public List<Role> GetAll() => Roles;

        public List<Role> GetByUser(Guid userId)
            => UserRoles.TryGetValue(userId, out var ids) ? Roles.Where(r => ids.Contains(r.Id)).ToList() : new();

        public Role? GetByName(string name) => Roles.FirstOrDefault(r => r.Name == name);

        public int Insert(Role role, string createdBy)
        {
            role.Id = nextId++;
            Roles.Add(role);
            return role.Id;
        }

        public void Delete(int roleId) => Roles.RemoveAll(r => r.Id == roleId);
        public bool HasUsers(int roleId) => UserRoles.Values.Any(ids => ids.Contains(roleId));
        public void AddPermission(int roleId, int permissionId) { }
        public void RemovePermission(int roleId, int permissionId) { }

        public void AssignToUser(Guid userId, int roleId)
        {
            if (!UserRoles.TryGetValue(userId, out var ids)) UserRoles[userId] = ids = new();
            if (!ids.Contains(roleId)) ids.Add(roleId);
        }

        public void RemoveFromUser(Guid userId, int roleId)
        {
            if (UserRoles.TryGetValue(userId, out var ids)) ids.Remove(roleId);
        }
    }

    /// <summary>Armado de los árboles de prueba (mismos códigos que el seed SQL).</summary>
    public static class Samples
    {
        public static SimplePermission Simple(string code, int id = 0)
            => new() { Id = id, Code = code, Name = code };

        public static CompoundPermission Compound(string code, params Permission[] children)
        {
            var compound = new CompoundPermission { Code = code, Name = code };
            foreach (var child in children) compound.Add(child);
            return compound;
        }

        public static Role RoleWith(string name, int id, params Permission[] permissions)
        {
            var role = new Role { Id = id, Name = name };
            foreach (var permission in permissions) role.AddPermission(permission);
            return role;
        }

        public static User UserWith(string username, params Role[] roles)
            => new() { Username = username, FirstName = "N", LastName = "A", IsActive = true, Roles = roles.ToList() };
    }
}
