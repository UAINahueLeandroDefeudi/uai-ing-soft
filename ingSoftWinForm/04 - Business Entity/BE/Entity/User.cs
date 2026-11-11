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

        // Campos con control de cambios (se versionan en [User_History]; el Username de login no).
        public string? NickName { get; set; }
        public string? Pais { get; set; }
        public string? Provincia { get; set; }
        public string? Direccion { get; set; }

        /// <summary>Número de la última versión registrada en [User_History] (concurrencia optimista).</summary>
        public int Version { get; set; } = 1;
        public int FailedAttempts { get; set; }
        public bool IsBlocked { get; set; }
        public bool IsActive { get; set; }
        public DateTime? LastLoginAt { get; set; }

        /// <summary>Idioma elegido por el usuario (T05); null usa el idioma por defecto.</summary>
        public int? IdIdioma { get; set; }

        /// <summary>Permisos asignados (T04): roles, o permisos sueltos. Se cargan al iniciar sesión.</summary>
        public List<Permission> Permissions { get; set; } = new();

        /// <summary>Los asignados que son roles (<see cref="RolePermission"/>).</summary>
        public IReadOnlyList<Permission> Roles => Permissions.OfType<RolePermission>().ToList<Permission>();

        /// <summary>Alguno de sus permisos otorga el código (búsqueda recursiva en el Composite).</summary>
        public bool HasPermission(string code) => Permissions.Any(p => p.Grants(code));

        public override string ToString() => Username;
    }
}
