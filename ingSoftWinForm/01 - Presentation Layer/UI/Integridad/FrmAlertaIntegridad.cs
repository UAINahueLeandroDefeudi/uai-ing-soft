using BE.Entity;
using BLL;
using Services;
using UI.Idiomas;

namespace UI.Integridad
{
    /// <summary>
    /// T07: alerta para el administrador cuando la verificación de integridad falló al arrancar.
    /// Muestra tabla, registro y tipo de falla, y permite restaurar manualmente un backup
    /// (&lt;NombreBD&gt;_yyyyMMdd_HHmmss.bkp). Construido por código; el Name de cada control
    /// es la clave de traducción "FrmAlertaIntegridad.&lt;Name&gt;".
    /// </summary>
    public class FrmAlertaIntegridad : FrmTraducible
    {
        private readonly BackupBLL backupBLL = new();
        private readonly Label lblAviso = new() { Name = "lblAviso", Dock = DockStyle.Top, Height = 48, Padding = new Padding(8) };
        private readonly ListView lvErrores = new() { Name = "lvErrores", Dock = DockStyle.Fill, View = View.Details, FullRowSelect = true };
        private readonly Panel pnlBackup = new() { Dock = DockStyle.Bottom, Height = 44, Padding = new Padding(8) };
        private readonly ComboBox cboBackups = new() { Name = "cboBackups", DropDownStyle = ComboBoxStyle.DropDownList, Width = 380, Left = 8, Top = 10 };
        private readonly Button btnRestaurar = new() { Name = "btnRestaurar", Left = 400, Top = 8, Width = 130 };
        private readonly Button btnBackup = new() { Name = "btnBackup", Left = 536, Top = 8, Width = 130 };

        public FrmAlertaIntegridad()
        {
            Text = "Integridad de la base de datos";
            Width = 720;
            Height = 420;
            StartPosition = FormStartPosition.CenterParent;

            lvErrores.Columns.Add("Tabla", 160);
            lvErrores.Columns.Add("Registro", 220);
            lvErrores.Columns.Add("Falla", 260);

            lblAviso.Text = "La verificación de integridad detectó diferencias en la base de datos.";
            btnRestaurar.Text = "Restaurar backup";
            btnBackup.Text = "Generar backup";
            btnRestaurar.Click += BtnRestaurar_Click;
            btnBackup.Click += BtnBackup_Click;

            pnlBackup.Controls.AddRange([cboBackups, btnRestaurar, btnBackup]);
            Controls.Add(lvErrores);
            Controls.Add(pnlBackup);
            Controls.Add(lblAviso);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            Refrescar();
        }

        protected override void OnIdiomaAplicado() => Refrescar();

        private void Refrescar()
        {
            lblAviso.Text = T("FrmAlertaIntegridad.aviso");

            lvErrores.Items.Clear();
            var errores = IntegridadManager.Resultado?.Errores ?? [];
            foreach (var error in errores)
            {
                var falla = T($"FrmAlertaIntegridad.tipo.{error.Tipo}");
                lvErrores.Items.Add(new ListViewItem([error.Tabla, error.Clave, falla]));
            }

            // Solo con la base verificada tiene sentido generar un backup nuevo.
            btnBackup.Enabled = !IntegridadManager.HayProblemas;

            try
            {
                cboBackups.DataSource = backupBLL.ListarBackups();
                cboBackups.DisplayMember = string.Empty;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
                cboBackups.DataSource = null;
            }

            btnRestaurar.Enabled = cboBackups.Items.Count > 0;
        }

        private void BtnRestaurar_Click(object? sender, EventArgs e)
        {
            if (cboBackups.SelectedItem is not string archivo) return;

            // Se analiza el backup antes de tocar la base: integridad y cambios que se perderían.
            UseWaitCursor = true;
            var validacion = backupBLL.Validar(archivo);
            UseWaitCursor = false;

            if (validacion.ErrorKey != null)
            {
                MessageBox.Show(this, T(OperationResult.Fail(validacion.ErrorKey)), T("FrmAlertaIntegridad.Title"),
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!validacion.Integridad.EsValido)
            {
                MessageBox.Show(this, T(OperationResult.Fail("msg.backup.invalido")), T("FrmAlertaIntegridad.Title"),
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var aceptaPerdida = false;
            if (validacion.HayPerdida)
            {
                var detalle = string.Join(Environment.NewLine,
                    validacion.Perdidas.Select(p => $"- {p.Tabla}: {p.Total}"));
                var ultimo = validacion.UltimoCambioActual?.ToString("g") ?? "-";

                var sigue = MessageBox.Show(this,
                    T("FrmAlertaIntegridad.perdida", validacion.PerdidaDesde.ToString("g"), validacion.TotalPerdido, ultimo)
                        + Environment.NewLine + Environment.NewLine + detalle,
                    T("FrmAlertaIntegridad.Title"), MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);
                if (sigue != DialogResult.Yes) return;

                aceptaPerdida = true;
            }
            else
            {
                var confirma = MessageBox.Show(this,
                    T("FrmAlertaIntegridad.confirmar", Path.GetFileName(archivo)),
                    T("FrmAlertaIntegridad.Title"), MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (confirma != DialogResult.Yes) return;
            }

            UseWaitCursor = true;
            var resultado = backupBLL.Restaurar(archivo, aceptaPerdida);
            UseWaitCursor = false;

            MessageBox.Show(this, T(resultado), T("FrmAlertaIntegridad.Title"),
                MessageBoxButtons.OK, resultado.Success ? MessageBoxIcon.Information : MessageBoxIcon.Warning);

            Refrescar();
        }

        private void BtnBackup_Click(object? sender, EventArgs e)
        {
            var resultado = backupBLL.GenerarBackup();
            MessageBox.Show(this, T(resultado), T("FrmAlertaIntegridad.Title"),
                MessageBoxButtons.OK, resultado.Success ? MessageBoxIcon.Information : MessageBoxIcon.Warning);

            Refrescar();
        }
    }
}
