using BE.Entity;
using BLL;
using UI.Idiomas;

namespace UI.Usuarios
{
    /// <summary>
    /// ABM de usuarios con control de cambios (permiso GESTIONAR_USUARIOS). Edita los seis campos
    /// versionados (NickName, FirstName, LastName, Pais, Provincia, Direccion): cada Guardar deja una
    /// versión nueva en [User_History]. El botón "Control de cambios" abre el historial del usuario elegido.
    /// El Username de login se muestra pero no se edita ni se versiona.
    /// </summary>
    public partial class FrmUsuarios : FrmTraducible
    {
        private readonly UserBLL usuarios = new();
        private User? actual;
        private bool cargando;

        public FrmUsuarios()
        {
            InitializeComponent();
            ArmarColumnas();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            CargarUsuarios();
        }

        // ---------- Usuarios ----------

        private void CargarUsuarios(Guid? seleccionarId = null)
        {
            var idActual = seleccionarId ?? actual?.Id;

            cargando = true;
            try
            {
                var lista = usuarios.GetAll().Select(u => new FilaUsuario(u)).ToList();
                dgvUsuarios.DataSource = lista;

                actual = lista.FirstOrDefault(f => f.Usuario.Id == idActual)?.Usuario ?? lista.FirstOrDefault()?.Usuario;
                Seleccionar(dgvUsuarios, lista.FindIndex(f => f.Usuario == actual));
            }
            catch (Exception ex)
            {
                actual = null;
                dgvUsuarios.DataSource = new List<FilaUsuario>();
                MostrarFalloGenerico(ex);
            }
            finally
            {
                cargando = false;
            }

            MostrarUsuario();
        }

        private void DgvUsuarios_SelectionChanged(object? sender, EventArgs e)
        {
            if (cargando) return;

            actual = (dgvUsuarios.CurrentRow?.DataBoundItem as FilaUsuario)?.Usuario;
            MostrarUsuario();
        }

        private void MostrarUsuario()
        {
            txtUsername.Text = actual?.Username ?? string.Empty;
            txtNickName.Text = actual?.NickName ?? string.Empty;
            txtFirstName.Text = actual?.FirstName ?? string.Empty;
            txtLastName.Text = actual?.LastName ?? string.Empty;
            txtPais.Text = actual?.Pais ?? string.Empty;
            txtProvincia.Text = actual?.Provincia ?? string.Empty;
            txtDireccion.Text = actual?.Direccion ?? string.Empty;

            btnGuardar.Enabled = actual != null;
            btnControlCambios.Enabled = actual != null;

            MostrarEstado();
        }

        private void MostrarEstado()
            => lblEstado.Text = actual == null ? "-" : T("FrmUsuarios.estado", actual.Version);

        private void BtnGuardar_Click(object? sender, EventArgs e)
        {
            if (actual == null) return;

            // Se manda un usuario con los datos de pantalla y la Version con la que se leyó.
            var editado = new User
            {
                Id = actual.Id,
                Username = actual.Username,
                Version = actual.Version,
                NickName = txtNickName.Text,
                FirstName = txtFirstName.Text,
                LastName = txtLastName.Text,
                Pais = txtPais.Text,
                Provincia = txtProvincia.Text,
                Direccion = txtDireccion.Text
            };

            var resultado = usuarios.Modificar(editado);
            Informar(resultado);

            // Si otro usuario lo cambió, se recarga para mostrar la versión vigente.
            if (resultado.Success || resultado.MessageKey == "msg.usuario.desactualizado") CargarUsuarios();
        }

        private void BtnControlCambios_Click(object? sender, EventArgs e)
        {
            if (actual == null) return;

            using (var control = new FrmControlCambios(actual.Id))
                control.ShowDialog(this);

            // Una restauración cambia los datos y la versión del usuario.
            CargarUsuarios();
        }

        // ---------- Soporte ----------

        private void ArmarColumnas()
        {
            dgvUsuarios.Columns.Add(Columna(nameof(FilaUsuario.Username), 100));
            dgvUsuarios.Columns.Add(Columna(nameof(FilaUsuario.NickName), 90));
            dgvUsuarios.Columns.Add(Columna(nameof(FilaUsuario.FirstName), 100));
            dgvUsuarios.Columns.Add(Columna(nameof(FilaUsuario.LastName), 100));
            dgvUsuarios.Columns.Add(Columna(nameof(FilaUsuario.Pais), 90));
            dgvUsuarios.Columns.Add(Columna(nameof(FilaUsuario.Provincia), 100));
            dgvUsuarios.Columns.Add(Columna(nameof(FilaUsuario.Direccion), 140));
            dgvUsuarios.Columns.Add(Columna(nameof(FilaUsuario.Version), 60));
        }

        private static DataGridViewTextBoxColumn Columna(string propiedad, int ancho)
            => new()
            {
                Name = propiedad,
                DataPropertyName = propiedad,
                Width = ancho
            };

        private static void Seleccionar(DataGridView grilla, int fila)
        {
            if (fila < 0 || fila >= grilla.Rows.Count) return;
            grilla.CurrentCell = grilla.Rows[fila].Cells[0];
        }

        /// <summary>Los encabezados y el estado los arma el código.</summary>
        protected override void OnIdiomaAplicado()
        {
            foreach (DataGridViewColumn columna in dgvUsuarios.Columns)
            {
                var leyenda = Idiomas.Traducir($"FrmUsuarios.col.{columna.DataPropertyName}");
                if (leyenda.Existe) columna.HeaderText = leyenda.Texto;
            }

            dgvUsuarios.Invalidate();
            MostrarEstado();
        }

        private void Informar(OperationResult resultado)
            => MessageBox.Show(this, T(resultado), T("FrmUsuarios.tituloMensaje"), MessageBoxButtons.OK,
                resultado.Success ? MessageBoxIcon.Information : MessageBoxIcon.Warning);

        private void MostrarFalloGenerico(Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(ex);
            MessageBox.Show(this, T("msg.generic.error"), T("FrmUsuarios.tituloMensaje"),
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        /// <summary>Fila de la grilla de usuarios.</summary>
        private class FilaUsuario
        {
            public FilaUsuario(User usuario) => Usuario = usuario;

            public User Usuario { get; }
            public string Username => Usuario.Username;
            public string? NickName => Usuario.NickName;
            public string FirstName => Usuario.FirstName;
            public string LastName => Usuario.LastName;
            public string? Pais => Usuario.Pais;
            public string? Provincia => Usuario.Provincia;
            public string? Direccion => Usuario.Direccion;
            public int Version => Usuario.Version;
        }
    }
}
