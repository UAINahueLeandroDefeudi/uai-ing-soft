using BE.Enum;
using BLL;

// El namespace UI.Event tapa al tipo BE.Entity.Bitacora tanto como lo hacía UI.Bitacora,
// así que el alias sigue haciendo falta para nombrar la entidad.
using BitacoraEntity = BE.Entity.Bitacora;

namespace UI.Event
{
    /// <summary>
    /// Visor de sólo lectura de la bitácora de auditoría, con filtros por rango de
    /// fechas, tipo, evento y prioridad.
    /// </summary>
    public partial class FrmEvent : Form
    {
        /// <summary>Primer ítem de los tres combos: no filtrar por esa columna.</summary>
        private const string Todos = "(todos)";

        private const int DiasPorDefecto = 7;

        private readonly BitacoraBLL bitacoraBLL;

        public FrmEvent()
        {
            InitializeComponent();
            bitacoraBLL = new BitacoraBLL();

            CargarCombo<BitacoraType>(cboTipo);
            CargarCombo<NameEvent>(cboEvento);
            CargarCombo<Priority>(cboPrioridad);
            ResetearFiltros();

            Cargar(bitacoraBLL.GetAll);
        }

        private void BtnFiltrar_Click(object sender, EventArgs e)
            => Cargar(() => bitacoraBLL.GetByFilter(
                dtpDesde.Value,
                dtpHasta.Value,
                Seleccion<BitacoraType>(cboTipo),
                Seleccion<NameEvent>(cboEvento),
                Seleccion<Priority>(cboPrioridad)));

        private void BtnLimpiar_Click(object sender, EventArgs e)
        {
            ResetearFiltros();
            Cargar(bitacoraBLL.GetAll);
        }

        private void ResetearFiltros()
        {
            dtpDesde.Value = DateTime.Today.AddDays(-DiasPorDefecto);
            dtpHasta.Value = DateTime.Today;

            cboTipo.SelectedIndex = 0;
            cboEvento.SelectedIndex = 0;
            cboPrioridad.SelectedIndex = 0;
        }

        /// <summary>
        /// Carga el combo con "(todos)" y después los valores del enum. Se agregan los
        /// valores en crudo y no su texto: así <see cref="Seleccion{TEnum}"/> los
        /// recupera tipados, sin volver a parsear.
        /// </summary>
        private static void CargarCombo<TEnum>(ComboBox combo) where TEnum : struct, System.Enum
        {
            combo.Items.Add(Todos);

            foreach (var valor in System.Enum.GetValues<TEnum>())
                combo.Items.Add(valor);

            combo.SelectedIndex = 0;
        }

        /// <summary>null cuando está elegido "(todos)", o sea sin filtrar por esa columna.</summary>
        private static TEnum? Seleccion<TEnum>(ComboBox combo) where TEnum : struct, System.Enum
            => combo.SelectedItem is TEnum valor ? valor : null;

        /// <summary>
        /// A diferencia de BitacoraBLL.Registrar, que se traga los errores para no
        /// voltear la operación auditada, acá el fallo sí se avisa: si no se puede
        /// leer, la pantalla no tiene nada que mostrar.
        /// </summary>
        private void Cargar(Func<List<BitacoraEntity>> consulta)
        {
            try
            {
                var registros = consulta();

                dgvEventos.DataSource = registros.Select(Proyectar).ToList();
                lblTotal.Text = $"{registros.Count} registro(s)";
            }
            catch (Exception ex)
            {
                dgvEventos.DataSource = null;
                lblTotal.Text = "-";

                MessageBox.Show(this, "No se pudo leer la bitácora.", "Bitácora",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                System.Diagnostics.Debug.WriteLine(ex);
            }
        }

        private static Fila Proyectar(BitacoraEntity bitacora) => new Fila
        {
            Id = bitacora.id_bitacora,
            Fecha = bitacora.BitacoraDate.ToString("dd/MM/yyyy HH:mm:ss"),
            Tipo = bitacora.Type.ToString(),
            Evento = bitacora.NameEvent.ToString(),
            Prioridad = bitacora.Priority.ToString(),
            Usuario = $"{bitacora.FirstName} {bitacora.LastName}".Trim(),
            Email = bitacora.Email,
            Detalle = bitacora.Detail
        };

        /// <summary>
        /// Lo que ve el grid: los nombres de propiedad son los encabezados de columna.
        /// Deja afuera IdUser y RolesPermisos, que no aportan a la lectura rápida.
        /// </summary>
        private class Fila
        {
            public int Id { get; set; }
            public string Fecha { get; set; } = string.Empty;
            public string Tipo { get; set; } = string.Empty;
            public string Evento { get; set; } = string.Empty;
            public string Prioridad { get; set; } = string.Empty;
            public string Usuario { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
            public string Detalle { get; set; } = string.Empty;
        }
    }
}
