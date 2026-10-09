using BE.Entity;
using BLL;
using UI.Event;
using UI.Idiomas;
using UI.Inicio;
using UI.Login;
using UI.Profile;
using UI.Roles;

namespace UI
{
    /// <summary>
    /// Contenedor MDI de la aplicación. Los formularios de gestión se abren como
    /// ventanas hijas; el login y el logout siguen siendo diálogos modales aparte.
    /// T05: es un FrmTraducible; el menú Idioma se arma con los idiomas activos de la base.
    /// </summary>
    public partial class FrmMain : FrmTraducible
    {
        private readonly SessionBLL sessionBLL;

        /// <summary>true si el MDI se cerró por un cierre de sesión (y no por "Salir"): Program muestra el login de nuevo.</summary>
        public bool SesionCerrada { get; private set; }

        // Creados por código (no están en el Designer): el Name es la clave de traducción.
        private readonly ToolStripMenuItem mnuIdioma = new() { Name = "mnuIdioma" };
        private readonly ToolStripMenuItem mnuIdiomas = new() { Name = "mnuIdiomas" };
        private readonly ToolStripSeparator sepIdioma = new() { Name = "sepIdioma" };

        public FrmMain()
        {
            InitializeComponent();
            sessionBLL = new SessionBLL();
            ArmarMenuIdioma();
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

            // T05: cambiar de idioma es libre; administrarlos exige el permiso.
            var gestionaIdiomas = sessionBLL.HasPermission(PermissionCode.GestionarIdiomas);
            mnuIdiomas.Visible = sepIdioma.Visible = gestionaIdiomas;
        }

        // ---------- Idioma (T05) ----------

        private void ArmarMenuIdioma()
        {
            mnuIdiomas.Click += MnuIdiomas_Click;
            mnuIdioma.DropDownItems.Add(sepIdioma);
            mnuIdioma.DropDownItems.Add(mnuIdiomas);

            // Entre "Sesión" y "Ventana".
            menuStrip.Items.Insert(1, mnuIdioma);
        }

        /// <summary>
        /// Muestra un ítem por idioma activo, con tilde en el que está en uso. Sólo se reconstruyen
        /// los ítems si cambió el conjunto de idiomas: así un cambio de idioma (que se dispara desde
        /// el click de uno de estos ítems) no los elimina mientras se está procesando ese click.
        /// </summary>
        private void RefrescarMenuIdioma()
        {
            List<Idioma> activos;
            try { activos = Idiomas.GetIdiomasActivos(); }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine(ex); return; }

            var actuales = mnuIdioma.DropDownItems.OfType<ToolStripMenuItem>()
                .Where(i => i.Tag is Idioma).ToList();

            if (!actuales.Select(i => ((Idioma)i.Tag!).Codigo).SequenceEqual(activos.Select(i => i.Codigo)))
            {
                foreach (var item in actuales) mnuIdioma.DropDownItems.Remove(item);

                var posicion = 0;
                foreach (var idioma in activos)
                {
                    var item = new ToolStripMenuItem { Tag = idioma, Name = $"mnuIdioma_{idioma.Codigo}" };
                    item.Click += MnuElegirIdioma_Click;
                    mnuIdioma.DropDownItems.Insert(posicion++, item);
                }

                actuales = mnuIdioma.DropDownItems.OfType<ToolStripMenuItem>().Where(i => i.Tag is Idioma).ToList();
            }

            var codigoActivo = Idiomas.IdiomaActivo?.Codigo;
            foreach (var item in actuales)
            {
                var idioma = activos.First(i => i.Codigo == ((Idioma)item.Tag!).Codigo);
                item.Tag = idioma;
                item.Text = idioma.Nombre;
                item.Checked = idioma.Codigo == codigoActivo;
            }
        }

        private void MnuElegirIdioma_Click(object? sender, EventArgs e)
        {
            if (sender is not ToolStripMenuItem { Tag: Idioma idioma }) return;

            var resultado = Idiomas.CambiarIdioma(idioma.Codigo);
            if (!resultado.Success)
                MessageBox.Show(this, T(resultado), T("FrmMain.Title"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        private void MnuIdiomas_Click(object? sender, EventArgs e) => AbrirHijo<FrmIdiomas>();

        /// <summary>Menú de idiomas y barra de estado: textos que arma el código.</summary>
        protected override void OnIdiomaAplicado()
        {
            RefrescarMenuIdioma();
            MostrarUsuarioEnSesion();
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
            var user = sessionBLL?.CurrentUser;

            lblUsuario.Text = user == null
                ? T("FrmMain.sinSesion")
                : T("FrmMain.usuarioFmt", user.Username, user.FirstName, user.LastName);
        }

        private void MnuInicio_Click(object sender, EventArgs e) => AbrirHijo<FrmInicio>();

        private void MnuPerfil_Click(object sender, EventArgs e) => AbrirHijo<FrmProfile>();

        private void MnuEvent_Click(object sender, EventArgs e) => AbrirHijo<FrmEvent>();

        private void MnuRoles_Click(object sender, EventArgs e) => AbrirHijo<FrmRoleManagement>();

        private void MnuCerrarSesion_Click(object sender, EventArgs e)
        {
            using var logout = new FrmLogout();
            if (logout.ShowDialog(this) != DialogResult.OK) return;

            // Cerrada la sesión no queda nada operable: se cierra el MDI y Program vuelve al login.
            SesionCerrada = true;
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
