using BE.Entity;
using BLL;

namespace UI.Profile
{
    /// <summary>
    /// Muestra en sólo lectura los datos del usuario de la sesión activa.
    /// </summary>
    public partial class FrmProfile : Form
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
                MessageBox.Show(this, "No hay una sesión iniciada.", "Mi perfil",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            lblUsername.Text = user.Username;
            lblNombre.Text = $"{user.FirstName} {user.LastName}".Trim();
            lblEmail.Text = string.IsNullOrWhiteSpace(user.Email) ? "-" : user.Email;
            lblEstado.Text = DescribirEstado(user);
            lblUltimoAcceso.Text = FormatearFecha(user.LastLoginAt);
            lblAlta.Text = FormatearFecha(user.CreatedAt);
        }

        private static string DescribirEstado(User user)
        {
            if (user.IsBlocked) return "Bloqueado";
            return user.IsActive ? "Activo" : "Dado de baja";
        }

        private static string FormatearFecha(DateTime? fecha)
            => fecha.HasValue ? fecha.Value.ToString("dd/MM/yyyy HH:mm") : "-";

        private void BtnCerrar_Click(object sender, EventArgs e) => Close();
    }
}
