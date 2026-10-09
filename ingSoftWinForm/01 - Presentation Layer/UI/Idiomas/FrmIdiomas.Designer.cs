namespace UI.Idiomas
{
    partial class FrmIdiomas
    {
        private System.ComponentModel.IContainer components = null;

        private System.Windows.Forms.Label lblIdiomas;
        private System.Windows.Forms.ListBox lstIdiomas;
        private System.Windows.Forms.Label lblCodigo;
        private System.Windows.Forms.TextBox txtCodigo;
        private System.Windows.Forms.Label lblNombre;
        private System.Windows.Forms.TextBox txtNombre;
        private System.Windows.Forms.Button btnCrear;
        private System.Windows.Forms.CheckBox chkActivo;
        private System.Windows.Forms.Button btnGuardarIdioma;
        private System.Windows.Forms.Label lblTraducciones;
        private System.Windows.Forms.CheckBox chkSoloPendientes;
        private System.Windows.Forms.DataGridView dgvTraducciones;
        private System.Windows.Forms.Label lblResumen;

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
            lblIdiomas = new Label();
            lstIdiomas = new ListBox();
            lblCodigo = new Label();
            txtCodigo = new TextBox();
            lblNombre = new Label();
            txtNombre = new TextBox();
            btnCrear = new Button();
            chkActivo = new CheckBox();
            btnGuardarIdioma = new Button();
            lblTraducciones = new Label();
            chkSoloPendientes = new CheckBox();
            dgvTraducciones = new DataGridView();
            lblResumen = new Label();
            ((System.ComponentModel.ISupportInitialize)dgvTraducciones).BeginInit();
            SuspendLayout();
            //
            // lblIdiomas
            //
            lblIdiomas.AutoSize = true;
            lblIdiomas.Location = new Point(12, 12);
            lblIdiomas.Name = "lblIdiomas";
            lblIdiomas.Size = new Size(58, 20);
            lblIdiomas.Text = "Idiomas";
            //
            // lstIdiomas
            //
            lstIdiomas.FormattingEnabled = true;
            lstIdiomas.ItemHeight = 20;
            lstIdiomas.Location = new Point(12, 36);
            lstIdiomas.Name = "lstIdiomas";
            lstIdiomas.Size = new Size(220, 164);
            lstIdiomas.TabIndex = 0;
            lstIdiomas.SelectedIndexChanged += LstIdiomas_SelectedIndexChanged;
            //
            // lblCodigo
            //
            lblCodigo.AutoSize = true;
            lblCodigo.Location = new Point(256, 12);
            lblCodigo.Name = "lblCodigo";
            lblCodigo.Size = new Size(120, 20);
            lblCodigo.Text = "Código (p. ej. pt)";
            //
            // txtCodigo
            //
            txtCodigo.CharacterCasing = CharacterCasing.Lower;
            txtCodigo.Location = new Point(256, 36);
            txtCodigo.MaxLength = 10;
            txtCodigo.Name = "txtCodigo";
            txtCodigo.Size = new Size(120, 27);
            txtCodigo.TabIndex = 1;
            //
            // lblNombre
            //
            lblNombre.AutoSize = true;
            lblNombre.Location = new Point(396, 12);
            lblNombre.Name = "lblNombre";
            lblNombre.Size = new Size(64, 20);
            lblNombre.Text = "Nombre";
            //
            // txtNombre
            //
            txtNombre.Location = new Point(396, 36);
            txtNombre.MaxLength = 50;
            txtNombre.Name = "txtNombre";
            txtNombre.Size = new Size(220, 27);
            txtNombre.TabIndex = 2;
            //
            // btnCrear
            //
            btnCrear.Location = new Point(636, 34);
            btnCrear.Name = "btnCrear";
            btnCrear.Size = new Size(140, 30);
            btnCrear.TabIndex = 3;
            btnCrear.Text = "Crear idioma";
            btnCrear.UseVisualStyleBackColor = true;
            btnCrear.Click += BtnCrear_Click;
            //
            // chkActivo
            //
            chkActivo.AutoSize = true;
            chkActivo.Location = new Point(256, 84);
            chkActivo.Name = "chkActivo";
            chkActivo.Size = new Size(72, 24);
            chkActivo.TabIndex = 4;
            chkActivo.Text = "Activo";
            chkActivo.UseVisualStyleBackColor = true;
            //
            // btnGuardarIdioma
            //
            btnGuardarIdioma.Location = new Point(396, 80);
            btnGuardarIdioma.Name = "btnGuardarIdioma";
            btnGuardarIdioma.Size = new Size(220, 30);
            btnGuardarIdioma.TabIndex = 5;
            btnGuardarIdioma.Text = "Guardar idioma";
            btnGuardarIdioma.UseVisualStyleBackColor = true;
            btnGuardarIdioma.Click += BtnGuardarIdioma_Click;
            //
            // lblTraducciones
            //
            lblTraducciones.AutoSize = true;
            lblTraducciones.Location = new Point(12, 216);
            lblTraducciones.Name = "lblTraducciones";
            lblTraducciones.Size = new Size(200, 20);
            lblTraducciones.Text = "Traducciones del idioma seleccionado";
            //
            // chkSoloPendientes
            //
            chkSoloPendientes.AutoSize = true;
            chkSoloPendientes.Location = new Point(700, 214);
            chkSoloPendientes.Name = "chkSoloPendientes";
            chkSoloPendientes.Size = new Size(150, 24);
            chkSoloPendientes.TabIndex = 6;
            chkSoloPendientes.Text = "Solo sin traducir";
            chkSoloPendientes.UseVisualStyleBackColor = true;
            chkSoloPendientes.CheckedChanged += ChkSoloPendientes_CheckedChanged;
            //
            // dgvTraducciones
            //
            dgvTraducciones.AllowUserToAddRows = false;
            dgvTraducciones.AllowUserToDeleteRows = false;
            dgvTraducciones.AutoGenerateColumns = false;
            dgvTraducciones.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvTraducciones.Location = new Point(12, 244);
            dgvTraducciones.Name = "dgvTraducciones";
            dgvTraducciones.RowHeadersWidth = 51;
            dgvTraducciones.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvTraducciones.Size = new Size(860, 280);
            dgvTraducciones.TabIndex = 7;
            dgvTraducciones.CellEndEdit += DgvTraducciones_CellEndEdit;
            //
            // lblResumen
            //
            lblResumen.AutoSize = true;
            lblResumen.Location = new Point(12, 532);
            lblResumen.Name = "lblResumen";
            lblResumen.Size = new Size(15, 20);
            lblResumen.Text = "-";
            //
            // FrmIdiomas
            //
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(884, 566);
            Controls.Add(lblIdiomas);
            Controls.Add(lstIdiomas);
            Controls.Add(lblCodigo);
            Controls.Add(txtCodigo);
            Controls.Add(lblNombre);
            Controls.Add(txtNombre);
            Controls.Add(btnCrear);
            Controls.Add(chkActivo);
            Controls.Add(btnGuardarIdioma);
            Controls.Add(lblTraducciones);
            Controls.Add(chkSoloPendientes);
            Controls.Add(dgvTraducciones);
            Controls.Add(lblResumen);
            Name = "FrmIdiomas";
            Text = "Gestión de idiomas";
            ((System.ComponentModel.ISupportInitialize)dgvTraducciones).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
    }
}
