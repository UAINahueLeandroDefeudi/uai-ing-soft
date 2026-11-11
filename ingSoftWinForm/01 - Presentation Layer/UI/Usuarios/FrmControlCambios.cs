using System.Globalization;
using BE.Entity;
using BE.Enum;
using BLL;
using UI.Idiomas;

namespace UI.Usuarios
{
    /// <summary>
    /// Control de cambios de usuarios (permiso GESTIONAR_USUARIOS), en tres pasos: 1) elegir el usuario,
    /// 2) ver el historial de versiones (quién, cuándo y qué cambió) y 3) ver los datos de la versión elegida,
    /// con lo que cambió respecto de la anterior en amarillo. Cualquier versión vieja se puede restaurar:
    /// no se reescribe la historia, queda como una versión nueva de tipo Restauración.
    /// </summary>
    public partial class FrmControlCambios : FrmTraducible
    {
        private readonly UserBLL usuarios = new();
        private readonly Guid? usuarioInicial;
        private User? actual;
        private List<FilaHistorial> filasHistorial = new();
        private bool cargando;

        public FrmControlCambios() : this(null) { }

        /// <param name="usuarioInicial">Usuario que queda elegido al abrir (el de la pantalla de usuarios).</param>
        public FrmControlCambios(Guid? usuarioInicial)
        {
            this.usuarioInicial = usuarioInicial;
            InitializeComponent();
            ArmarColumnas();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            CargarUsuarios(usuarioInicial);
        }

        private UserHistory? VersionSeleccionada
            => (dgvHistorial.CurrentRow?.DataBoundItem as FilaHistorial)?.Item;

        // ---------- 1. Usuarios ----------

        private void CargarUsuarios(Guid? seleccionarId = null)
        {
            var idActual = seleccionarId ?? actual?.Id;

            cargando = true;
            try
            {
                var lista = usuarios.GetAll().Select(u => new ItemUsuario(u)).ToList();
                lstUsuarios.DataSource = lista;

                var elegido = lista.FirstOrDefault(i => i.Usuario.Id == idActual) ?? lista.FirstOrDefault();
                lstUsuarios.SelectedItem = elegido;
                actual = elegido?.Usuario;
            }
            catch (Exception ex)
            {
                actual = null;
                lstUsuarios.DataSource = new List<ItemUsuario>();
                MostrarFalloGenerico(ex);
            }
            finally
            {
                cargando = false;
            }

            CargarHistorial();
        }

        private void LstUsuarios_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (cargando) return;

            actual = (lstUsuarios.SelectedItem as ItemUsuario)?.Usuario;
            CargarHistorial();
        }

        // ---------- 2. Historial ----------

        /// <summary>Carga las versiones del usuario elegido y deja elegida la última (la vigente).</summary>
        private void CargarHistorial()
        {
            var filas = new List<FilaHistorial>();
            try
            {
                if (actual != null)
                {
                    var versiones = usuarios.GetHistorial(actual.Id).OrderBy(h => h.Version).ToList();
                    for (var i = 0; i < versiones.Count; i++)
                        filas.Add(new FilaHistorial(versiones[i], i > 0 ? versiones[i - 1] : null));

                    // Se muestra de la más nueva a la más vieja: la versión vigente queda arriba.
                    filas.Reverse();
                }
            }
            catch (Exception ex)
            {
                filas.Clear();
                MostrarFalloGenerico(ex);
            }

            cargando = true;
            try
            {
                filasHistorial = filas;
                dgvHistorial.DataSource = filas;
                Seleccionar(dgvHistorial, 0);
            }
            finally
            {
                cargando = false;
            }

            MostrarComparacion();
        }

        private void DgvHistorial_SelectionChanged(object? sender, EventArgs e)
        {
            if (cargando) return;
            MostrarComparacion();
        }

        // ---------- 3. Datos de la versión elegida ----------

        private void MostrarComparacion()
        {
            var fila = dgvHistorial.CurrentRow?.DataBoundItem as FilaHistorial;

            var cambios = fila == null
                ? new List<CambioCampo>()
                : UserBLL.CompararVersiones(fila.Anterior, fila.Item).ToList();

            dgvComparacion.DataSource = cambios.Select(c => new FilaComparacion(c)).ToList();

            for (var i = 0; i < cambios.Count; i++)
            {
                if (!cambios[i].Cambio) continue;

                var estilo = dgvComparacion.Rows[i].DefaultCellStyle;
                estilo.BackColor = Color.LightYellow;
                estilo.SelectionBackColor = Color.Gold;
                estilo.SelectionForeColor = Color.Black;
            }

            dgvComparacion.ClearSelection();
            ActualizarBotonRestaurar();
        }

        /// <summary>Restaurar la versión vigente no tiene sentido: el botón solo se habilita con una versión vieja.</summary>
        private void ActualizarBotonRestaurar()
            => btnRestaurar.Enabled = actual != null && VersionSeleccionada is { } version && version.Version != actual.Version;

        private void BtnRestaurar_Click(object? sender, EventArgs e)
        {
            if (actual == null || VersionSeleccionada is not { } version) return;
            if (!Confirmar(T("FrmControlCambios.confirmarRestaurar", version.Version))) return;

            var resultado = usuarios.RestaurarVersion(actual, version);
            Informar(resultado);

            // Si otro usuario lo cambió, también se recarga para mostrar la versión vigente.
            if (resultado.Success || resultado.MessageKey == "msg.usuario.desactualizado")
                CargarUsuarios(actual.Id);
        }

        private void BtnVolver_Click(object? sender, EventArgs e) => Close();

        // ---------- Soporte ----------

        private void ArmarColumnas()
        {
            dgvHistorial.Columns.Add(Columna(nameof(FilaHistorial.Version), 70));
            dgvHistorial.Columns.Add(Columna(nameof(FilaHistorial.EventAt), 150));
            dgvHistorial.Columns.Add(Columna(nameof(FilaHistorial.EventBy), 110));
            dgvHistorial.Columns.Add(Columna(nameof(FilaHistorial.TipoCambio), 120));
            dgvHistorial.Columns.Add(Columna(nameof(FilaHistorial.Detalle), 220, DataGridViewAutoSizeColumnMode.Fill));

            dgvComparacion.Columns.Add(Columna(nameof(FilaComparacion.Campo), 120));
            dgvComparacion.Columns.Add(Columna(nameof(FilaComparacion.ValorAnterior), 280, DataGridViewAutoSizeColumnMode.Fill));
            dgvComparacion.Columns.Add(Columna(nameof(FilaComparacion.ValorActual), 280, DataGridViewAutoSizeColumnMode.Fill));
        }

        private static DataGridViewTextBoxColumn Columna(string propiedad, int ancho,
            DataGridViewAutoSizeColumnMode autoSize = DataGridViewAutoSizeColumnMode.None)
            => new()
            {
                Name = propiedad,
                DataPropertyName = propiedad,
                Width = ancho,
                AutoSizeMode = autoSize
            };

        private static void Seleccionar(DataGridView grilla, int fila)
        {
            if (fila < 0 || fila >= grilla.Rows.Count) return;
            grilla.CurrentCell = grilla.Rows[fila].Cells[0];
        }

        /// <summary>Los encabezados y los textos de las filas los arma el código.</summary>
        protected override void OnIdiomaAplicado()
        {
            foreach (var grilla in new[] { dgvHistorial, dgvComparacion })
            {
                foreach (DataGridViewColumn columna in grilla.Columns)
                {
                    var leyenda = Idiomas.Traducir($"FrmControlCambios.col.{columna.DataPropertyName}");
                    if (leyenda.Existe) columna.HeaderText = leyenda.Texto;
                }

                grilla.Invalidate();
            }
        }

        private static string NombreCampo(string campo) => T($"FrmControlCambios.campo.{campo}");

        /// <summary>Texto de la columna Detalle: Alta, "Modificó: Nick, País" o "Restauración de la versión N".</summary>
        private static string Detalle(DetalleVersion detalle) => detalle.Tipo switch
        {
            TipoDetalleVersion.Alta => T("FrmControlCambios.detalle.alta"),
            TipoDetalleVersion.Modificacion => T("FrmControlCambios.detalle.modifico",
                string.Join(", ", detalle.Campos.Select(NombreCampo))),
            TipoDetalleVersion.Restauracion => T("FrmControlCambios.detalle.restauracion", detalle.RestauradaDesde ?? 0),
            _ => string.Empty
        };

        private bool Confirmar(string mensaje)
            => MessageBox.Show(this, mensaje, T("FrmControlCambios.tituloMensaje"), MessageBoxButtons.YesNo,
                MessageBoxIcon.Question) == DialogResult.Yes;

        private void Informar(OperationResult resultado)
            => MessageBox.Show(this, T(resultado), T("FrmControlCambios.tituloMensaje"), MessageBoxButtons.OK,
                resultado.Success ? MessageBoxIcon.Information : MessageBoxIcon.Warning);

        private void MostrarFalloGenerico(Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(ex);
            MessageBox.Show(this, T("msg.generic.error"), T("FrmControlCambios.tituloMensaje"),
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        /// <summary>Elemento de la lista de usuarios: "nick (Nombre Apellido)".</summary>
        private class ItemUsuario
        {
            public ItemUsuario(User usuario) => Usuario = usuario;

            public User Usuario { get; }

            public override string ToString()
                => $"{(string.IsNullOrWhiteSpace(Usuario.NickName) ? Usuario.Username : Usuario.NickName)} " +
                   $"({Usuario.FirstName} {Usuario.LastName})";
        }

        /// <summary>Fila del historial: una versión, con la anterior para calcular el detalle.</summary>
        private class FilaHistorial
        {
            public FilaHistorial(UserHistory item, UserHistory? anterior)
            {
                Item = item;
                Anterior = anterior;
            }

            public UserHistory Item { get; }
            public UserHistory? Anterior { get; }
            public int Version => Item.Version;
            public string EventAt => Item.EventAt.ToString("dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture);
            public string EventBy => Item.EventByUsername;
            public string TipoCambio => T($"FrmControlCambios.tipo.{Item.TipoCambio}");
            public string Detalle => FrmControlCambios.Detalle(UserBLL.DescribirVersion(Anterior, Item));
        }

        /// <summary>Fila de la comparación: un campo con su valor en la versión anterior y en la elegida.</summary>
        private class FilaComparacion
        {
            private readonly CambioCampo cambio;

            public FilaComparacion(CambioCampo cambio) => this.cambio = cambio;

            public string Campo => NombreCampo(cambio.Campo);
            public string ValorAnterior => cambio.ValorAnterior ?? string.Empty;
            public string ValorActual => cambio.ValorActual ?? string.Empty;
        }
    }
}
