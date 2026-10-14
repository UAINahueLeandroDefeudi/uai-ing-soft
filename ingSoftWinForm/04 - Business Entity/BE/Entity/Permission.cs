namespace BE.Entity
{
    /// <summary>
    /// Patrón Composite (T04): Component. Un permiso es simple (hoja), compuesto
    /// (agrupa otros permisos) o rol (compuesto que se asigna a usuarios, Modelo I).
    /// El cliente nunca pregunta el tipo: cada clase resuelve por sí misma Grants,
    /// Flatten y LoadChildren. Ver DC-permisos-composite.md.
    /// </summary>
    public abstract class Permission
    {
        public int Id { get; set; }

        /// <summary>Código estable e inmutable (ej. GESTIONAR_ROLES). Es lo que consulta el sistema.</summary>
        public string Code { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        /// <summary>Se asigna a usuarios. Sólo lo redefine <see cref="RolePermission"/>.</summary>
        public virtual bool IsRol => false;

        public abstract IReadOnlyList<Permission> GetChildren();

        public abstract void Add(Permission permission);

        public abstract void Remove(Permission permission);

        /// <summary>
        /// Carga los hijos de este nodo a partir de las relaciones padre → hijos leídas de
        /// la base. La hoja no hace nada; el compuesto agrega los suyos. Así la DAL recorre
        /// todo el catálogo sin preguntar quién es compuesto.
        /// </summary>
        public abstract void LoadChildren(ILookup<int, Permission> childrenByParent);

        /// <summary>Recursivo: aplana el árbol en una lista de permisos simples (hojas).</summary>
        public abstract IEnumerable<Permission> Flatten();

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

        public override string ToString() => Name;
    }
}
