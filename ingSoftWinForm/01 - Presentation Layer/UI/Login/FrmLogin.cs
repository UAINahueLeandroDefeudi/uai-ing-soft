using BE.Enum;
using BLL;

namespace UI.Login
{
    public partial class FrmLogin : Form
    {
        private readonly SessionBLL sessionBLL;

        public FrmLogin()
        {
            InitializeComponent();
            sessionBLL = new SessionBLL();
        }

        private void BtnAceptar_Click(object sender, EventArgs e)
        {
            lblError.Text = string.Empty;

            try
            {
                var resultado = sessionBLL.Login(txtUsername.Text.Trim(), txtPassword.Text);

                switch (resultado.Status)
                {
                    case LoginStatus.Success:
                        this.DialogResult = DialogResult.OK;
                        this.Close();
                        break;

                    case LoginStatus.UserBlocked:
                        MostrarError("El usuario está bloqueado. Contacte al administrador.");
                        break;

                    case LoginStatus.UserInactive:
                        MostrarError("El usuario está dado de baja.");
                        break;

                    case LoginStatus.SessionAlreadyOpen:
                        MostrarError("Ya hay una sesión iniciada.");
                        break;

                    default:
                        // Mismo mensaje para usuario inexistente y contraseña incorrecta.
                        MostrarError("Usuario o contraseña incorrectos.");
                        break;
                }
            }
            catch (Exception ex)
            {
                // FA-5: sin conexión a la base de datos.
                MostrarError("No se pudo conectar con el servidor. Intente nuevamente.");
                System.Diagnostics.Debug.WriteLine(ex);
            }
        }

        private void BtnCancelar_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void MostrarError(string mensaje)
        {
            lblError.Text = mensaje;
            txtPassword.Clear();
            txtPassword.Focus();
        }

        private void lblError_Click(object sender, EventArgs e)
        {

        }
    }
}
