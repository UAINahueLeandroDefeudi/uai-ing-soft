namespace UI.Usuarios
{
    partial class FrmControlCambios
    {
        private System.ComponentModel.IContainer components = null;

        private System.Windows.Forms.Label lblTitulo;
        private System.Windows.Forms.Label lblPaso1;
        private System.Windows.Forms.ListBox lstUsuarios;
        private System.Windows.Forms.Label lblPaso2;
        private System.Windows.Forms.DataGridView dgvHistorial;
        private System.Windows.Forms.Label lblPaso3;
        private System.Windows.Forms.DataGridView dgvComparacion;
        private System.Windows.Forms.Button btnRestaurar;
        private System.Windows.Forms.Button btnVolver;

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
            lblTitulo = new Label();
            lblPaso1 = new Label();
            lstUsuarios = new ListBox();
            lblPaso2 = new Label();
            dgvHistorial = new DataGridView();
            lblPaso3 = new Label();
            dgvComparacion = new DataGridView();
            btnRestaurar = new Button();
            btnVolver = new Button();
            ((System.ComponentModel.ISupportInitialize)dgvHistorial).BeginInit();
            ((System.ComponentModel.ISupportInitialize)dgvComparacion).BeginInit();
            SuspendLayout();
            //
            // lblTitulo
            //
            lblTitulo.AutoSize = true;
            lblTitulo.Font = new Font("Segoe UI", 16F, FontStyle.Bold, GraphicsUnit.Point);
            lblTitulo.Location = new Point(12, 9);
            lblTitulo.Name = "lblTitulo";
            lblTitulo.Size = new Size(215, 37);
            lblTitulo.Text = "Control de Cambios";
            //
            // lblPaso1
            //
            lblPaso1.AutoSize = true;
            lblPaso1.Location = new Point(12, 58);
            lblPaso1.Name = "lblPaso1";
            lblPaso1.Size = new Size(126, 20);
            lblPaso1.Text = "1. Elegí el usuario";
            //
            // lstUsuarios
            //
            lstUsuarios.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
            lstUsuarios.FormattingEnabled = true;
            lstUsuarios.ItemHeight = 20;
            lstUsuarios.Location = new Point(12, 82);
            lstUsuarios.Name = "lstUsuarios";
            lstUsuarios.Size = new Size(260, 464);
            lstUsuarios.TabIndex = 0;
            lstUsuarios.SelectedIndexChanged += LstUsuarios_SelectedIndexChanged;
            //
            // lblPaso2
            //
            lblPaso2.AutoSize = true;
            lblPaso2.Location = new Point(288, 58);
            lblPaso2.Name = "lblPaso2";
            lblPaso2.Size = new Size(366, 20);
            lblPaso2.Text = "2. Historial de versiones: quién, cuándo y qué cambió";
            //
            // dgvHistorial
            //
            dgvHistorial.AllowUserToAddRows = false;
            dgvHistorial.AllowUserToDeleteRows = false;
            dgvHistorial.AllowUserToResizeRows = false;
            dgvHistorial.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            dgvHistorial.AutoGenerateColumns = false;
            dgvHistorial.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvHistorial.Location = new Point(288, 82);
            dgvHistorial.MultiSelect = false;
            dgvHistorial.Name = "dgvHistorial";
            dgvHistorial.ReadOnly = true;
            dgvHistorial.RowHeadersVisible = false;
            dgvHistorial.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvHistorial.Size = new Size(700, 220);
            dgvHistorial.TabIndex = 1;
            dgvHistorial.SelectionChanged += DgvHistorial_SelectionChanged;
            //
            // lblPaso3
            //
            lblPaso3.AutoSize = true;
            lblPaso3.Location = new Point(288, 314);
            lblPaso3.Name = "lblPaso3";
            lblPaso3.Size = new Size(640, 20);
            lblPaso3.Text = "3. Datos de la versión elegida (en amarillo, lo que cambió respecto de la versión anterior)";
            //
            // dgvComparacion
            //
            dgvComparacion.AllowUserToAddRows = false;
            dgvComparacion.AllowUserToDeleteRows = false;
            dgvComparacion.AllowUserToResizeRows = false;
            dgvComparacion.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dgvComparacion.AutoGenerateColumns = false;
            dgvComparacion.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvComparacion.Location = new Point(288, 338);
            dgvComparacion.MultiSelect = false;
            dgvComparacion.Name = "dgvComparacion";
            dgvComparacion.ReadOnly = true;
            dgvComparacion.RowHeadersVisible = false;
            dgvComparacion.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvComparacion.Size = new Size(700, 208);
            dgvComparacion.TabIndex = 2;
            //
            // btnRestaurar
            //
            btnRestaurar.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnRestaurar.Location = new Point(288, 560);
            btnRestaurar.Name = "btnRestaurar";
            btnRestaurar.Size = new Size(200, 32);
            btnRestaurar.TabIndex = 3;
            btnRestaurar.Text = "Restaurar esta versión";
            btnRestaurar.UseVisualStyleBackColor = true;
            btnRestaurar.Click += BtnRestaurar_Click;
            //
            // btnVolver
            //
            btnVolver.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnVolver.Location = new Point(888, 560);
            btnVolver.Name = "btnVolver";
            btnVolver.Size = new Size(100, 32);
            btnVolver.TabIndex = 4;
            btnVolver.Text = "Volver";
            btnVolver.UseVisualStyleBackColor = true;
            btnVolver.Click += BtnVolver_Click;
            //
            // FrmControlCambios
            //
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1000, 604);
            Controls.Add(lblTitulo);
            Controls.Add(lblPaso1);
            Controls.Add(lstUsuarios);
            Controls.Add(lblPaso2);
            Controls.Add(dgvHistorial);
            Controls.Add(lblPaso3);
            Controls.Add(dgvComparacion);
            Controls.Add(btnRestaurar);
            Controls.Add(btnVolver);
            MinimumSize = new Size(900, 560);
            Name = "FrmControlCambios";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Control de Cambios";
            ((System.ComponentModel.ISupportInitialize)dgvHistorial).EndInit();
            ((System.ComponentModel.ISupportInitialize)dgvComparacion).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
    }
}
