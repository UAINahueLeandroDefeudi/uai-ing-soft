namespace UI.Login
{
    partial class FrmLogin
    {
        private System.ComponentModel.IContainer components = null;

        private System.Windows.Forms.Label lblUsername;
        private System.Windows.Forms.Label lblPassword;
        private System.Windows.Forms.TextBox txtUsername;
        private System.Windows.Forms.TextBox txtPassword;
        private System.Windows.Forms.Label lblIdioma;
        private System.Windows.Forms.ComboBox cboIdioma;
        private System.Windows.Forms.Button btnAceptar;
        private System.Windows.Forms.Button btnCancelar;
        private System.Windows.Forms.Button btnRegistrar;
        private System.Windows.Forms.Label lblError;

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
            lblUsername = new Label();
            lblPassword = new Label();
            txtUsername = new TextBox();
            txtPassword = new TextBox();
            lblIdioma = new Label();
            cboIdioma = new ComboBox();
            btnAceptar = new Button();
            btnCancelar = new Button();
            btnRegistrar = new Button();
            lblError = new Label();
            SuspendLayout();
            //
            // lblUsername
            //
            lblUsername.AutoSize = true;
            lblUsername.Location = new Point(24, 27);
            lblUsername.Name = "lblUsername";
            lblUsername.Size = new Size(59, 20);
            lblUsername.TabIndex = 0;
            lblUsername.Text = "Usuario";
            //
            // lblPassword
            //
            lblPassword.AutoSize = true;
            lblPassword.Location = new Point(24, 62);
            lblPassword.Name = "lblPassword";
            lblPassword.Size = new Size(83, 20);
            lblPassword.TabIndex = 2;
            lblPassword.Text = "Contraseña";
            //
            // txtUsername
            //
            txtUsername.Location = new Point(110, 24);
            txtUsername.MaxLength = 50;
            txtUsername.Name = "txtUsername";
            txtUsername.Size = new Size(220, 27);
            txtUsername.TabIndex = 1;
            //
            // txtPassword
            //
            txtPassword.Location = new Point(110, 59);
            txtPassword.MaxLength = 100;
            txtPassword.Name = "txtPassword";
            txtPassword.Size = new Size(220, 27);
            txtPassword.TabIndex = 3;
            txtPassword.UseSystemPasswordChar = true;
            //
            // lblIdioma
            //
            lblIdioma.AutoSize = true;
            lblIdioma.Location = new Point(24, 97);
            lblIdioma.Name = "lblIdioma";
            lblIdioma.Size = new Size(60, 20);
            lblIdioma.TabIndex = 8;
            lblIdioma.Text = "Idioma";
            //
            // cboIdioma
            //
            cboIdioma.DropDownStyle = ComboBoxStyle.DropDownList;
            cboIdioma.Location = new Point(110, 94);
            cboIdioma.Name = "cboIdioma";
            cboIdioma.Size = new Size(220, 28);
            cboIdioma.TabIndex = 9;
            //
            // btnAceptar
            //
            btnAceptar.Location = new Point(174, 175);
            btnAceptar.Name = "btnAceptar";
            btnAceptar.Size = new Size(75, 27);
            btnAceptar.TabIndex = 5;
            btnAceptar.Text = "Aceptar";
            btnAceptar.UseVisualStyleBackColor = true;
            btnAceptar.Click += BtnAceptar_Click;
            //
            // btnCancelar
            //
            btnCancelar.Location = new Point(255, 175);
            btnCancelar.Name = "btnCancelar";
            btnCancelar.Size = new Size(75, 27);
            btnCancelar.TabIndex = 6;
            btnCancelar.Text = "Cancelar";
            btnCancelar.UseVisualStyleBackColor = true;
            btnCancelar.Click += BtnCancelar_Click;
            //
            // btnRegistrar
            //
            btnRegistrar.Location = new Point(24, 175);
            btnRegistrar.Name = "btnRegistrar";
            btnRegistrar.Size = new Size(110, 27);
            btnRegistrar.TabIndex = 7;
            btnRegistrar.Text = "Registrarse";
            btnRegistrar.UseVisualStyleBackColor = true;
            btnRegistrar.Click += BtnRegistrar_Click;
            //
            // lblError
            //
            lblError.ForeColor = Color.Firebrick;
            lblError.Location = new Point(24, 130);
            lblError.Name = "lblError";
            lblError.Size = new Size(306, 36);
            lblError.TabIndex = 4;
            //
            // FrmLogin
            //
            AcceptButton = btnAceptar;
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = btnCancelar;
            ClientSize = new Size(354, 217);
            Controls.Add(lblUsername);
            Controls.Add(txtUsername);
            Controls.Add(lblPassword);
            Controls.Add(txtPassword);
            Controls.Add(lblIdioma);
            Controls.Add(cboIdioma);
            Controls.Add(lblError);
            Controls.Add(btnAceptar);
            Controls.Add(btnCancelar);
            Controls.Add(btnRegistrar);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "FrmLogin";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Iniciar sesión";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
    }
}
