namespace BE.Entity
{
    /// <summary>
    /// Patrón Composite (T04): Component. Un permiso es simple (hoja) o compuesto
    /// (agrupa otros permisos). Los roles NO forman parte del Composite (Modelo II):
    /// un Role agrega Permission. Ver DC-permisos-composite.md.
    /// </summary>
    public abstract class Permission
    {
        public int Id { get; set; }

        /// <summary>Código estable e inmutable (ej. GESTIONAR_ROLES). Es lo que consulta el sistema.</summary>
        public string Code { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public abstract bool IsCompound { get; }

        public abstract IReadOnlyList<Permission> GetChildren();

        public abstract void Add(Permission permission);

        public abstract void Remove(Permission permission);

        /// <summary>
        /// Recursivo: este permiso, o alguno de sus descendientes, tiene ese código.
        /// Corta en el primer match.
        /// </summary>
        public bool Grants(string code)
        {
            if (Code == code) return true;

            foreach (var child in GetChildren())
                if (child.Grants(code)) return true;

            return false;
        }

        /// <summary>Recursivo: <paramref name="other"/> es este permiso o un descendiente.</summary>
        public bool Contains(Permission other) => Grants(other.Code);

        /// <summary>Recursivo: aplana el árbol en una lista de permisos simples (hojas), sin repetidos.</summary>
        public IEnumerable<Permission> Flatten()
        {
            if (!IsCompound)
            {
                yield return this;
                yield break;
            }

            foreach (var child in GetChildren())
                foreach (var leaf in child.Flatten())
                    yield return leaf;
        }

        public override string ToString() => Name;
    }
}
