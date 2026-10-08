using BLL;

namespace UI.Inicio
{
    /// <summary>
    /// Página de inicio mínima. Existe para que el permiso VER_INICIO
    /// (roles client, moderador y administrador) controle algo concreto.
    /// </summary>
    public partial class FrmInicio : Form
    {
        public FrmInicio()
        {
            InitializeComponent();

            var user = new SessionBLL().CurrentUser;
            lblBienvenida.Text = user == null
                ? "Bienvenido"
                : $"Bienvenido, {user.FirstName} {user.LastName}";
        }
    }
}
