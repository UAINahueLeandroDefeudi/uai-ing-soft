namespace UI.Login
{
    partial class FrmRegister
    {
        private System.ComponentModel.IContainer components = null;

        private System.Windows.Forms.Label lblUsername;
        private System.Windows.Forms.Label lblPassword;
        private System.Windows.Forms.Label lblConfirmPassword;
        private System.Windows.Forms.Label lblFirstName;
        private System.Windows.Forms.Label lblLastName;
        private System.Windows.Forms.Label lblEmail;
        private System.Windows.Forms.TextBox txtUsername;
        private System.Windows.Forms.TextBox txtPassword;
        private System.Windows.Forms.TextBox txtConfirmPassword;
        private System.Windows.Forms.TextBox txtFirstName;
        private System.Windows.Forms.TextBox txtLastName;
        private System.Windows.Forms.TextBox txtEmail;
        private System.Windows.Forms.Button btnRegistrar;
        private System.Windows.Forms.Button btnCancelar;
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
            lblConfirmPassword = new Label();
            lblFirstName = new Label();
            lblLastName = new Label();
            lblEmail = new Label();
            txtUsername = new TextBox();
            txtPassword = new TextBox();
            txtConfirmPassword = new TextBox();
            txtFirstName = new TextBox();
            txtLastName = new TextBox();
            txtEmail = new TextBox();
            btnRegistrar = new Button();
            btnCancelar = new Button();
            lblError = new Label();
            SuspendLayout();
            //
            // lblUsername
            //
            lblUsername.AutoSize = true;
            lblUsername.Location = new Point(24, 27);
            lblUsername.Name = "lblUsername";
            lblUsername.TabIndex = 0;
            lblUsername.Text = "Usuario";
            //
            // txtUsername
            //
            txtUsername.Location = new Point(160, 24);
            txtUsername.MaxLength = 50;
            txtUsername.Name = "txtUsername";
            txtUsername.Size = new Size(220, 27);
            txtUsername.TabIndex = 1;
            //
            // lblPassword
            //
            lblPassword.AutoSize = true;
            lblPassword.Location = new Point(24, 62);
            lblPassword.Name = "lblPassword";
            lblPassword.TabIndex = 2;
            lblPassword.Text = "Contraseña";
            //
            // txtPassword
            //
            txtPassword.Location = new Point(160, 59);
            txtPassword.MaxLength = 100;
            txtPassword.Name = "txtPassword";
            txtPassword.Size = new Size(220, 27);
            txtPassword.TabIndex = 3;
            txtPassword.UseSystemPasswordChar = true;
            //
            // lblConfirmPassword
            //
            lblConfirmPassword.AutoSize = true;
            lblConfirmPassword.Location = new Point(24, 97);
            lblConfirmPassword.Name = "lblConfirmPassword";
            lblConfirmPassword.TabIndex = 4;
            lblConfirmPassword.Text = "Repetir contraseña";
            //
            // txtConfirmPassword
            //
            txtConfirmPassword.Location = new Point(160, 94);
            txtConfirmPassword.MaxLength = 100;
            txtConfirmPassword.Name = "txtConfirmPassword";
            txtConfirmPassword.Size = new Size(220, 27);
            txtConfirmPassword.TabIndex = 5;
            txtConfirmPassword.UseSystemPasswordChar = true;
            //
            // lblFirstName
            //
            lblFirstName.AutoSize = true;
            lblFirstName.Location = new Point(24, 132);
            lblFirstName.Name = "lblFirstName";
            lblFirstName.TabIndex = 6;
            lblFirstName.Text = "Nombre";
            //
            // txtFirstName
            //
            txtFirstName.Location = new Point(160, 129);
            txtFirstName.MaxLength = 100;
            txtFirstName.Name = "txtFirstName";
            txtFirstName.Size = new Size(220, 27);
            txtFirstName.TabIndex = 7;
            //
            // lblLastName
            //
            lblLastName.AutoSize = true;
            lblLastName.Location = new Point(24, 167);
            lblLastName.Name = "lblLastName";
            lblLastName.TabIndex = 8;
            lblLastName.Text = "Apellido";
            //
            // txtLastName
            //
            txtLastName.Location = new Point(160, 164);
            txtLastName.MaxLength = 100;
            txtLastName.Name = "txtLastName";
            txtLastName.Size = new Size(220, 27);
            txtLastName.TabIndex = 9;
            //
            // lblEmail
            //
            lblEmail.AutoSize = true;
            lblEmail.Location = new Point(24, 202);
            lblEmail.Name = "lblEmail";
            lblEmail.TabIndex = 10;
            lblEmail.Text = "Email (opcional)";
            //
            // txtEmail
            //
            txtEmail.Location = new Point(160, 199);
            txtEmail.MaxLength = 150;
            txtEmail.Name = "txtEmail";
            txtEmail.Size = new Size(220, 27);
            txtEmail.TabIndex = 11;
            //
            // lblError
            //
            lblError.ForeColor = Color.Firebrick;
            lblError.Location = new Point(24, 235);
            lblError.Name = "lblError";
            lblError.Size = new Size(356, 40);
            lblError.TabIndex = 12;
            //
            // btnRegistrar
            //
            btnRegistrar.Location = new Point(224, 285);
            btnRegistrar.Name = "btnRegistrar";
            btnRegistrar.Size = new Size(75, 27);
            btnRegistrar.TabIndex = 13;
            btnRegistrar.Text = "Registrar";
            btnRegistrar.UseVisualStyleBackColor = true;
            btnRegistrar.Click += BtnRegistrar_Click;
            //
            // btnCancelar
            //
            btnCancelar.Location = new Point(305, 285);
            btnCancelar.Name = "btnCancelar";
            btnCancelar.Size = new Size(75, 27);
            btnCancelar.TabIndex = 14;
            btnCancelar.Text = "Cancelar";
            btnCancelar.UseVisualStyleBackColor = true;
            btnCancelar.Click += BtnCancelar_Click;
            //
            // FrmRegister
            //
            AcceptButton = btnRegistrar;
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = btnCancelar;
            ClientSize = new Size(404, 330);
            Controls.Add(lblUsername);
            Controls.Add(txtUsername);
            Controls.Add(lblPassword);
            Controls.Add(txtPassword);
            Controls.Add(lblConfirmPassword);
            Controls.Add(txtConfirmPassword);
            Controls.Add(lblFirstName);
            Controls.Add(txtFirstName);
            Controls.Add(lblLastName);
            Controls.Add(txtLastName);
            Controls.Add(lblEmail);
            Controls.Add(txtEmail);
            Controls.Add(lblError);
            Controls.Add(btnRegistrar);
            Controls.Add(btnCancelar);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "FrmRegister";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Registrarse";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
    }
}
