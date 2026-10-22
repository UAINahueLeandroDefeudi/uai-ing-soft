namespace UI.Productos
{
    partial class FrmProductos
    {
        private System.ComponentModel.IContainer components = null;

        private System.Windows.Forms.Label lblProductos;
        private System.Windows.Forms.CheckBox chkMostrarBajas;
        private System.Windows.Forms.DataGridView dgvProductos;
        private System.Windows.Forms.GroupBox grpDatos;
        private System.Windows.Forms.Label lblCodigo;
        private System.Windows.Forms.TextBox txtCodigo;
        private System.Windows.Forms.Label lblNombre;
        private System.Windows.Forms.TextBox txtNombre;
        private System.Windows.Forms.Label lblDescripcion;
        private System.Windows.Forms.TextBox txtDescripcion;
        private System.Windows.Forms.Label lblPrecio;
        private System.Windows.Forms.NumericUpDown nudPrecio;
        private System.Windows.Forms.Label lblStock;
        private System.Windows.Forms.NumericUpDown nudStock;
        private System.Windows.Forms.Label lblEstado;
        private System.Windows.Forms.Button btnNuevo;
        private System.Windows.Forms.Button btnCrear;
        private System.Windows.Forms.Button btnGuardar;
        private System.Windows.Forms.Button btnBaja;
        private System.Windows.Forms.Label lblHistorial;
        private System.Windows.Forms.DataGridView dgvHistorial;
        private System.Windows.Forms.Button btnRestaurar;

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
            lblProductos = new Label();
            chkMostrarBajas = new CheckBox();
            dgvProductos = new DataGridView();
            grpDatos = new GroupBox();
            lblCodigo = new Label();
            txtCodigo = new TextBox();
            lblNombre = new Label();
            txtNombre = new TextBox();
            lblDescripcion = new Label();
            txtDescripcion = new TextBox();
            lblPrecio = new Label();
            nudPrecio = new NumericUpDown();
            lblStock = new Label();
            nudStock = new NumericUpDown();
            lblEstado = new Label();
            btnNuevo = new Button();
            btnCrear = new Button();
            btnGuardar = new Button();
            btnBaja = new Button();
            lblHistorial = new Label();
            dgvHistorial = new DataGridView();
            btnRestaurar = new Button();
            ((System.ComponentModel.ISupportInitialize)dgvProductos).BeginInit();
            grpDatos.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)nudPrecio).BeginInit();
            ((System.ComponentModel.ISupportInitialize)nudStock).BeginInit();
            ((System.ComponentModel.ISupportInitialize)dgvHistorial).BeginInit();
            SuspendLayout();
            //
            // lblProductos
            //
            lblProductos.AutoSize = true;
            lblProductos.Location = new Point(12, 12);
            lblProductos.Name = "lblProductos";
            lblProductos.Size = new Size(74, 20);
            lblProductos.Text = "Productos";
            //
            // chkMostrarBajas
            //
            chkMostrarBajas.AutoSize = true;
            chkMostrarBajas.Location = new Point(380, 10);
            chkMostrarBajas.Name = "chkMostrarBajas";
            chkMostrarBajas.Size = new Size(186, 24);
            chkMostrarBajas.TabIndex = 0;
            chkMostrarBajas.Text = "Mostrar dados de baja";
            chkMostrarBajas.UseVisualStyleBackColor = true;
            chkMostrarBajas.CheckedChanged += ChkMostrarBajas_CheckedChanged;
            //
            // dgvProductos
            //
            dgvProductos.AllowUserToAddRows = false;
            dgvProductos.AllowUserToDeleteRows = false;
            dgvProductos.AutoGenerateColumns = false;
            dgvProductos.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvProductos.Location = new Point(12, 36);
            dgvProductos.MultiSelect = false;
            dgvProductos.Name = "dgvProductos";
            dgvProductos.ReadOnly = true;
            dgvProductos.RowHeadersVisible = false;
            dgvProductos.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvProductos.Size = new Size(560, 250);
            dgvProductos.TabIndex = 1;
            dgvProductos.SelectionChanged += DgvProductos_SelectionChanged;
            //
            // grpDatos
            //
            grpDatos.Controls.Add(lblCodigo);
            grpDatos.Controls.Add(txtCodigo);
            grpDatos.Controls.Add(lblNombre);
            grpDatos.Controls.Add(txtNombre);
            grpDatos.Controls.Add(lblDescripcion);
            grpDatos.Controls.Add(txtDescripcion);
            grpDatos.Controls.Add(lblPrecio);
            grpDatos.Controls.Add(nudPrecio);
            grpDatos.Controls.Add(lblStock);
            grpDatos.Controls.Add(nudStock);
            grpDatos.Controls.Add(lblEstado);
            grpDatos.Location = new Point(588, 30);
            grpDatos.Name = "grpDatos";
            grpDatos.Size = new Size(400, 256);
            grpDatos.TabIndex = 2;
            grpDatos.TabStop = false;
            grpDatos.Text = "Datos del producto";
            //
            // lblCodigo
            //
            lblCodigo.AutoSize = true;
            lblCodigo.Location = new Point(12, 30);
            lblCodigo.Name = "lblCodigo";
            lblCodigo.Size = new Size(58, 20);
            lblCodigo.Text = "Código";
            //
            // txtCodigo
            //
            txtCodigo.CharacterCasing = CharacterCasing.Upper;
            txtCodigo.Location = new Point(116, 27);
            txtCodigo.MaxLength = 20;
            txtCodigo.Name = "txtCodigo";
            txtCodigo.Size = new Size(268, 27);
            txtCodigo.TabIndex = 0;
            //
            // lblNombre
            //
            lblNombre.AutoSize = true;
            lblNombre.Location = new Point(12, 64);
            lblNombre.Name = "lblNombre";
            lblNombre.Size = new Size(64, 20);
            lblNombre.Text = "Nombre";
            //
            // txtNombre
            //
            txtNombre.Location = new Point(116, 61);
            txtNombre.MaxLength = 100;
            txtNombre.Name = "txtNombre";
            txtNombre.Size = new Size(268, 27);
            txtNombre.TabIndex = 1;
            //
            // lblDescripcion
            //
            lblDescripcion.AutoSize = true;
            lblDescripcion.Location = new Point(12, 98);
            lblDescripcion.Name = "lblDescripcion";
            lblDescripcion.Size = new Size(87, 20);
            lblDescripcion.Text = "Descripción";
            //
            // txtDescripcion
            //
            txtDescripcion.Location = new Point(116, 95);
            txtDescripcion.MaxLength = 500;
            txtDescripcion.Multiline = true;
            txtDescripcion.Name = "txtDescripcion";
            txtDescripcion.ScrollBars = ScrollBars.Vertical;
            txtDescripcion.Size = new Size(268, 56);
            txtDescripcion.TabIndex = 2;
            //
            // lblPrecio
            //
            lblPrecio.AutoSize = true;
            lblPrecio.Location = new Point(12, 162);
            lblPrecio.Name = "lblPrecio";
            lblPrecio.Size = new Size(50, 20);
            lblPrecio.Text = "Precio";
            //
            // nudPrecio
            //
            nudPrecio.DecimalPlaces = 2;
            nudPrecio.Location = new Point(116, 159);
            nudPrecio.Maximum = new decimal(new int[] { -727379969, 232, 0, 0 });
            nudPrecio.Name = "nudPrecio";
            nudPrecio.Size = new Size(150, 27);
            nudPrecio.TabIndex = 3;
            nudPrecio.ThousandsSeparator = true;
            //
            // lblStock
            //
            lblStock.AutoSize = true;
            lblStock.Location = new Point(12, 196);
            lblStock.Name = "lblStock";
            lblStock.Size = new Size(45, 20);
            lblStock.Text = "Stock";
            //
            // nudStock
            //
            nudStock.Location = new Point(116, 193);
            nudStock.Maximum = new decimal(new int[] { int.MaxValue, 0, 0, 0 });
            nudStock.Name = "nudStock";
            nudStock.Size = new Size(150, 27);
            nudStock.TabIndex = 4;
            nudStock.ThousandsSeparator = true;
            //
            // lblEstado
            //
            lblEstado.AutoSize = true;
            lblEstado.Location = new Point(12, 228);
            lblEstado.Name = "lblEstado";
            lblEstado.Size = new Size(15, 20);
            lblEstado.Text = "-";
            //
            // btnNuevo
            //
            btnNuevo.Location = new Point(588, 296);
            btnNuevo.Name = "btnNuevo";
            btnNuevo.Size = new Size(90, 30);
            btnNuevo.TabIndex = 3;
            btnNuevo.Text = "Nuevo";
            btnNuevo.UseVisualStyleBackColor = true;
            btnNuevo.Click += BtnNuevo_Click;
            //
            // btnCrear
            //
            btnCrear.Location = new Point(684, 296);
            btnCrear.Name = "btnCrear";
            btnCrear.Size = new Size(90, 30);
            btnCrear.TabIndex = 4;
            btnCrear.Text = "Crear";
            btnCrear.UseVisualStyleBackColor = true;
            btnCrear.Click += BtnCrear_Click;
            //
            // btnGuardar
            //
            btnGuardar.Location = new Point(780, 296);
            btnGuardar.Name = "btnGuardar";
            btnGuardar.Size = new Size(120, 30);
            btnGuardar.TabIndex = 5;
            btnGuardar.Text = "Guardar cambios";
            btnGuardar.UseVisualStyleBackColor = true;
            btnGuardar.Click += BtnGuardar_Click;
            //
            // btnBaja
            //
            btnBaja.Location = new Point(906, 296);
            btnBaja.Name = "btnBaja";
            btnBaja.Size = new Size(82, 30);
            btnBaja.TabIndex = 6;
            btnBaja.Text = "Dar de baja";
            btnBaja.UseVisualStyleBackColor = true;
            btnBaja.Click += BtnBaja_Click;
            //
            // lblHistorial
            //
            lblHistorial.AutoSize = true;
            lblHistorial.Location = new Point(12, 340);
            lblHistorial.Name = "lblHistorial";
            lblHistorial.Size = new Size(140, 20);
            lblHistorial.Text = "Historial de cambios";
            //
            // dgvHistorial
            //
            dgvHistorial.AllowUserToAddRows = false;
            dgvHistorial.AllowUserToDeleteRows = false;
            dgvHistorial.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dgvHistorial.AutoGenerateColumns = false;
            dgvHistorial.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvHistorial.Location = new Point(12, 364);
            dgvHistorial.MultiSelect = false;
            dgvHistorial.Name = "dgvHistorial";
            dgvHistorial.ReadOnly = true;
            dgvHistorial.RowHeadersVisible = false;
            dgvHistorial.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvHistorial.Size = new Size(976, 260);
            dgvHistorial.TabIndex = 7;
            dgvHistorial.SelectionChanged += DgvHistorial_SelectionChanged;
            //
            // btnRestaurar
            //
            btnRestaurar.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnRestaurar.Location = new Point(12, 634);
            btnRestaurar.Name = "btnRestaurar";
            btnRestaurar.Size = new Size(200, 30);
            btnRestaurar.TabIndex = 8;
            btnRestaurar.Text = "Restaurar esta versión";
            btnRestaurar.UseVisualStyleBackColor = true;
            btnRestaurar.Click += BtnRestaurar_Click;
            //
            // FrmProductos
            //
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1000, 676);
            Controls.Add(lblProductos);
            Controls.Add(chkMostrarBajas);
            Controls.Add(dgvProductos);
            Controls.Add(grpDatos);
            Controls.Add(btnNuevo);
            Controls.Add(btnCrear);
            Controls.Add(btnGuardar);
            Controls.Add(btnBaja);
            Controls.Add(lblHistorial);
            Controls.Add(dgvHistorial);
            Controls.Add(btnRestaurar);
            Name = "FrmProductos";
            Text = "Gestión de productos";
            ((System.ComponentModel.ISupportInitialize)dgvProductos).EndInit();
            grpDatos.ResumeLayout(false);
            grpDatos.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)nudPrecio).EndInit();
            ((System.ComponentModel.ISupportInitialize)nudStock).EndInit();
            ((System.ComponentModel.ISupportInitialize)dgvHistorial).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
    }
}
