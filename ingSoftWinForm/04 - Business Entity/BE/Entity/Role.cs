using BE.Base;

namespace BE.Entity
{
    /// <summary>
    /// Rol (Modelo II): agrupa permisos simples y compuestos. No es parte del Composite;
    /// un usuario tiene uno o más roles.
    /// </summary>
    public class Role : BaseEntity
    {
        private readonly List<Permission> _permissions = new();

        public string Name { get; set; } = string.Empty;

        public IReadOnlyList<Permission> Permissions => _permissions.AsReadOnly();

        public void AddPermission(Permission permission)
        {
            if (_permissions.Any(p => p.Code == permission.Code)) return;
            _permissions.Add(permission);
        }

        public void RemovePermission(Permission permission)
            => _permissions.RemoveAll(p => p.Code == permission.Code);

        public bool Grants(string code) => _permissions.Any(p => p.Grants(code));

        public override string ToString() => Name;
    }
}
