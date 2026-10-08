using BLL;

namespace UI.Landing
{
    /// <summary>
    /// Página de inicio mínima. Existe para que el permiso VER_LANDING_PAGE
    /// (roles client, moderador y administrador) controle algo concreto.
    /// </summary>
    public partial class FrmLanding : Form
    {
        public FrmLanding()
        {
            InitializeComponent();

            var user = new SessionBLL().CurrentUser;
            lblBienvenida.Text = user == null
                ? "Bienvenido"
                : $"Bienvenido, {user.FirstName} {user.LastName}";
        }
    }
}
