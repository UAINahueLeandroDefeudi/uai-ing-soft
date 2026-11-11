namespace UI.Usuarios
{
    partial class FrmUsuarios
    {
        private System.ComponentModel.IContainer components = null;

        private System.Windows.Forms.Label lblUsuarios;
        private System.Windows.Forms.DataGridView dgvUsuarios;
        private System.Windows.Forms.GroupBox grpDatos;
        private System.Windows.Forms.Label lblUsername;
        private System.Windows.Forms.TextBox txtUsername;
        private System.Windows.Forms.Label lblNickName;
        private System.Windows.Forms.TextBox txtNickName;
        private System.Windows.Forms.Label lblFirstName;
        private System.Windows.Forms.TextBox txtFirstName;
        private System.Windows.Forms.Label lblLastName;
        private System.Windows.Forms.TextBox txtLastName;
        private System.Windows.Forms.Label lblPais;
        private System.Windows.Forms.TextBox txtPais;
        private System.Windows.Forms.Label lblProvincia;
        private System.Windows.Forms.TextBox txtProvincia;
        private System.Windows.Forms.Label lblDireccion;
        private System.Windows.Forms.TextBox txtDireccion;
        private System.Windows.Forms.Label lblEstado;
        private System.Windows.Forms.Button btnGuardar;
        private System.Windows.Forms.Button btnControlCambios;

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
            lblUsuarios = new Label();
            dgvUsuarios = new DataGridView();
            grpDatos = new GroupBox();
            lblUsername = new Label();
            txtUsername = new TextBox();
            lblNickName = new Label();
            txtNickName = new TextBox();
            lblFirstName = new Label();
            txtFirstName = new TextBox();
            lblLastName = new Label();
            txtLastName = new TextBox();
            lblPais = new Label();
            txtPais = new TextBox();
            lblProvincia = new Label();
            txtProvincia = new TextBox();
            lblDireccion = new Label();
            txtDireccion = new TextBox();
            lblEstado = new Label();
            btnGuardar = new Button();
            btnControlCambios = new Button();
            ((System.ComponentModel.ISupportInitialize)dgvUsuarios).BeginInit();
            grpDatos.SuspendLayout();
            SuspendLayout();
            //
            // lblUsuarios
            //
            lblUsuarios.AutoSize = true;
            lblUsuarios.Location = new Point(12, 12);
            lblUsuarios.Name = "lblUsuarios";
            lblUsuarios.Size = new Size(68, 20);
            lblUsuarios.Text = "Usuarios";
            //
            // dgvUsuarios
            //
            dgvUsuarios.AllowUserToAddRows = false;
            dgvUsuarios.AllowUserToDeleteRows = false;
            dgvUsuarios.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
            dgvUsuarios.AutoGenerateColumns = false;
            dgvUsuarios.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvUsuarios.Location = new Point(12, 36);
            dgvUsuarios.MultiSelect = false;
            dgvUsuarios.Name = "dgvUsuarios";
            dgvUsuarios.ReadOnly = true;
            dgvUsuarios.RowHeadersVisible = false;
            dgvUsuarios.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvUsuarios.Size = new Size(580, 400);
            dgvUsuarios.TabIndex = 0;
            dgvUsuarios.SelectionChanged += DgvUsuarios_SelectionChanged;
            //
            // grpDatos
            //
            grpDatos.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            grpDatos.Controls.Add(lblUsername);
            grpDatos.Controls.Add(txtUsername);
            grpDatos.Controls.Add(lblNickName);
            grpDatos.Controls.Add(txtNickName);
            grpDatos.Controls.Add(lblFirstName);
            grpDatos.Controls.Add(txtFirstName);
            grpDatos.Controls.Add(lblLastName);
            grpDatos.Controls.Add(txtLastName);
            grpDatos.Controls.Add(lblPais);
            grpDatos.Controls.Add(txtPais);
            grpDatos.Controls.Add(lblProvincia);
            grpDatos.Controls.Add(txtProvincia);
            grpDatos.Controls.Add(lblDireccion);
            grpDatos.Controls.Add(txtDireccion);
            grpDatos.Controls.Add(lblEstado);
            grpDatos.Location = new Point(608, 30);
            grpDatos.Name = "grpDatos";
            grpDatos.Size = new Size(400, 340);
            grpDatos.TabIndex = 1;
            grpDatos.TabStop = false;
            grpDatos.Text = "Datos del usuario";
            //
            // lblUsername
            //
            lblUsername.AutoSize = true;
            lblUsername.Location = new Point(12, 30);
            lblUsername.Name = "lblUsername";
            lblUsername.Size = new Size(68, 20);
            lblUsername.Text = "Usuario";
            //
            // txtUsername
            //
            txtUsername.Location = new Point(116, 27);
            txtUsername.Name = "txtUsername";
            txtUsername.ReadOnly = true;
            txtUsername.Size = new Size(268, 27);
            txtUsername.TabStop = false;
            //
            // lblNickName
            //
            lblNickName.AutoSize = true;
            lblNickName.Location = new Point(12, 64);
            lblNickName.Name = "lblNickName";
            lblNickName.Size = new Size(40, 20);
            lblNickName.Text = "Nick";
            //
            // txtNickName
            //
            txtNickName.Location = new Point(116, 61);
            txtNickName.MaxLength = 50;
            txtNickName.Name = "txtNickName";
            txtNickName.Size = new Size(268, 27);
            txtNickName.TabIndex = 0;
            //
            // lblFirstName
            //
            lblFirstName.AutoSize = true;
            lblFirstName.Location = new Point(12, 98);
            lblFirstName.Name = "lblFirstName";
            lblFirstName.Size = new Size(64, 20);
            lblFirstName.Text = "Nombre";
            //
            // txtFirstName
            //
            txtFirstName.Location = new Point(116, 95);
            txtFirstName.MaxLength = 100;
            txtFirstName.Name = "txtFirstName";
            txtFirstName.Size = new Size(268, 27);
            txtFirstName.TabIndex = 1;
            //
            // lblLastName
            //
            lblLastName.AutoSize = true;
            lblLastName.Location = new Point(12, 132);
            lblLastName.Name = "lblLastName";
            lblLastName.Size = new Size(68, 20);
            lblLastName.Text = "Apellido";
            //
            // txtLastName
            //
            txtLastName.Location = new Point(116, 129);
            txtLastName.MaxLength = 100;
            txtLastName.Name = "txtLastName";
            txtLastName.Size = new Size(268, 27);
            txtLastName.TabIndex = 2;
            //
            // lblPais
            //
            lblPais.AutoSize = true;
            lblPais.Location = new Point(12, 166);
            lblPais.Name = "lblPais";
            lblPais.Size = new Size(37, 20);
            lblPais.Text = "País";
            //
            // txtPais
            //
            txtPais.Location = new Point(116, 163);
            txtPais.MaxLength = 100;
            txtPais.Name = "txtPais";
            txtPais.Size = new Size(268, 27);
            txtPais.TabIndex = 3;
            //
            // lblProvincia
            //
            lblProvincia.AutoSize = true;
            lblProvincia.Location = new Point(12, 200);
            lblProvincia.Name = "lblProvincia";
            lblProvincia.Size = new Size(72, 20);
            lblProvincia.Text = "Provincia";
            //
            // txtProvincia
            //
            txtProvincia.Location = new Point(116, 197);
            txtProvincia.MaxLength = 100;
            txtProvincia.Name = "txtProvincia";
            txtProvincia.Size = new Size(268, 27);
            txtProvincia.TabIndex = 4;
            //
            // lblDireccion
            //
            lblDireccion.AutoSize = true;
            lblDireccion.Location = new Point(12, 234);
            lblDireccion.Name = "lblDireccion";
            lblDireccion.Size = new Size(76, 20);
            lblDireccion.Text = "Dirección";
            //
            // txtDireccion
            //
            txtDireccion.Location = new Point(116, 231);
            txtDireccion.MaxLength = 200;
            txtDireccion.Multiline = true;
            txtDireccion.Name = "txtDireccion";
            txtDireccion.ScrollBars = ScrollBars.Vertical;
            txtDireccion.Size = new Size(268, 56);
            txtDireccion.TabIndex = 5;
            //
            // lblEstado
            //
            lblEstado.AutoSize = true;
            lblEstado.Location = new Point(12, 302);
            lblEstado.Name = "lblEstado";
            lblEstado.Size = new Size(15, 20);
            lblEstado.Text = "-";
            //
            // btnGuardar
            //
            btnGuardar.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnGuardar.Location = new Point(608, 380);
            btnGuardar.Name = "btnGuardar";
            btnGuardar.Size = new Size(120, 30);
            btnGuardar.TabIndex = 2;
            btnGuardar.Text = "Guardar";
            btnGuardar.UseVisualStyleBackColor = true;
            btnGuardar.Click += BtnGuardar_Click;
            //
            // btnControlCambios
            //
            btnControlCambios.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnControlCambios.Location = new Point(734, 380);
            btnControlCambios.Name = "btnControlCambios";
            btnControlCambios.Size = new Size(170, 30);
            btnControlCambios.TabIndex = 3;
            btnControlCambios.Text = "Control de cambios";
            btnControlCambios.UseVisualStyleBackColor = true;
            btnControlCambios.Click += BtnControlCambios_Click;
            //
            // FrmUsuarios
            //
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1020, 450);
            Controls.Add(lblUsuarios);
            Controls.Add(dgvUsuarios);
            Controls.Add(grpDatos);
            Controls.Add(btnGuardar);
            Controls.Add(btnControlCambios);
            Name = "FrmUsuarios";
            Text = "Gestión de usuarios";
            ((System.ComponentModel.ISupportInitialize)dgvUsuarios).EndInit();
            grpDatos.ResumeLayout(false);
            grpDatos.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
    }
}
