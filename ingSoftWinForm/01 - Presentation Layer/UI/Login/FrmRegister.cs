using BLL;

namespace UI.Login
{
    /// <summary>
    /// Alta de usuario abierta a cualquiera (T04). El usuario creado recibe siempre
    /// el rol 'invitado'; no se puede elegir otro desde acá.
    /// </summary>
    public partial class FrmRegister : Form
    {
        private readonly UserBLL userBLL;

        /// <summary>Usuario recién creado, para que el login lo precargue.</summary>
        public string RegisteredUsername { get; private set; } = string.Empty;

        public FrmRegister()
        {
            InitializeComponent();
            userBLL = new UserBLL();
        }

        private void BtnRegistrar_Click(object sender, EventArgs e)
        {
            lblError.Text = string.Empty;

            try
            {
                var resultado = userBLL.Register(txtUsername.Text, txtPassword.Text, txtConfirmPassword.Text,
                    txtFirstName.Text, txtLastName.Text, txtEmail.Text);

                if (!resultado.Success)
                {
                    lblError.Text = resultado.Message;
                    return;
                }

                RegisteredUsername = txtUsername.Text.Trim();
                MessageBox.Show(this, resultado.Message, "Registro", MessageBoxButtons.OK, MessageBoxIcon.Information);
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                lblError.Text = "No se pudo conectar con el servidor. Intente nuevamente.";
                System.Diagnostics.Debug.WriteLine(ex);
            }
        }

        private void BtnCancelar_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}
