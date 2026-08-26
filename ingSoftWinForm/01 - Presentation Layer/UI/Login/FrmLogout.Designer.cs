namespace UI.Login
{
    partial class FrmLogout
    {
        private System.ComponentModel.IContainer components = null;

        private System.Windows.Forms.Label lblPregunta;
        private System.Windows.Forms.Button btnAceptar;
        private System.Windows.Forms.Button btnCancelar;

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
            this.lblPregunta = new System.Windows.Forms.Label();
            this.btnAceptar = new System.Windows.Forms.Button();
            this.btnCancelar = new System.Windows.Forms.Button();
            this.SuspendLayout();
            //
            // lblPregunta
            //
            this.lblPregunta.Location = new System.Drawing.Point(24, 25);
            this.lblPregunta.Name = "lblPregunta";
            this.lblPregunta.Size = new System.Drawing.Size(290, 40);
            this.lblPregunta.TabIndex = 0;
            this.lblPregunta.Text = "¿Desea cerrar la sesión?";
            //
            // btnAceptar
            //
            this.btnAceptar.Location = new System.Drawing.Point(158, 75);
            this.btnAceptar.Name = "btnAceptar";
            this.btnAceptar.Size = new System.Drawing.Size(75, 27);
            this.btnAceptar.TabIndex = 1;
            this.btnAceptar.Text = "Aceptar";
            this.btnAceptar.UseVisualStyleBackColor = true;
            this.btnAceptar.Click += new System.EventHandler(this.BtnAceptar_Click);
            //
            // btnCancelar
            //
            this.btnCancelar.Location = new System.Drawing.Point(239, 75);
            this.btnCancelar.Name = "btnCancelar";
            this.btnCancelar.Size = new System.Drawing.Size(75, 27);
            this.btnCancelar.TabIndex = 2;
            this.btnCancelar.Text = "Cancelar";
            this.btnCancelar.UseVisualStyleBackColor = true;
            this.btnCancelar.Click += new System.EventHandler(this.BtnCancelar_Click);
            //
            // FrmLogout
            //
            this.AcceptButton = this.btnAceptar;
            this.CancelButton = this.btnCancelar;
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(338, 117);
            this.Controls.Add(this.lblPregunta);
            this.Controls.Add(this.btnAceptar);
            this.Controls.Add(this.btnCancelar);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FrmLogout";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Cerrar sesión";
            this.ResumeLayout(false);
        }

        #endregion
    }
}
