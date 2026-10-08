using BE.Entity;

namespace UI.Roles
{
    /// <summary>
    /// Dibuja roles y permisos (Composite) en un TreeView mediante funciones recursivas.
    /// Lo comparten Mi perfil y Gestión de roles. El Tag de cada nodo guarda el Role o el Permission.
    /// </summary>
    public static class PermissionTreeBuilder
    {
        /// <summary>Un nodo raíz por rol; debajo, su árbol de permisos.</summary>
        public static void LlenarArbolRoles(TreeView tv, IEnumerable<Role> roles)
        {
            tv.BeginUpdate();
            tv.Nodes.Clear();

            foreach (var rol in roles)
            {
                var nodoRol = new TreeNode(rol.Name) { Tag = rol, ForeColor = Color.Blue };
                tv.Nodes.Add(nodoRol);

                foreach (var permiso in rol.Permissions)
                    MostrarRecursivo(nodoRol, permiso);
            }

            tv.ExpandAll();
            tv.EndUpdate();
        }

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
                ForeColor = permiso.IsCompound ? Color.DarkGreen : Color.Black
            };
            nodoPadre.Nodes.Add(nodo);

            foreach (var hijo in permiso.GetChildren())
                MostrarRecursivo(nodo, hijo);
        }
    }
}
