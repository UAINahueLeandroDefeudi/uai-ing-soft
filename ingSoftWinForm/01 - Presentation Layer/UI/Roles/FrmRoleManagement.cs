using BE.Entity;
using BE.Enum;
using BLL;

namespace UI.Roles
{
    /// <summary>
    /// Gestión de roles y permisos (T04). Los tres TreeView se llenan con funciones
    /// recursivas que recorren el Composite de permisos (PermissionTreeBuilder.MostrarRecursivo).
    /// El Tag de cada nodo guarda el objeto de negocio (Role o Permission).
    /// </summary>
    public partial class FrmRoleManagement : Form
    {
        private readonly RoleBLL roleBLL;
        private readonly SessionBLL sessionBLL;
        private readonly BitacoraBLL bitacoraBLL;

        public FrmRoleManagement()
        {
            InitializeComponent();
            roleBLL = new RoleBLL();
            sessionBLL = new SessionBLL();
            bitacoraBLL = new BitacoraBLL();
        }

        private void FrmRoleManagement_Load(object sender, EventArgs e)
        {
            // Cada grupo de botones se habilita según el permiso que exige su operación en la BLL.
            var gestionaRoles = sessionBLL.HasPermission(PermissionCode.GestionarRoles);
            var asignaRoles = sessionBLL.HasPermission(PermissionCode.AsignarRolesUsuario);

            // Lo que el usuario no puede hacer no se muestra (en vez de verse deshabilitado).
            btnCrearRol.Visible = btnEliminarRol.Visible = gestionaRoles;
            lblNombreRol.Visible = txtNombreRol.Visible = gestionaRoles;
            btnAsignarPermisoRol.Visible = btnQuitarPermisoRol.Visible = gestionaRoles;
            btnAsignarRolUsuario.Visible = btnQuitarRolUsuario.Visible = asignaRoles;

            if (!gestionaRoles)
            {
                Controls.Add(new Label
                {
                    AutoSize = false,
                    Location = new System.Drawing.Point(430, 440),
                    Size = new System.Drawing.Size(560, 50),
                    ForeColor = System.Drawing.SystemColors.GrayText,
                    Text = "Si desea gestionar roles o permisos, contáctese con un administrador."
                });
            }

            CargarTodo();
        }

        // ---------- Carga ----------

        private void CargarTodo()
        {
            try
            {
                var roles = roleBLL.GetRoles();
                var catalogo = roleBLL.GetPermissionCatalog();

                PermissionTreeBuilder.LlenarArbolRoles(tvRoles, roles);
                LlenarCatalogo(catalogo, roles);
                CargarUsuarios();
                CargarPermisosEfectivos();
            }
            catch (Exception ex)
            {
                MostrarErrorSistema("No se pudo cargar la información de roles", ex);
            }
        }

        private void CargarUsuarios()
        {
            var seleccionado = (cbUsuarios.SelectedItem as User)?.Id;

            cbUsuarios.SelectedIndexChanged -= CbUsuarios_SelectedIndexChanged;
            cbUsuarios.DataSource = roleBLL.GetUsers();
            cbUsuarios.SelectedItem = (cbUsuarios.DataSource as List<User>)?.FirstOrDefault(u => u.Id == seleccionado)
                                      ?? (cbUsuarios.Items.Count > 0 ? cbUsuarios.Items[0] : null);
            cbUsuarios.SelectedIndexChanged += CbUsuarios_SelectedIndexChanged;
        }

        private void CargarPermisosEfectivos()
        {
            if (cbUsuarios.SelectedItem is not User usuario)
            {
                tvEfectivos.Nodes.Clear();
                return;
            }

            PermissionTreeBuilder.LlenarArbolRoles(tvEfectivos, roleBLL.GetUserRoles(usuario));
        }

        /// <summary>
        /// Catálogo general: ROLES, PERMISOS COMPUESTOS (con su árbol) y PERMISOS SIMPLES.
        /// </summary>
        private void LlenarCatalogo(List<Permission> catalogo, List<Role> roles)
        {
            tvCatalogo.BeginUpdate();
            tvCatalogo.Nodes.Clear();

            var nodoRoles = new TreeNode("ROLES") { NodeFont = new Font(tvCatalogo.Font, FontStyle.Bold) };
            var nodoCompuestos = new TreeNode("PERMISOS COMPUESTOS") { NodeFont = new Font(tvCatalogo.Font, FontStyle.Bold) };
            var nodoSimples = new TreeNode("PERMISOS SIMPLES") { NodeFont = new Font(tvCatalogo.Font, FontStyle.Bold) };

            foreach (var rol in roles)
                nodoRoles.Nodes.Add(new TreeNode(rol.Name) { Tag = rol, ForeColor = Color.Blue });

            foreach (var permiso in catalogo.Where(p => p.IsCompound))
                PermissionTreeBuilder.MostrarRecursivo(nodoCompuestos, permiso);

            foreach (var permiso in catalogo.Where(p => !p.IsCompound))
                PermissionTreeBuilder.MostrarRecursivo(nodoSimples, permiso);

            tvCatalogo.Nodes.AddRange([nodoRoles, nodoCompuestos, nodoSimples]);
            tvCatalogo.ExpandAll();
            tvCatalogo.EndUpdate();
        }

        // ---------- Selección ----------

        private void CbUsuarios_SelectedIndexChanged(object? sender, EventArgs e) => CargarPermisosEfectivos();

        /// <summary>Rol al que pertenece el nodo: el nodo mismo o su ancestro raíz.</summary>
        private static Role? RolDelNodo(TreeNode? nodo)
        {
            while (nodo != null)
            {
                if (nodo.Tag is Role rol) return rol;
                nodo = nodo.Parent;
            }

            return null;
        }

        // ---------- Acciones ----------

        private void BtnCrearRol_Click(object sender, EventArgs e)
        {
            Ejecutar(() => roleBLL.CreateRole(txtNombreRol.Text), alExito: () => txtNombreRol.Clear());
        }

        private void BtnEliminarRol_Click(object sender, EventArgs e)
        {
            var rol = RolDelNodo(tvRoles.SelectedNode) ?? RolDelNodo(tvCatalogo.SelectedNode);
            if (rol == null) { Avisar("Seleccione un rol."); return; }

            var confirmar = MessageBox.Show(this, $"¿Eliminar el rol '{rol.Name}'?", "Eliminar rol",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirmar != DialogResult.Yes) return;

            Ejecutar(() => roleBLL.DeleteRole(rol));
        }

        private void BtnAsignarPermisoRol_Click(object sender, EventArgs e)
        {
            var rol = RolDelNodo(tvRoles.SelectedNode);
            if (rol == null) { Avisar("Seleccione el rol destino en la estructura de roles."); return; }

            if (tvCatalogo.SelectedNode?.Tag is not Permission permiso)
            { Avisar("Seleccione un permiso en el catálogo."); return; }

            Ejecutar(() => roleBLL.AddPermissionToRole(rol, permiso));
        }

        private void BtnQuitarPermisoRol_Click(object sender, EventArgs e)
        {
            var nodo = tvRoles.SelectedNode;
            if (nodo?.Tag is not Permission permiso || nodo.Parent?.Tag is not Role rol)
            { Avisar("Seleccione, en la estructura de roles, un permiso asignado directamente a un rol."); return; }

            Ejecutar(() => roleBLL.RemovePermissionFromRole(rol, permiso));
        }

        private void BtnAsignarRolUsuario_Click(object sender, EventArgs e)
        {
            if (cbUsuarios.SelectedItem is not User usuario) { Avisar("Seleccione un usuario."); return; }

            var rol = tvCatalogo.SelectedNode?.Tag as Role;
            if (rol == null) { Avisar("Seleccione un rol en el catálogo."); return; }

            Ejecutar(() => roleBLL.AssignRoleToUser(usuario, rol));
        }

        private void BtnQuitarRolUsuario_Click(object sender, EventArgs e)
        {
            if (cbUsuarios.SelectedItem is not User usuario) { Avisar("Seleccione un usuario."); return; }

            var rol = tvEfectivos.SelectedNode?.Tag as Role;
            if (rol == null) { Avisar("Seleccione, en los permisos efectivos, el rol a quitar."); return; }

            Ejecutar(() => roleBLL.RemoveRoleFromUser(usuario, rol));
        }

        // ---------- Soporte ----------

        /// <summary>Ejecuta la operación de la BLL, informa el resultado y refresca los árboles.</summary>
        private void Ejecutar(Func<OperationResult> operacion, Action? alExito = null)
        {
            try
            {
                var resultado = operacion();

                if (!resultado.Success)
                {
                    MessageBox.Show(this, resultado.Message, "No se pudo completar", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                alExito?.Invoke();
                CargarTodo();
            }
            catch (Exception ex)
            {
                MostrarErrorSistema("Error inesperado en la gestión de roles", ex);
            }
        }

        private void Avisar(string mensaje)
            => MessageBox.Show(this, mensaje, "Gestión de roles", MessageBoxButtons.OK, MessageBoxIcon.Information);

        private void MostrarErrorSistema(string mensaje, Exception ex)
        {
            bitacoraBLL.RegistrarError(NameEvent.ErrorSistema, $"{mensaje}: {ex.Message}", Priority.High);
            System.Diagnostics.Debug.WriteLine(ex);
            MessageBox.Show(this, $"{mensaje}. Intente nuevamente.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
