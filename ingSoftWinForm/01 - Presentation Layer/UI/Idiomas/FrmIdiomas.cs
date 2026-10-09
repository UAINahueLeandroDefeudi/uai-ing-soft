using BE.Entity;
using BLL;

namespace UI.Idiomas
{
    /// <summary>
    /// Administración de idiomas (T05, permiso GESTIONAR_IDIOMAS): alta y modificación de idiomas
    /// y carga de las traducciones de cada etiqueta, con filtro de las que faltan. Un idioma nuevo
    /// nace sin ninguna traducción: hasta completarlas, se muestra el idioma por defecto con marca.
    /// </summary>
    public partial class FrmIdiomas : FrmTraducible
    {
        private readonly BindingSource fuente = new();
        private List<Fila> filas = new();
        private bool cargando;

        public FrmIdiomas()
        {
            InitializeComponent();
            ArmarColumnas();
            dgvTraducciones.DataSource = fuente;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            CargarIdiomas();
        }

        private Idioma? IdiomaSeleccionado => lstIdiomas.SelectedItem as Idioma;

        // ---------- Idiomas ----------

        private void CargarIdiomas(int? seleccionarId = null)
        {
            var idActual = seleccionarId ?? IdiomaSeleccionado?.Id;

            cargando = true;
            try
            {
                var idiomas = Idiomas.GetIdiomas();
                lstIdiomas.DataSource = idiomas;
                lstIdiomas.SelectedItem = idiomas.FirstOrDefault(i => i.Id == idActual) ?? idiomas.FirstOrDefault();
            }
            catch (Exception ex)
            {
                MostrarFalloGenerico(ex);
            }
            finally
            {
                cargando = false;
            }

            MostrarIdiomaSeleccionado();
        }

        private void LstIdiomas_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (!cargando) MostrarIdiomaSeleccionado();
        }

        private void MostrarIdiomaSeleccionado()
        {
            var idioma = IdiomaSeleccionado;

            txtNombre.Text = idioma?.Nombre ?? string.Empty;
            chkActivo.Checked = idioma?.Activo ?? false;
            // El default es el respaldo de todos los demás: tiene que estar siempre activo.
            chkActivo.Enabled = idioma is { EsDefault: false };
            btnGuardarIdioma.Enabled = idioma != null;

            CargarTraducciones();
        }

        private void BtnCrear_Click(object? sender, EventArgs e)
        {
            var resultado = Idiomas.CrearIdioma(txtCodigo.Text, txtNombre.Text);
            Informar(resultado);
            if (!resultado.Success) return;

            var creado = Idiomas.GetIdiomas().FirstOrDefault(i => i.Codigo == txtCodigo.Text.Trim().ToLowerInvariant());
            txtCodigo.Clear();
            CargarIdiomas(creado?.Id);

            // Para que el menú Idioma de la ventana principal vea el idioma nuevo.
            Idiomas.notifySuscribers();
        }

        private void BtnGuardarIdioma_Click(object? sender, EventArgs e)
        {
            if (IdiomaSeleccionado is not { } idioma) return;

            var resultado = Idiomas.ActualizarIdioma(idioma, txtNombre.Text, chkActivo.Checked);
            Informar(resultado);
            if (!resultado.Success)
            {
                CargarIdiomas();
                return;
            }

            CargarIdiomas(idioma.Id);
            Idiomas.notifySuscribers();
        }

        // ---------- Traducciones ----------

        private void ArmarColumnas()
        {
            dgvTraducciones.Columns.Add(Columna("Clave", readOnly: true, ancho: 220));
            dgvTraducciones.Columns.Add(Columna("TextoDefault", readOnly: true, ancho: 280));
            dgvTraducciones.Columns.Add(Columna("Texto", readOnly: false, ancho: 280));
            dgvTraducciones.Columns.Add(Columna("Estado", readOnly: true, ancho: 90));
        }

        private static DataGridViewTextBoxColumn Columna(string propiedad, bool readOnly, int ancho)
            => new()
            {
                Name = propiedad,
                DataPropertyName = propiedad,
                ReadOnly = readOnly,
                Width = ancho,
                SortMode = DataGridViewColumnSortMode.NotSortable
            };

        private void CargarTraducciones()
        {
            if (IdiomaSeleccionado is not { } idioma)
            {
                filas = new();
                AplicarFiltro();
                return;
            }

            try
            {
                filas = Idiomas.GetTraducciones(idioma).Select(i => new Fila(i)).ToList();
            }
            catch (Exception ex)
            {
                filas = new();
                MostrarFalloGenerico(ex);
            }

            AplicarFiltro();
        }

        private void ChkSoloPendientes_CheckedChanged(object? sender, EventArgs e) => AplicarFiltro();

        private void AplicarFiltro()
        {
            fuente.DataSource = (chkSoloPendientes.Checked ? filas.Where(f => !f.Item.Traducido) : filas).ToList();
            MostrarResumen();
        }

        /// <summary>Al terminar de editar la celda se guarda esa traducción (texto vacío = vuelve a pendiente).</summary>
        private void DgvTraducciones_CellEndEdit(object? sender, DataGridViewCellEventArgs e)
        {
            if (IdiomaSeleccionado is not { } idioma) return;
            if (dgvTraducciones.Columns[e.ColumnIndex].DataPropertyName != nameof(Fila.Texto)) return;
            if (dgvTraducciones.Rows[e.RowIndex].DataBoundItem is not Fila fila) return;

            var resultado = Idiomas.GuardarTraduccion(idioma, fila.Item, fila.Texto);
            if (!resultado.Success) Informar(resultado);

            // Sea cual sea el resultado, la grilla se vuelve a mostrar con lo que quedó en la base.
            CargarTraducciones();
        }

        private void MostrarResumen()
            => lblResumen.Text = T("FrmIdiomas.resumen", filas.Count, filas.Count(f => !f.Item.Traducido));

        // ---------- Soporte ----------

        /// <summary>Los encabezados y el resumen los arma el código.</summary>
        protected override void OnIdiomaAplicado()
        {
            foreach (DataGridViewColumn columna in dgvTraducciones.Columns)
            {
                var leyenda = Idiomas.Traducir($"FrmIdiomas.col.{columna.DataPropertyName}");
                if (leyenda.Existe) columna.HeaderText = leyenda.Texto;
            }

            MostrarResumen();
            dgvTraducciones.Invalidate();
        }

        private void Informar(OperationResult resultado)
            => MessageBox.Show(this, T(resultado), T("FrmIdiomas.tituloMensaje"), MessageBoxButtons.OK,
                resultado.Success ? MessageBoxIcon.Information : MessageBoxIcon.Warning);

        private void MostrarFalloGenerico(Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(ex);
            MessageBox.Show(this, T("msg.generic.error"), T("FrmIdiomas.tituloMensaje"),
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        /// <summary>Fila de la grilla: la propiedad Texto es la única editable.</summary>
        private class Fila
        {
            public Fila(TraduccionItem item) => Item = item;

            public TraduccionItem Item { get; }
            public string Clave => Item.Clave;
            public string TextoDefault => Item.TextoDefault;

            public string? Texto
            {
                get => Item.Texto;
                set => Item.Texto = value;
            }

            public string Estado => IdiomaBLL.Instance.Traducir(
                Item.Traducido ? "FrmIdiomas.estadoTraducido" : "FrmIdiomas.estadoPendiente").Texto;
        }
    }
}
