using BLL;

namespace UI.Login
{
    public partial class FrmLogout : Form
    {
        private readonly SessionBLL sessionBLL;

        public FrmLogout()
        {
            InitializeComponent();
            sessionBLL = new SessionBLL();
        }

        private void BtnCancelar_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void BtnAceptar_Click(object sender, EventArgs e)
        {
            // La UI no llama al SessionManager directamente: pasa por la capa de negocio.
            sessionBLL.Logout();
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
