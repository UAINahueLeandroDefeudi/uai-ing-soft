using BE.Entity;
using BLL;
using UI.Event;
using UI.Inicio;
using UI.Login;
using UI.Profile;
using UI.Roles;

namespace UI
{
    /// <summary>
    /// Contenedor MDI de la aplicación. Los formularios de gestión se abren como
    /// ventanas hijas; el login y el logout siguen siendo diálogos modales aparte.
    /// </summary>
    public partial class FrmMain : Form
    {
        private readonly SessionBLL sessionBLL;

        public FrmMain()
        {
            InitializeComponent();
            sessionBLL = new SessionBLL();
            MostrarUsuarioEnSesion();
            AplicarPermisos();
        }

        private void FrmMain_Load(object sender, EventArgs e)
        {
            // El perfil arranca abierto como ventana hija: es la primera pantalla
            // que ve el usuario al entrar. Va en el Load y no en el constructor
            // porque el contenedor MDI todavía no tiene handle creado.
            // T04: sólo si el rol del usuario le permite ver su perfil.
            if (sessionBLL.HasPermission(PermissionCode.VerMiPerfil))
                AbrirHijo<FrmProfile>();
        }

        /// <summary>
        /// T04: habilita sólo las opciones para las que el usuario tiene permiso
        /// (CU-01). La BLL vuelve a validar al operar: ocultar el menú no es seguridad.
        /// </summary>
        private void AplicarPermisos()
        {
            mnuInicio.Visible = sessionBLL.HasPermission(PermissionCode.VerInicio);
            mnuPerfil.Visible = sessionBLL.HasPermission(PermissionCode.VerMiPerfil);
            mnuEvent.Visible = sessionBLL.HasPermission(PermissionCode.VerBitacora);
            // La pantalla sirve a dos permisos: gestionar roles y asignarlos a usuarios.
            // Cada botón del form se habilita por separado según el permiso que exige.
            mnuRoles.Visible = sessionBLL.HasPermission(PermissionCode.GestionarRoles)
                            || sessionBLL.HasPermission(PermissionCode.AsignarRolesUsuario);
            mnuCerrarSesion.Visible = sessionBLL.HasPermission(PermissionCode.CerrarSesion);
        }

        /// <summary>
        /// Abre el formulario como ventana hija. Si ya estaba abierto lo trae al
        /// frente en vez de duplicarlo.
        /// </summary>
        public void AbrirHijo<TForm>() where TForm : Form, new()
        {
            var abierto = MdiChildren.OfType<TForm>().FirstOrDefault();

            if (abierto != null)
            {
                if (abierto.WindowState == FormWindowState.Minimized)
                    abierto.WindowState = FormWindowState.Normal;

                abierto.Activate();
                return;
            }

            var frm = new TForm { MdiParent = this };
            frm.Show();
        }

        private void MostrarUsuarioEnSesion()
        {
            var user = sessionBLL.CurrentUser;

            lblUsuario.Text = user == null
                ? "Sin sesión"
                : $"Usuario: {user.Username} ({user.FirstName} {user.LastName})";
        }

        private void MnuInicio_Click(object sender, EventArgs e) => AbrirHijo<FrmInicio>();

        private void MnuPerfil_Click(object sender, EventArgs e) => AbrirHijo<FrmProfile>();

        private void MnuEvent_Click(object sender, EventArgs e) => AbrirHijo<FrmEvent>();

        private void MnuRoles_Click(object sender, EventArgs e) => AbrirHijo<FrmRoleManagement>();

        private void MnuCerrarSesion_Click(object sender, EventArgs e)
        {
            using var logout = new FrmLogout();
            if (logout.ShowDialog(this) != DialogResult.OK) return;

            // Cerrada la sesión no queda nada operable: se cierra el MDI y con él la app.
            Close();
        }

        private void MnuSalir_Click(object sender, EventArgs e) => Close();

        private void MnuCascada_Click(object sender, EventArgs e)
            => LayoutMdi(MdiLayout.Cascade);

        private void MnuMosaicoHorizontal_Click(object sender, EventArgs e)
            => LayoutMdi(MdiLayout.TileHorizontal);

        private void MnuMosaicoVertical_Click(object sender, EventArgs e)
            => LayoutMdi(MdiLayout.TileVertical);

        private void MnuCerrarTodas_Click(object sender, EventArgs e)
        {
            // Se copia la colección: cerrar un hijo la modifica mientras se recorre.
            foreach (var hijo in MdiChildren.ToList())
                hijo.Close();
        }
    }
}
