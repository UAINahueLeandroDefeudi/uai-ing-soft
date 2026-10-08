namespace UI.Inicio
{
    partial class FrmInicio
    {
        private System.ComponentModel.IContainer components = null;

        private System.Windows.Forms.Label lblBienvenida;

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
            lblBienvenida = new Label();
            SuspendLayout();
            //
            // lblBienvenida
            //
            lblBienvenida.Dock = DockStyle.Fill;
            lblBienvenida.Font = new Font("Segoe UI", 16F);
            lblBienvenida.Name = "lblBienvenida";
            lblBienvenida.TabIndex = 0;
            lblBienvenida.TextAlign = ContentAlignment.MiddleCenter;
            //
            // FrmInicio
            //
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(500, 300);
            Controls.Add(lblBienvenida);
            Name = "FrmInicio";
            Text = "Inicio";
            ResumeLayout(false);
        }

        #endregion
    }
}
