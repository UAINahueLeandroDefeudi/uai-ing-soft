using BE.Base;

namespace BE.Entity
{
    public class User : BaseGuidEntity
    {
        public string Username { get; set; } = string.Empty;
        public byte[] PasswordHash { get; set; } = Array.Empty<byte>();
        public byte[] Salt { get; set; } = Array.Empty<byte>();
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string? Email { get; set; }
        public int FailedAttempts { get; set; }
        public bool IsBlocked { get; set; }
        public bool IsActive { get; set; }
        public DateTime? LastLoginAt { get; set; }

        /// <summary>Idioma elegido por el usuario (T05); null usa el idioma por defecto.</summary>
        public int? IdIdioma { get; set; }

        /// <summary>Roles asignados (T04). Se cargan al iniciar sesión.</summary>
        public List<Role> Roles { get; set; } = new();

        /// <summary>Alguno de sus roles otorga el permiso (búsqueda recursiva en el Composite).</summary>
        public bool HasPermission(string code) => Roles.Any(r => r.Grants(code));

        public override string ToString() => Username;
    }
}
