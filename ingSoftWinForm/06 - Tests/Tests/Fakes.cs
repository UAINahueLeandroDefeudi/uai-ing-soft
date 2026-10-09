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

    /// <summary>Idiomas y textos en memoria. Los textos se cargan por código de idioma y clave.</summary>
    public class FakeIdiomaDAL : IIdiomaDAL
    {
        private int nextId = 10;

        public List<Idioma> Idiomas { get; } = new()
        {
            new Idioma { Id = 1, Codigo = "es", Nombre = "Español", EsDefault = true, Activo = true },
            new Idioma { Id = 2, Codigo = "en", Nombre = "English", EsDefault = false, Activo = true }
        };

        public Dictionary<string, string> Etiquetas { get; } = new();                    // clave -> id lógico
        public Dictionary<(string Clave, int IdIdioma), string> Textos { get; } = new();
        public Dictionary<Guid, int> IdiomaPorUsuario { get; } = new();

        public int GetTextosCalls { get; private set; }

        public void Texto(string clave, int idIdioma, string texto)
        {
            Etiquetas[clave] = clave;
            Textos[(clave, idIdioma)] = texto;
        }

        public List<Idioma> GetAll() => Idiomas;
        public Idioma? GetById(int id) => Idiomas.FirstOrDefault(i => i.Id == id);
        public Idioma? GetByCodigo(string codigo) => Idiomas.FirstOrDefault(i => i.Codigo == codigo);
        public Idioma? GetDefault() => Idiomas.FirstOrDefault(i => i.EsDefault);

        public int Insert(Idioma idioma)
        {
            idioma.Id = nextId++;
            idioma.EsDefault = false;
            idioma.Activo = true;
            Idiomas.Add(idioma);
            return idioma.Id;
        }

        public void Update(Idioma idioma) { }

        public Dictionary<string, string> GetTextos(int idIdioma)
        {
            GetTextosCalls++;
            return Textos.Where(t => t.Key.IdIdioma == idIdioma).ToDictionary(t => t.Key.Clave, t => t.Value);
        }

        public List<TraduccionItem> GetTraducciones(int idIdioma)
        {
            var porDefecto = GetDefault()!.Id;
            return Etiquetas.Keys.Select((clave, i) => new TraduccionItem
            {
                IdEtiqueta = i + 1,
                Clave = clave,
                TextoDefault = Textos.GetValueOrDefault((clave, porDefecto)) ?? string.Empty,
                Texto = Textos.GetValueOrDefault((clave, idIdioma))
            }).ToList();
        }

        public void UpsertTraduccion(int idEtiqueta, int idIdioma, string? texto)
        {
            var clave = Etiquetas.Keys.ElementAt(idEtiqueta - 1);
            if (texto == null) Textos.Remove((clave, idIdioma));
            else Textos[(clave, idIdioma)] = texto;
        }

        public void SetUserIdioma(Guid userId, int idIdioma) => IdiomaPorUsuario[userId] = idIdioma;
    }

    /// <summary>Suscriptor de prueba: guarda cada idioma que le notifican.</summary>
    public class SuscriberEspia : BE.Observer.ISuscriberIdioma
    {
        public List<string> Recibidos { get; } = new();
        public void Actualizar(Idioma idiomaActivo) => Recibidos.Add(idiomaActivo.Codigo);
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
