using BE.Entity;
using BLL;
using UI.Idiomas;
using UI.Roles;

namespace UI.Profile
{
    /// <summary>
    /// Muestra en sólo lectura los datos del usuario de la sesión activa y, salvo que sea
    /// sólo "invitado", sus roles con los permisos que otorga cada uno (T04).
    /// </summary>
    public partial class FrmProfile : FrmTraducible
    {
        private readonly SessionBLL sessionBLL;

        public FrmProfile()
        {
            InitializeComponent();
            sessionBLL = new SessionBLL();
            MostrarDatos(sessionBLL.CurrentUser);
        }

        private void MostrarDatos(User? user)
        {
            if (user == null)
            {
                // Sin sesión no hay perfil que mostrar; los labels quedan en "-".
                MessageBox.Show(this, T("FrmProfile.sinSesion"), T("FrmProfile.Title"),
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            lblUsername.Text = user.Username;
            lblNombre.Text = $"{user.FirstName} {user.LastName}".Trim();
            lblEmail.Text = string.IsNullOrWhiteSpace(user.Email) ? "-" : user.Email;
            lblEstado.Text = DescribirEstado(user);
            lblUltimoAcceso.Text = FormatearFecha(user.LastLoginAt);
            lblAlta.Text = FormatearFecha(user.CreatedAt);

            MostrarRolesYPermisos(user);
        }

        /// <summary>
        /// Un usuario que sólo es "invitado" no ve el árbol: no tiene nada para listar más
        /// que lo básico. Con cualquier otro rol (solo o junto a invitado) se listan todos.
        /// </summary>
        private void MostrarRolesYPermisos(User user)
        {
            var soloInvitado = user.Roles.Count == 1 && user.Roles[0].Name == RoleName.Invitado;
            if (user.Roles.Count == 0 || soloInvitado) return;

            PermissionTreeBuilder.LlenarArbolRoles(tvRoles, user.Roles);

            // El formulario nace con el tamaño sin árbol; al mostrarlo se agranda y baja el botón.
            lblRolesCaption.Visible = true;
            tvRoles.Visible = true;
            ClientSize = new Size(ClientSize.Width, 468);
            btnCerrar.Top = 424;
        }

        private static string DescribirEstado(User user)
        {
            if (user.IsBlocked) return T("FrmProfile.estadoBloqueado");
            return user.IsActive ? T("FrmProfile.estadoActivo") : T("FrmProfile.estadoBaja");
        }

        private static string FormatearFecha(DateTime? fecha)
            => fecha.HasValue ? fecha.Value.ToString("dd/MM/yyyy HH:mm") : "-";

        /// <summary>El estado es un texto traducible: se vuelve a describir cuando cambia el idioma.</summary>
        protected override void OnIdiomaAplicado()
        {
            var user = sessionBLL?.CurrentUser;
            if (user != null) lblEstado.Text = DescribirEstado(user);
        }

        private void BtnCerrar_Click(object sender, EventArgs e) => Close();
    }
}
