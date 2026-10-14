namespace BE.Entity
{
    /// <summary>Patrón Composite: Composite. Agrupa permisos simples y/u otros compuestos.</summary>
    public class CompoundPermission : Permission
    {
        private readonly List<Permission> _children = new();

        public override IReadOnlyList<Permission> GetChildren() => _children.AsReadOnly();

        /// <summary>No admite duplicados ni ciclos (un compuesto no puede contenerse a sí mismo).</summary>
        public override void Add(Permission permission)
        {
            if (permission.Contains(this))
                throw new InvalidOperationException(
                    $"Agregar '{permission.Name}' a '{Name}' generaría un ciclo.");

            if (_children.Any(c => c.Code == permission.Code)) return;

            _children.Add(permission);
        }

        public override void Remove(Permission permission)
            => _children.RemoveAll(c => c.Code == permission.Code);

        public override void LoadChildren(ILookup<int, Permission> childrenByParent)
        {
            foreach (var child in childrenByParent[Id])
                Add(child);
        }

        public override IEnumerable<Permission> Flatten()
            => _children.SelectMany(child => child.Flatten());
    }
}
