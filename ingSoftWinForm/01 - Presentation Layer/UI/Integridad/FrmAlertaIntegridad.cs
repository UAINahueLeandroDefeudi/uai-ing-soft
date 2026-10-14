using BE.Entity;
using BLL;
using Services;
using UI.Idiomas;

namespace UI.Integridad
{
    /// <summary>
    /// T07: diagnóstico de integridad para quien gestiona backups. Muestra tabla, registro, tipo de falla
    /// y fechas de lo que falló, y permite elegir un backup (&lt;NombreBD&gt;_yyyyMMdd_HHmmss.bkp) para
    /// compararlo con la base actual fila por fila y columna por columna antes de restaurarlo.
    /// Construido por código; el Name de cada control es la clave "FrmAlertaIntegridad.&lt;Name&gt;".
    /// </summary>
    public class FrmAlertaIntegridad : FrmTraducible
    {
        private const int MaxValor = 120;

        private readonly BackupBLL backupBLL = new();
        private readonly Label lblAviso = new() { Name = "lblAviso", Dock = DockStyle.Top, Height = 40, Padding = new Padding(8) };
        private readonly ListView lvErrores = new() { Name = "lvErrores", Dock = DockStyle.Top, Height = 150, View = View.Details, FullRowSelect = true, GridLines = true };
        private readonly Label lblDetalle = new() { Name = "lblDetalle", Dock = DockStyle.Top, Height = 36, Padding = new Padding(8, 8, 8, 0) };
        private readonly ListView lvDiferencias = new() { Name = "lvDiferencias", Dock = DockStyle.Fill, View = View.Details, FullRowSelect = true, GridLines = true };
        private readonly Panel pnlBackup = new() { Dock = DockStyle.Bottom, Height = 56, Padding = new Padding(8) };
        private readonly ComboBox cboBackups = new() { Name = "cboBackups", DropDownStyle = ComboBoxStyle.DropDownList, Width = 420, Left = 8, Top = 14, FormattingEnabled = true };
        private readonly Button btnComparar = new() { Name = "btnComparar", Left = 438, Top = 10, Width = 170, Height = 34, UseVisualStyleBackColor = true };
        private readonly Button btnRestaurar = new() { Name = "btnRestaurar", Left = 614, Top = 10, Width = 170, Height = 34, UseVisualStyleBackColor = true };

        public FrmAlertaIntegridad()
        {
            Text = "Integridad de la base de datos";
            Width = 1000;
            Height = 640;
            StartPosition = FormStartPosition.CenterParent;

            lblAviso.Text = "La verificación de integridad detectó diferencias en la base de datos.";
            btnComparar.Text = "Comparar con backup";
            btnRestaurar.Text = "Restaurar backup";
            // Se muestra solo el nombre del archivo (la ruta completa no entra y oculta cuál es el backup).
            cboBackups.Format += (_, e) => e.Value = e.ListItem is string ruta ? Path.GetFileName(ruta) : e.Value;
            btnComparar.Click += BtnComparar_Click;
            btnRestaurar.Click += BtnRestaurar_Click;

            pnlBackup.Controls.AddRange([cboBackups, btnComparar, btnRestaurar]);

            // El último agregado se acopla primero: Fill va antes que los acoplados arriba y abajo.
            Controls.Add(lvDiferencias);
            Controls.Add(lblDetalle);
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
            var resultado = IntegridadManager.Resultado;
            lblAviso.Text = T(IntegridadManager.HayProblemas ? "FrmAlertaIntegridad.aviso" : "FrmAlertaIntegridad.avisoOk");
            lblDetalle.Text = T("FrmAlertaIntegridad.detalleAyuda");

            ArmarColumnas();

            lvErrores.Items.Clear();
            var detectado = resultado?.FechaVerificacion.ToString("G") ?? "-";
            foreach (var error in resultado?.Errores ?? [])
            {
                lvErrores.Items.Add(new ListViewItem([
                    error.Tabla,
                    error.Clave,
                    T($"FrmAlertaIntegridad.tipo.{error.Tipo}"),
                    Fecha(error.FechaRegistro),
                    Fecha(error.UltimoCalculoValido),
                    detectado]));
            }

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

            // ListarBackups ya devuelve vacío sin GESTIONAR_BACKUP.
            btnRestaurar.Enabled = btnComparar.Enabled = cboBackups.Items.Count > 0;
        }

        private void ArmarColumnas()
        {
            lvErrores.Columns.Clear();
            lvErrores.Columns.Add(T("FrmAlertaIntegridad.col.tabla"), 130);
            lvErrores.Columns.Add(T("FrmAlertaIntegridad.col.registro"), 170);
            lvErrores.Columns.Add(T("FrmAlertaIntegridad.col.falla"), 260);
            lvErrores.Columns.Add(T("FrmAlertaIntegridad.col.fechaRegistro"), 120);
            lvErrores.Columns.Add(T("FrmAlertaIntegridad.col.ultimaEscrituraValida"), 150);
            lvErrores.Columns.Add(T("FrmAlertaIntegridad.col.detectado"), 120);

            lvDiferencias.Columns.Clear();
            lvDiferencias.Columns.Add(T("FrmAlertaIntegridad.col.tabla"), 110);
            lvDiferencias.Columns.Add(T("FrmAlertaIntegridad.col.registro"), 120);
            lvDiferencias.Columns.Add(T("FrmAlertaIntegridad.col.diferencia"), 170);
            lvDiferencias.Columns.Add(T("FrmAlertaIntegridad.col.columna"), 100);
            lvDiferencias.Columns.Add(T("FrmAlertaIntegridad.col.valorActual"), 170);
            lvDiferencias.Columns.Add(T("FrmAlertaIntegridad.col.valorBackup"), 170);
            lvDiferencias.Columns.Add(T("FrmAlertaIntegridad.col.fechaRegistro"), 120);
            lvDiferencias.Columns.Add(T("FrmAlertaIntegridad.col.estado"), 160);
        }

        private static string Fecha(DateTime? fecha) => fecha?.ToString("G") ?? "-";

        private static string Recortar(string? valor)
            => valor == null ? "(null)" : valor.Length > MaxValor ? valor[..MaxValor] + "…" : valor;

        /// <summary>Analiza el backup (sin tocar la base) y vuelca el detalle; null si no se pudo.</summary>
        private ResultadoValidacionBackup? Analizar(string archivo)
        {
            UseWaitCursor = true;
            ResultadoValidacionBackup validacion;
            try
            {
                validacion = backupBLL.Validar(archivo);
            }
            finally
            {
                UseWaitCursor = false;
            }

            MostrarDiferencias(validacion);

            if (validacion.ErrorKey == null) return validacion;

            MessageBox.Show(this, T(OperationResult.Fail(validacion.ErrorKey)), T("FrmAlertaIntegridad.Title"),
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return null;
        }

        private void MostrarDiferencias(ResultadoValidacionBackup validacion)
        {
            lvDiferencias.Items.Clear();
            if (validacion.ErrorKey != null) return;

            // Un backup que no pasa la verificación: se muestra qué le falla a él.
            foreach (var error in validacion.Integridad.Errores)
            {
                lvDiferencias.Items.Add(new ListViewItem([
                    error.Tabla, error.Clave, T("FrmAlertaIntegridad.dif.BackupAlterado"),
                    "-", "-", T($"FrmAlertaIntegridad.tipo.{error.Tipo}"), Fecha(error.FechaRegistro), "-"])
                { ForeColor = Color.DarkRed });
            }

            foreach (var dif in validacion.Diferencias)
            {
                var tipo = T($"FrmAlertaIntegridad.dif.{dif.Tipo}");
                var estado = dif.FilaActualAdulterada ? T("FrmAlertaIntegridad.estado.adulterada") : "-";

                if (dif.Cambios.Count == 0)
                {
                    lvDiferencias.Items.Add(Fila(dif, tipo, "-", "-", "-", estado));
                    continue;
                }

                foreach (var cambio in dif.Cambios)
                    lvDiferencias.Items.Add(Fila(dif, tipo, cambio.Columna,
                        Recortar(cambio.ValorActual), Recortar(cambio.ValorBackup), estado));
            }
        }

        private static ListViewItem Fila(DiferenciaBackup dif, string tipo, string columna,
            string actual, string backup, string estado)
        {
            var item = new ListViewItem([dif.Tabla, dif.Clave, tipo, columna, actual, backup, Fecha(dif.FechaRegistro), estado]);
            if (dif.FilaActualAdulterada) item.ForeColor = Color.DarkRed;
            return item;
        }

        private void BtnComparar_Click(object? sender, EventArgs e)
        {
            if (cboBackups.SelectedItem is not string archivo) return;

            var validacion = Analizar(archivo);
            if (validacion == null) return;

            var resumen = !validacion.Integridad.EsValido
                ? T("msg.backup.invalido")
                : validacion.Diferencias.Count == 0
                    ? T("FrmAlertaIntegridad.sinDiferencias")
                    : T("FrmAlertaIntegridad.resumen", validacion.Diferencias.Count, validacion.FechaBackup.ToString("G"));
            lblDetalle.Text = resumen;
        }

        private void BtnRestaurar_Click(object? sender, EventArgs e)
        {
            if (cboBackups.SelectedItem is not string archivo) return;

            // Se analiza el backup antes de tocar la base: integridad y diferencias, con detalle en pantalla.
            var validacion = Analizar(archivo);
            if (validacion == null) return;

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
                    validacion.Perdidas.Where(p => p.Total > 0).Select(p => $"- {p.Tabla}: {p.Total}"));
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

            lvDiferencias.Items.Clear();
            Refrescar();
        }
    }
}
