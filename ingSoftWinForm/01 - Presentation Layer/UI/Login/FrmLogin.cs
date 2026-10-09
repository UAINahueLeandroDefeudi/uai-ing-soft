using BE.Entity;
using BE.Enum;
using BLL;
using UI.Idiomas;

namespace UI.Login
{
    public partial class FrmLogin : FrmTraducible
    {
        private readonly SessionBLL sessionBLL;
        private bool cargandoIdiomas;

        public FrmLogin()
        {
            InitializeComponent();
            sessionBLL = new SessionBLL();
            cboIdioma.SelectedIndexChanged += CboIdioma_SelectedIndexChanged;
        }

        protected override void OnLoad(EventArgs e)
        {
            CargarIdiomas();
            base.OnLoad(e);
        }

        /// <summary>Antes del login se elige el idioma de la pantalla; después manda el guardado en el usuario.</summary>
        private void CargarIdiomas()
        {
            cargandoIdiomas = true;
            try
            {
                var activos = Idiomas.GetIdiomasActivos();
                cboIdioma.DataSource = activos;
                cboIdioma.SelectedItem = activos.FirstOrDefault(i => i.Id == Idiomas.IdiomaActivo?.Id);
            }
            catch (Exception ex)
            {
                // Sin base no hay idiomas para ofrecer; el login mostrará su error de conexión.
                cboIdioma.Enabled = false;
                System.Diagnostics.Debug.WriteLine(ex);
            }
            finally
            {
                cargandoIdiomas = false;
            }
        }

        private void CboIdioma_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (cargandoIdiomas || cboIdioma.SelectedItem is not Idioma idioma) return;

            Idiomas.CambiarIdioma(idioma.Codigo);
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
                        // Se aplica el idioma guardado del usuario (o el por defecto).
                        Idiomas.AplicarIdiomaDe(resultado.User!);
                        this.DialogResult = DialogResult.OK;
                        this.Close();
                        break;

                    case LoginStatus.UserBlocked:
                        MostrarError(T("msg.login.blocked"));
                        break;

                    case LoginStatus.UserInactive:
                        MostrarError(T("msg.login.inactive"));
                        break;

                    case LoginStatus.SessionAlreadyOpen:
                        MostrarError(T("msg.login.sessionOpen"));
                        break;

                    default:
                        // Mismo mensaje para usuario inexistente y contraseña incorrecta.
                        MostrarError(T("msg.login.invalid"));
                        break;
                }
            }
            catch (Exception ex)
            {
                // FA-5: sin conexión a la base de datos.
                MostrarError(T("msg.login.noConnection"));
                System.Diagnostics.Debug.WriteLine(ex);
            }
        }

        private void BtnCancelar_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        /// <summary>T04: alta abierta a cualquiera; el usuario nuevo queda con rol 'invitado'.</summary>
        private void BtnRegistrar_Click(object sender, EventArgs e)
        {
            using var registro = new FrmRegister();
            if (registro.ShowDialog(this) == DialogResult.OK)
            {
                txtUsername.Text = registro.RegisteredUsername;
                txtPassword.Clear();
                txtPassword.Focus();
            }
        }

        private void MostrarError(string mensaje)
        {
            lblError.Text = mensaje;
            txtPassword.Clear();
            txtPassword.Focus();
        }
    }
}
