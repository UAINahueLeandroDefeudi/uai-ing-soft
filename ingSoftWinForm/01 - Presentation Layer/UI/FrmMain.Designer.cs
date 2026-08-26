namespace UI
{
    partial class FrmMain
    {
        private System.ComponentModel.IContainer components = null;

        private System.Windows.Forms.MenuStrip menuStrip;
        private System.Windows.Forms.ToolStripMenuItem mnuSesion;
        private System.Windows.Forms.ToolStripMenuItem mnuPerfil;
        private System.Windows.Forms.ToolStripMenuItem mnuCerrarSesion;
        private System.Windows.Forms.ToolStripSeparator sepSesion;
        private System.Windows.Forms.ToolStripMenuItem mnuSalir;
        private System.Windows.Forms.ToolStripMenuItem mnuVentana;
        private System.Windows.Forms.ToolStripMenuItem mnuCascada;
        private System.Windows.Forms.ToolStripMenuItem mnuMosaicoHorizontal;
        private System.Windows.Forms.ToolStripMenuItem mnuMosaicoVertical;
        private System.Windows.Forms.ToolStripSeparator sepVentana;
        private System.Windows.Forms.ToolStripMenuItem mnuCerrarTodas;
        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.ToolStripStatusLabel lblUsuario;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.menuStrip = new System.Windows.Forms.MenuStrip();
            this.mnuSesion = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuPerfil = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuCerrarSesion = new System.Windows.Forms.ToolStripMenuItem();
            this.sepSesion = new System.Windows.Forms.ToolStripSeparator();
            this.mnuSalir = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuVentana = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuCascada = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuMosaicoHorizontal = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuMosaicoVertical = new System.Windows.Forms.ToolStripMenuItem();
            this.sepVentana = new System.Windows.Forms.ToolStripSeparator();
            this.mnuCerrarTodas = new System.Windows.Forms.ToolStripMenuItem();
            this.statusStrip = new System.Windows.Forms.StatusStrip();
            this.lblUsuario = new System.Windows.Forms.ToolStripStatusLabel();
            this.menuStrip.SuspendLayout();
            this.statusStrip.SuspendLayout();
            this.SuspendLayout();
            //
            // menuStrip
            //
            this.menuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
                this.mnuSesion,
                this.mnuVentana});
            this.menuStrip.Location = new System.Drawing.Point(0, 0);
            // Las ventanas hijas abiertas se listan solas en el menú Ventana.
            this.menuStrip.MdiWindowListItem = this.mnuVentana;
            this.menuStrip.Name = "menuStrip";
            this.menuStrip.Size = new System.Drawing.Size(984, 24);
            this.menuStrip.TabIndex = 0;
            //
            // mnuSesion
            //
            this.mnuSesion.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
                this.mnuPerfil,
                this.mnuCerrarSesion,
                this.sepSesion,
                this.mnuSalir});
            this.mnuSesion.Name = "mnuSesion";
            this.mnuSesion.Size = new System.Drawing.Size(58, 20);
            this.mnuSesion.Text = "&Sesión";
            //
            // mnuPerfil
            //
            this.mnuPerfil.Name = "mnuPerfil";
            this.mnuPerfil.Size = new System.Drawing.Size(180, 22);
            this.mnuPerfil.Text = "Mi &perfil";
            this.mnuPerfil.Click += new System.EventHandler(this.MnuPerfil_Click);
            //
            // mnuCerrarSesion
            //
            this.mnuCerrarSesion.Name = "mnuCerrarSesion";
            this.mnuCerrarSesion.Size = new System.Drawing.Size(180, 22);
            this.mnuCerrarSesion.Text = "&Cerrar sesión";
            this.mnuCerrarSesion.Click += new System.EventHandler(this.MnuCerrarSesion_Click);
            //
            // sepSesion
            //
            this.sepSesion.Name = "sepSesion";
            this.sepSesion.Size = new System.Drawing.Size(177, 6);
            //
            // mnuSalir
            //
            this.mnuSalir.Name = "mnuSalir";
            this.mnuSalir.Size = new System.Drawing.Size(180, 22);
            this.mnuSalir.Text = "S&alir";
            this.mnuSalir.Click += new System.EventHandler(this.MnuSalir_Click);
            //
            // mnuVentana
            //
            this.mnuVentana.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
                this.mnuCascada,
                this.mnuMosaicoHorizontal,
                this.mnuMosaicoVertical,
                this.sepVentana,
                this.mnuCerrarTodas});
            this.mnuVentana.Name = "mnuVentana";
            this.mnuVentana.Size = new System.Drawing.Size(68, 20);
            this.mnuVentana.Text = "&Ventana";
            //
            // mnuCascada
            //
            this.mnuCascada.Name = "mnuCascada";
            this.mnuCascada.Size = new System.Drawing.Size(190, 22);
            this.mnuCascada.Text = "Cascada";
            this.mnuCascada.Click += new System.EventHandler(this.MnuCascada_Click);
            //
            // mnuMosaicoHorizontal
            //
            this.mnuMosaicoHorizontal.Name = "mnuMosaicoHorizontal";
            this.mnuMosaicoHorizontal.Size = new System.Drawing.Size(190, 22);
            this.mnuMosaicoHorizontal.Text = "Mosaico horizontal";
            this.mnuMosaicoHorizontal.Click += new System.EventHandler(this.MnuMosaicoHorizontal_Click);
            //
            // mnuMosaicoVertical
            //
            this.mnuMosaicoVertical.Name = "mnuMosaicoVertical";
            this.mnuMosaicoVertical.Size = new System.Drawing.Size(190, 22);
            this.mnuMosaicoVertical.Text = "Mosaico vertical";
            this.mnuMosaicoVertical.Click += new System.EventHandler(this.MnuMosaicoVertical_Click);
            //
            // sepVentana
            //
            this.sepVentana.Name = "sepVentana";
            this.sepVentana.Size = new System.Drawing.Size(187, 6);
            //
            // mnuCerrarTodas
            //
            this.mnuCerrarTodas.Name = "mnuCerrarTodas";
            this.mnuCerrarTodas.Size = new System.Drawing.Size(190, 22);
            this.mnuCerrarTodas.Text = "Cerrar todas";
            this.mnuCerrarTodas.Click += new System.EventHandler(this.MnuCerrarTodas_Click);
            //
            // statusStrip
            //
            this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
                this.lblUsuario});
            this.statusStrip.Location = new System.Drawing.Point(0, 539);
            this.statusStrip.Name = "statusStrip";
            this.statusStrip.Size = new System.Drawing.Size(984, 22);
            this.statusStrip.TabIndex = 1;
            //
            // lblUsuario
            //
            this.lblUsuario.Name = "lblUsuario";
            this.lblUsuario.Size = new System.Drawing.Size(0, 17);
            this.lblUsuario.Text = "";
            //
            // FrmMain
            //
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(984, 561);
            this.Controls.Add(this.statusStrip);
            this.Controls.Add(this.menuStrip);
            this.IsMdiContainer = true;
            this.MainMenuStrip = this.menuStrip;
            this.Name = "FrmMain";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Sistema de Gestión";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.Load += new System.EventHandler(this.FrmMain_Load);
            this.menuStrip.ResumeLayout(false);
            this.menuStrip.PerformLayout();
            this.statusStrip.ResumeLayout(false);
            this.statusStrip.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion
    }
}
