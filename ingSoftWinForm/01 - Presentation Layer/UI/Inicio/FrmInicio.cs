using BLL;
using UI.Idiomas;

namespace UI.Inicio
{
    /// <summary>
    /// Página de inicio mínima. Existe para que el permiso VER_INICIO
    /// (roles client, moderador y administrador) controle algo concreto.
    /// </summary>
    public partial class FrmInicio : FrmTraducible
    {
        public FrmInicio()
        {
            InitializeComponent();
        }

        /// <summary>El saludo lo arma el código: se vuelve a armar cada vez que cambia el idioma.</summary>
        protected override void OnIdiomaAplicado()
        {
            var user = new SessionBLL().CurrentUser;
            lblBienvenida.Text = user == null
                ? T("FrmInicio.bienvenidaGenerica")
                : T("FrmInicio.bienvenida", user.FirstName, user.LastName);
        }
    }
}
