namespace BE.Entity
{
    /// <summary>Patrón Composite: Leaf. No admite hijos.</summary>
    public class SimplePermission : Permission
    {
        public override IReadOnlyList<Permission> GetChildren() => Array.Empty<Permission>();

        public override void Add(Permission permission)
            => throw new InvalidOperationException("No se pueden agregar hijos a un permiso simple.");

        public override void Remove(Permission permission)
            => throw new InvalidOperationException("No se pueden remover hijos de un permiso simple.");

        /// <summary>Hoja: no tiene hijos que cargar.</summary>
        public override void LoadChildren(ILookup<int, Permission> childrenByParent) { }

        public override IEnumerable<Permission> Flatten()
        {
            yield return this;
        }
    }
}
