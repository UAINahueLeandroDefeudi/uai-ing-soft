using BE.Entity;
using BE.Observer;
using BLL;

namespace UI.Idiomas
{
    /// <summary>
    /// Formulario base traducible (T05): es el Subscriber del Observer. Al abrirse se suscribe
    /// a <see cref="IdiomaBLL"/>, aplica el idioma activo y vuelve a aplicarlo cada vez que
    /// el publisher notifica un cambio; al cerrarse se desuscribe.
    /// Qué texto lleva cada control se resuelve por convención de claves, sin tocar los
    /// Designer: "&lt;NombreDelFormulario&gt;.&lt;NombreDelControl&gt;" y "&lt;NombreDelFormulario&gt;.Title".
    /// Los controles cuya clave no existe en la base conservan el texto con el que fueron
    /// diseñados. Los textos que arma el código usan <see cref="T(string, object[])"/>.
    /// </summary>
    public class FrmTraducible : Form, ISuscriberIdioma
    {
        private readonly ToolTip toolTip = new();
        private bool suscripto;

        protected static IdiomaBLL Idiomas => IdiomaBLL.Instance;

        protected override void OnLoad(EventArgs e)
        {
            if (!DesignMode)
            {
                Idiomas.suscribe(this);
                suscripto = true;
                AplicarIdioma();
            }

            base.OnLoad(e);
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            if (suscripto)
            {
                Idiomas.unsuscribe(this);
                suscripto = false;
            }

            base.OnFormClosed(e);
        }

        /// <summary>Método Actualizar del Observer: el publisher avisa que cambió el idioma.</summary>
        public void Actualizar(Idioma idiomaActivo)
        {
            if (!IsDisposed) AplicarIdioma();
        }

        /// <summary>Texto de una etiqueta en el idioma activo (para mensajes y textos armados por código).</summary>
        protected static string T(string clave, params object[] args) => Idiomas.Traducir(clave, args).Texto;

        /// <summary>Texto del mensaje de un OperationResult de la BLL.</summary>
        protected static string T(OperationResult resultado) => Idiomas.Traducir(resultado).Texto;

        /// <summary>Se ejecuta tras aplicar el idioma: los formularios refrescan acá sus textos dinámicos.</summary>
        protected virtual void OnIdiomaAplicado() { }

        private void AplicarIdioma()
        {
            var nombre = GetType().Name;
            toolTip.RemoveAll();

            AplicarTitulo(nombre);
            Recorrer(Controls, nombre);
            OnIdiomaAplicado();
        }

        private void AplicarTitulo(string nombre)
        {
            var leyenda = Idiomas.Traducir($"{nombre}.Title");
            if (leyenda.Existe) Text = leyenda.Texto;
        }

        /// <summary>Recorre recursivamente el árbol de controles (paneles, tabs, grupos, menús y barras).</summary>
        private void Recorrer(Control.ControlCollection controles, string nombre)
        {
            foreach (Control control in controles)
            {
                var leyenda = Idiomas.Traducir($"{nombre}.{control.Name}");
                if (leyenda.Existe)
                {
                    control.Text = leyenda.Texto;
                    if (!leyenda.Traducido)
                        toolTip.SetToolTip(control, Idiomas.Traducir("msg.sinTraducir").Texto);
                }

                if (control is ToolStrip strip)
                    Recorrer(strip.Items, nombre);

                if (control.HasChildren)
                    Recorrer(control.Controls, nombre);
            }
        }

        private void Recorrer(ToolStripItemCollection items, string nombre)
        {
            foreach (ToolStripItem item in items)
            {
                var leyenda = Idiomas.Traducir($"{nombre}.{item.Name}");
                if (leyenda.Existe)
                {
                    item.Text = leyenda.Texto;
                    item.ToolTipText = leyenda.Traducido ? string.Empty : Idiomas.Traducir("msg.sinTraducir").Texto;
                }

                if (item is ToolStripDropDownItem desplegable)
                    Recorrer(desplegable.DropDownItems, nombre);
            }
        }
    }
}
