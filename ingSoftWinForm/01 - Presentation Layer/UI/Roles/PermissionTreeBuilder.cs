using BE.Entity;

namespace UI.Roles
{
    /// <summary>
    /// Dibuja roles y permisos (Composite) en un TreeView mediante funciones recursivas.
    /// Lo comparten Mi perfil y Gestión de roles. El Tag de cada nodo guarda el Permission (un rol es un Permission con IsRol).
    /// </summary>
    public static class PermissionTreeBuilder
    {
        /// <summary>Un nodo raíz por rol; debajo, su árbol de permisos.</summary>
        public static void LlenarArbolRoles(TreeView tv, IEnumerable<Permission> roles)
        {
            tv.BeginUpdate();
            tv.Nodes.Clear();

            foreach (var rol in roles)
            {
                var nodoRol = new TreeNode(rol.Name) { Tag = rol, ForeColor = Color.Blue };
                tv.Nodes.Add(nodoRol);

                foreach (var permiso in rol.GetChildren())
                    MostrarRecursivo(nodoRol, permiso);
            }

            tv.ExpandAll();
            tv.EndUpdate();
        }

        /// <summary>Rol en azul, compuesto en verde, hoja en negro.</summary>
        private static Color ColorDe(Permission permiso) => permiso switch
        {
            RolePermission => Color.Blue,
            CompoundPermission => Color.DarkGreen,
            _ => Color.Black
        };

        /// <summary>
        /// Función recursiva del Composite: agrega un nodo para <paramref name="permiso"/>
        /// bajo <paramref name="nodoPadre"/> y desciende por sus hijos. La hoja
        /// (permiso simple) devuelve una lista vacía y corta la recursión.
        /// </summary>
        public static void MostrarRecursivo(TreeNode nodoPadre, Permission permiso)
        {
            var nodo = new TreeNode(permiso.Name)
            {
                Tag = permiso,
                ForeColor = ColorDe(permiso)
            };
            nodoPadre.Nodes.Add(nodo);

            foreach (var hijo in permiso.GetChildren())
                MostrarRecursivo(nodo, hijo);
        }
    }
}
