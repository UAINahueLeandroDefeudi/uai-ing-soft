using BE.Entity;
using BLL;
using UI.Idiomas;

namespace UI.Productos
{
    /// <summary>
    /// ABM de productos con control de cambios (T06b, permiso GESTIONAR_PRODUCTOS). Abajo se ve
    /// el historial del producto elegido: cada fila es la copia completa del producto en esa versión,
    /// con quién y cuándo la generó. Cualquier versión vieja se puede restaurar (queda como versión nueva).
    /// </summary>
    public partial class FrmProductos : FrmTraducible
    {
        private readonly ProductoBLL productos = new();
        private Producto? actual;
        private bool cargando;

        public FrmProductos()
        {
            InitializeComponent();
            ArmarColumnas();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            CargarProductos();
        }

        private ProductoHistorial? VersionSeleccionada
            => (dgvHistorial.CurrentRow?.DataBoundItem as FilaHistorial)?.Item;

        // ---------- Productos ----------

        private void CargarProductos(int? seleccionarId = null)
        {
            var idActual = seleccionarId ?? actual?.Id;

            cargando = true;
            try
            {
                var lista = productos.GetAll()
                    .Where(p => chkMostrarBajas.Checked || p.Activo || p.Id == idActual)
                    .Select(p => new FilaProducto(p))
                    .ToList();
                dgvProductos.DataSource = lista;

                actual = lista.FirstOrDefault(f => f.Producto.Id == idActual)?.Producto ?? lista.FirstOrDefault()?.Producto;
                Seleccionar(dgvProductos, lista.FindIndex(f => f.Producto == actual));
            }
            catch (Exception ex)
            {
                actual = null;
                dgvProductos.DataSource = new List<FilaProducto>();
                MostrarFalloGenerico(ex);
            }
            finally
            {
                cargando = false;
            }

            MostrarProducto();
        }

        private void ChkMostrarBajas_CheckedChanged(object? sender, EventArgs e) => CargarProductos();

        private void DgvProductos_SelectionChanged(object? sender, EventArgs e)
        {
            if (cargando) return;

            actual = (dgvProductos.CurrentRow?.DataBoundItem as FilaProducto)?.Producto;
            MostrarProducto();
        }

        private void MostrarProducto()
        {
            txtCodigo.Text = actual?.Codigo ?? string.Empty;
            txtNombre.Text = actual?.Nombre ?? string.Empty;
            txtDescripcion.Text = actual?.Descripcion ?? string.Empty;
            nudPrecio.Value = Math.Min(actual?.Precio ?? 0, nudPrecio.Maximum);
            nudStock.Value = actual?.Stock ?? 0;

            btnGuardar.Enabled = actual != null;
            btnBaja.Enabled = actual is { Activo: true };

            MostrarEstado();
            CargarHistorial();
        }

        private void MostrarEstado()
            => lblEstado.Text = actual == null
                ? T("FrmProductos.estadoNuevo")
                : T("FrmProductos.estado", EstadoTexto(actual.Activo), actual.Version, actual.FechaModificacion.ToString("g"));

        private void BtnNuevo_Click(object? sender, EventArgs e)
        {
            cargando = true;
            dgvProductos.ClearSelection();
            dgvProductos.CurrentCell = null;
            cargando = false;

            actual = null;
            MostrarProducto();
            txtCodigo.Focus();
        }

        private void BtnCrear_Click(object? sender, EventArgs e)
        {
            var resultado = productos.Crear(txtCodigo.Text, txtNombre.Text, txtDescripcion.Text,
                nudPrecio.Value, (int)nudStock.Value);
            Informar(resultado);
            if (!resultado.Success) return;

            var codigo = txtCodigo.Text.Trim().ToUpperInvariant();
            var creado = productos.GetAll().FirstOrDefault(p => p.Codigo == codigo);
            CargarProductos(creado?.Id);
        }

        private void BtnGuardar_Click(object? sender, EventArgs e)
        {
            if (actual == null) return;

            var resultado = productos.Modificar(actual, txtCodigo.Text, txtNombre.Text, txtDescripcion.Text,
                nudPrecio.Value, (int)nudStock.Value);
            Informar(resultado);

            // Si otro usuario lo cambió, se recarga para mostrar la versión vigente.
            if (resultado.Success || resultado.MessageKey == "msg.producto.desactualizado") CargarProductos();
        }

        private void BtnBaja_Click(object? sender, EventArgs e)
        {
            if (actual == null) return;
            if (!Confirmar(T("FrmProductos.confirmarBaja", actual.Codigo))) return;

            Informar(productos.DarDeBaja(actual));
            CargarProductos();
        }

        // ---------- Historial ----------

        private void CargarHistorial()
        {
            List<FilaHistorial> filas;
            try
            {
                filas = actual == null
                    ? new()
                    : productos.GetHistorial(actual).Select(h => new FilaHistorial(h)).ToList();
            }
            catch (Exception ex)
            {
                filas = new();
                MostrarFalloGenerico(ex);
            }

            dgvHistorial.DataSource = filas;
            ActualizarBotonRestaurar();
        }

        private void DgvHistorial_SelectionChanged(object? sender, EventArgs e) => ActualizarBotonRestaurar();

        /// <summary>Restaurar la versión vigente no tiene sentido: el botón solo se habilita con una versión vieja.</summary>
        private void ActualizarBotonRestaurar()
            => btnRestaurar.Enabled = actual != null && VersionSeleccionada is { } version && version.Version != actual.Version;

        private void BtnRestaurar_Click(object? sender, EventArgs e)
        {
            if (actual == null || VersionSeleccionada is not { } version) return;
            if (!Confirmar(T("FrmProductos.confirmarRestaurar", actual.Codigo, version.Version))) return;

            Informar(productos.RestaurarVersion(actual, version));
            CargarProductos();
        }

        // ---------- Soporte ----------

        private void ArmarColumnas()
        {
            dgvProductos.Columns.Add(Columna(nameof(FilaProducto.Codigo), 90));
            dgvProductos.Columns.Add(Columna(nameof(FilaProducto.Nombre), 160));
            dgvProductos.Columns.Add(Columna(nameof(FilaProducto.Precio), 90, "N2"));
            dgvProductos.Columns.Add(Columna(nameof(FilaProducto.Stock), 60));
            dgvProductos.Columns.Add(Columna(nameof(FilaProducto.Estado), 90));
            dgvProductos.Columns.Add(Columna(nameof(FilaProducto.Version), 60));

            dgvHistorial.Columns.Add(Columna(nameof(FilaHistorial.Version), 60));
            dgvHistorial.Columns.Add(Columna(nameof(FilaHistorial.Operacion), 110));
            dgvHistorial.Columns.Add(Columna(nameof(FilaHistorial.EventAt), 140, "g"));
            dgvHistorial.Columns.Add(Columna(nameof(FilaHistorial.EventBy), 100));
            dgvHistorial.Columns.Add(Columna(nameof(FilaHistorial.Codigo), 90));
            dgvHistorial.Columns.Add(Columna(nameof(FilaHistorial.Nombre), 150));
            dgvHistorial.Columns.Add(Columna(nameof(FilaHistorial.Descripcion), 160));
            dgvHistorial.Columns.Add(Columna(nameof(FilaHistorial.Precio), 80, "N2"));
            dgvHistorial.Columns.Add(Columna(nameof(FilaHistorial.Stock), 60));
            dgvHistorial.Columns.Add(Columna(nameof(FilaHistorial.Estado), 90));
        }

        private static DataGridViewTextBoxColumn Columna(string propiedad, int ancho, string? formato = null)
            => new()
            {
                Name = propiedad,
                DataPropertyName = propiedad,
                Width = ancho,
                DefaultCellStyle = { Format = formato ?? string.Empty }
            };

        private static void Seleccionar(DataGridView grilla, int fila)
        {
            if (fila < 0 || fila >= grilla.Rows.Count) return;
            grilla.CurrentCell = grilla.Rows[fila].Cells[0];
        }

        /// <summary>Los encabezados, el estado y las operaciones los arma el código.</summary>
        protected override void OnIdiomaAplicado()
        {
            foreach (var grilla in new[] { dgvProductos, dgvHistorial })
            {
                foreach (DataGridViewColumn columna in grilla.Columns)
                {
                    var leyenda = Idiomas.Traducir($"FrmProductos.col.{columna.DataPropertyName}");
                    if (leyenda.Existe) columna.HeaderText = leyenda.Texto;
                }

                grilla.Invalidate();
            }

            MostrarEstado();
        }

        private static string EstadoTexto(bool activo) => T(activo ? "FrmProductos.estadoActivo" : "FrmProductos.estadoBaja");

        private bool Confirmar(string mensaje)
            => MessageBox.Show(this, mensaje, T("FrmProductos.tituloMensaje"), MessageBoxButtons.YesNo,
                MessageBoxIcon.Question) == DialogResult.Yes;

        private void Informar(OperationResult resultado)
            => MessageBox.Show(this, T(resultado), T("FrmProductos.tituloMensaje"), MessageBoxButtons.OK,
                resultado.Success ? MessageBoxIcon.Information : MessageBoxIcon.Warning);

        private void MostrarFalloGenerico(Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(ex);
            MessageBox.Show(this, T("msg.generic.error"), T("FrmProductos.tituloMensaje"),
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        /// <summary>Fila de la grilla de productos.</summary>
        private class FilaProducto
        {
            public FilaProducto(Producto producto) => Producto = producto;

            public Producto Producto { get; }
            public string Codigo => Producto.Codigo;
            public string Nombre => Producto.Nombre;
            public decimal Precio => Producto.Precio;
            public int Stock => Producto.Stock;
            public string Estado => EstadoTexto(Producto.Activo);
            public int Version => Producto.Version;
        }

        /// <summary>Fila del historial: la copia completa del producto en esa versión.</summary>
        private class FilaHistorial
        {
            public FilaHistorial(ProductoHistorial item) => Item = item;

            public ProductoHistorial Item { get; }
            public int Version => Item.Version;
            public string Operacion => T($"FrmProductos.op.{Item.Operacion}");
            public DateTime EventAt => Item.EventAt;
            public string EventBy => Item.EventByUsername;
            public string Codigo => Item.Codigo;
            public string Nombre => Item.Nombre;
            public string? Descripcion => Item.Descripcion;
            public decimal Precio => Item.Precio;
            public int Stock => Item.Stock;
            public string Estado => EstadoTexto(Item.Activo);
        }
    }
}
