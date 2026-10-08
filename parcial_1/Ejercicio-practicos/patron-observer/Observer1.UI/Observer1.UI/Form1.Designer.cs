namespace Observer1.UI
{
    partial class Form1
    {
        /// <summary>
        /// Variable del diseñador necesaria.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Limpiar los recursos que se estén usando.
        /// </summary>
        /// <param name="disposing">true si los recursos administrados se deben desechar; false en caso contrario.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Código generado por el Diseñador de Windows Forms

        /// <summary>
        /// Método necesario para admitir el Diseñador. No se puede modificar
        /// el contenido de este método con el editor de código.
        /// </summary>
        private void InitializeComponent()
        {
            this.lstProductos = new System.Windows.Forms.ListBox();
            this.lstUsuarios = new System.Windows.Forms.ListBox();
            this.btnSuscribir = new System.Windows.Forms.Button();
            this.btnDesuscribir = new System.Windows.Forms.Button();
            this.lstNotificaciones = new System.Windows.Forms.ListBox();
            this.tvSuscripciones = new System.Windows.Forms.TreeView();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // lstProductos
            // 
            this.lstProductos.FormattingEnabled = true;
            this.lstProductos.ItemHeight = 16;
            this.lstProductos.Location = new System.Drawing.Point(29, 74);
            this.lstProductos.Margin = new System.Windows.Forms.Padding(4);
            this.lstProductos.Name = "lstProductos";
            this.lstProductos.Size = new System.Drawing.Size(291, 260);
            this.lstProductos.TabIndex = 0;
            this.lstProductos.SelectedValueChanged += new System.EventHandler(this.lstProductos_SelectedValueChanged);
            this.lstProductos.DoubleClick += new System.EventHandler(this.lstProductos_DoubleClick);
            // 
            // lstUsuarios
            // 
            this.lstUsuarios.FormattingEnabled = true;
            this.lstUsuarios.ItemHeight = 16;
            this.lstUsuarios.Location = new System.Drawing.Point(376, 74);
            this.lstUsuarios.Margin = new System.Windows.Forms.Padding(4);
            this.lstUsuarios.Name = "lstUsuarios";
            this.lstUsuarios.Size = new System.Drawing.Size(291, 260);
            this.lstUsuarios.TabIndex = 1;
            this.lstUsuarios.SelectedValueChanged += new System.EventHandler(this.lstUsuarios_SelectedValueChanged);
            // 
            // btnSuscribir
            // 
            this.btnSuscribir.Location = new System.Drawing.Point(171, 360);
            this.btnSuscribir.Margin = new System.Windows.Forms.Padding(4);
            this.btnSuscribir.Name = "btnSuscribir";
            this.btnSuscribir.Size = new System.Drawing.Size(172, 55);
            this.btnSuscribir.TabIndex = 3;
            this.btnSuscribir.Text = "Suscribir";
            this.btnSuscribir.UseVisualStyleBackColor = true;
            this.btnSuscribir.Click += new System.EventHandler(this.btnSuscribir_Click);
            // 
            // btnDesuscribir
            // 
            this.btnDesuscribir.Location = new System.Drawing.Point(362, 360);
            this.btnDesuscribir.Margin = new System.Windows.Forms.Padding(4);
            this.btnDesuscribir.Name = "btnDesuscribir";
            this.btnDesuscribir.Size = new System.Drawing.Size(172, 55);
            this.btnDesuscribir.TabIndex = 4;
            this.btnDesuscribir.Text = "Desuscribir";
            this.btnDesuscribir.UseVisualStyleBackColor = true;
            this.btnDesuscribir.Click += new System.EventHandler(this.btnDesuscribir_Click);
            // 
            // lstNotificaciones
            // 
            this.lstNotificaciones.FormattingEnabled = true;
            this.lstNotificaciones.ItemHeight = 16;
            this.lstNotificaciones.Location = new System.Drawing.Point(685, 74);
            this.lstNotificaciones.Margin = new System.Windows.Forms.Padding(4);
            this.lstNotificaciones.Name = "lstNotificaciones";
            this.lstNotificaciones.Size = new System.Drawing.Size(431, 660);
            this.lstNotificaciones.TabIndex = 5;
            // 
            // tvSuscripciones
            // 
            this.tvSuscripciones.Location = new System.Drawing.Point(29, 464);
            this.tvSuscripciones.Margin = new System.Windows.Forms.Padding(4);
            this.tvSuscripciones.Name = "tvSuscripciones";
            this.tvSuscripciones.Size = new System.Drawing.Size(638, 273);
            this.tvSuscripciones.TabIndex = 6;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(29, 35);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(129, 16);
            this.label1.TabIndex = 7;
            this.label1.Text = "listado de productos";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(373, 35);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(108, 16);
            this.label2.TabIndex = 8;
            this.label2.Text = "Lista de usuarios";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(695, 35);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(92, 16);
            this.label3.TabIndex = 9;
            this.label3.Text = "Notificaciones";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(29, 444);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(224, 16);
            this.label4.TabIndex = 10;
            this.label4.Text = "Tree Usuario suscriptos a productos";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(51, 54);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(218, 16);
            this.label5.TabIndex = 11;
            this.label5.Text = "(doble click para cambiar el precio)";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1142, 761);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.tvSuscripciones);
            this.Controls.Add(this.lstNotificaciones);
            this.Controls.Add(this.btnDesuscribir);
            this.Controls.Add(this.btnSuscribir);
            this.Controls.Add(this.lstUsuarios);
            this.Controls.Add(this.lstProductos);
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "Form1";
            this.Text = "Observer ejemplo 1";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListBox lstProductos;
        private System.Windows.Forms.ListBox lstUsuarios;
        private System.Windows.Forms.Button btnSuscribir;
        private System.Windows.Forms.Button btnDesuscribir;
        private System.Windows.Forms.ListBox lstNotificaciones;
        private System.Windows.Forms.TreeView tvSuscripciones;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
    }
}

