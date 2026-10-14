using BE.Entity;
using BLL;
using Services;
using UI.Idiomas;

namespace UI.Integridad
{
    /// <summary>
    /// T07: el usuario con REALIZAR_BACKUP fuerza un backup. Es modal: no se puede operar ni cerrar
    /// hasta que la operación termina, y al terminar se informa si fue exitosa o no. Con la integridad
    /// comprometida el botón queda deshabilitado (la BLL también lo rechaza). Construido por código;
    /// el Name de cada control es la clave de traducción "FrmBackup.&lt;Name&gt;".
    /// </summary>
    public class FrmBackup : FrmTraducible
    {
        private readonly BackupBLL backupBLL = new();
        private readonly Label lblAviso = new() { Name = "lblAviso", Dock = DockStyle.Top, Height = 64, Padding = new Padding(12) };
        private readonly Button btnRealizar = new() { Name = "btnRealizar", Left = 12, Top = 80, Width = 160, Height = 32 };
        private readonly Button btnCerrar = new() { Name = "btnCerrar", Left = 180, Top = 80, Width = 110, Height = 32 };
        private readonly Label lblEstado = new() { Name = "lblEstado", Left = 12, Top = 124, Width = 420, Height = 24 };

        private bool trabajando;

        public FrmBackup()
        {
            Text = "Backup de la base de datos";
            Width = 460;
            Height = 210;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;

            btnRealizar.Text = "Realizar backup";
            btnCerrar.Text = "Cerrar";
            btnRealizar.Click += BtnRealizar_Click;
            btnCerrar.Click += (_, _) => Close();
            CancelButton = btnCerrar;

            Controls.AddRange([lblAviso, btnRealizar, btnCerrar, lblEstado]);
        }

        protected override void OnLoad(EventArgs e)
        {
            // El resultado de arranque puede estar viejo: se verifica la base antes de ofrecer el backup.
            try
            {
                new IntegridadBLL().Verificar();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
            }

            base.OnLoad(e);
        }

        protected override void OnIdiomaAplicado() => Refrescar();

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            // Mientras el backup corre no se puede cerrar: se espera a que termine.
            if (trabajando) e.Cancel = true;
            base.OnFormClosing(e);
        }

        private void Refrescar()
        {
            var corrupta = IntegridadManager.HayProblemas;
            lblAviso.Text = T(corrupta ? "FrmBackup.avisoCorrupta" : "FrmBackup.aviso");
            btnRealizar.Enabled = !corrupta && !trabajando;
            btnCerrar.Enabled = !trabajando;
            lblEstado.Text = trabajando ? T("FrmBackup.trabajando") : string.Empty;
        }

        private async void BtnRealizar_Click(object? sender, EventArgs e)
        {
            trabajando = true;
            UseWaitCursor = true;
            Refrescar();

            OperationResult resultado;
            try
            {
                resultado = await Task.Run(() => backupBLL.GenerarBackup());
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
                resultado = OperationResult.Fail("msg.generic.error");
            }

            trabajando = false;
            UseWaitCursor = false;
            Refrescar();

            MessageBox.Show(this, T(resultado), T("FrmBackup.Title"),
                MessageBoxButtons.OK, resultado.Success ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
        }
    }
}
