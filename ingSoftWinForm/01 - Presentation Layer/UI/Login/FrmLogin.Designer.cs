namespace UI.Login
{
    partial class FrmLogin
    {
        private System.ComponentModel.IContainer components = null;

        private System.Windows.Forms.Label lblUsername;
        private System.Windows.Forms.Label lblPassword;
        private System.Windows.Forms.TextBox txtUsername;
        private System.Windows.Forms.TextBox txtPassword;
        private System.Windows.Forms.Button btnAceptar;
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
            this.components = new System.ComponentModel.Container();
            this.lblUsername = new System.Windows.Forms.Label();
            this.lblPassword = new System.Windows.Forms.Label();
            this.txtUsername = new System.Windows.Forms.TextBox();
            this.txtPassword = new System.Windows.Forms.TextBox();
            this.btnAceptar = new System.Windows.Forms.Button();
            this.btnCancelar = new System.Windows.Forms.Button();
            this.lblError = new System.Windows.Forms.Label();
            this.SuspendLayout();
            //
            // lblUsername
            //
            this.lblUsername.AutoSize = true;
            this.lblUsername.Location = new System.Drawing.Point(24, 27);
            this.lblUsername.Name = "lblUsername";
            this.lblUsername.Size = new System.Drawing.Size(56, 15);
            this.lblUsername.TabIndex = 0;
            this.lblUsername.Text = "Usuario";
            //
            // txtUsername
            //
            this.txtUsername.Location = new System.Drawing.Point(110, 24);
            this.txtUsername.MaxLength = 50;
            this.txtUsername.Name = "txtUsername";
            this.txtUsername.Size = new System.Drawing.Size(220, 23);
            this.txtUsername.TabIndex = 1;
            //
            // lblPassword
            //
            this.lblPassword.AutoSize = true;
            this.lblPassword.Location = new System.Drawing.Point(24, 62);
            this.lblPassword.Name = "lblPassword";
            this.lblPassword.Size = new System.Drawing.Size(70, 15);
            this.lblPassword.TabIndex = 2;
            this.lblPassword.Text = "Contraseña";
            //
            // txtPassword
            //
            this.txtPassword.Location = new System.Drawing.Point(110, 59);
            this.txtPassword.MaxLength = 100;
            this.txtPassword.Name = "txtPassword";
            this.txtPassword.Size = new System.Drawing.Size(220, 23);
            this.txtPassword.TabIndex = 3;
            this.txtPassword.UseSystemPasswordChar = true;
            //
            // lblError
            //
            this.lblError.ForeColor = System.Drawing.Color.Firebrick;
            this.lblError.Location = new System.Drawing.Point(24, 95);
            this.lblError.Name = "lblError";
            this.lblError.Size = new System.Drawing.Size(306, 36);
            this.lblError.TabIndex = 4;
            this.lblError.Text = "";
            //
            // btnAceptar
            //
            this.btnAceptar.Location = new System.Drawing.Point(174, 140);
            this.btnAceptar.Name = "btnAceptar";
            this.btnAceptar.Size = new System.Drawing.Size(75, 27);
            this.btnAceptar.TabIndex = 5;
            this.btnAceptar.Text = "Aceptar";
            this.btnAceptar.UseVisualStyleBackColor = true;
            this.btnAceptar.Click += new System.EventHandler(this.BtnAceptar_Click);
            //
            // btnCancelar
            //
            this.btnCancelar.Location = new System.Drawing.Point(255, 140);
            this.btnCancelar.Name = "btnCancelar";
            this.btnCancelar.Size = new System.Drawing.Size(75, 27);
            this.btnCancelar.TabIndex = 6;
            this.btnCancelar.Text = "Cancelar";
            this.btnCancelar.UseVisualStyleBackColor = true;
            this.btnCancelar.Click += new System.EventHandler(this.BtnCancelar_Click);
            //
            // FrmLogin
            //
            this.AcceptButton = this.btnAceptar;
            this.CancelButton = this.btnCancelar;
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(354, 182);
            this.Controls.Add(this.lblUsername);
            this.Controls.Add(this.txtUsername);
            this.Controls.Add(this.lblPassword);
            this.Controls.Add(this.txtPassword);
            this.Controls.Add(this.lblError);
            this.Controls.Add(this.btnAceptar);
            this.Controls.Add(this.btnCancelar);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FrmLogin";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Iniciar sesión";
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion
    }
}
