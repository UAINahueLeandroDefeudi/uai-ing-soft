namespace PatronObserver2
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
            this.components = new System.ComponentModel.Container();
            this.lstMaterias = new System.Windows.Forms.ListBox();
            this.lstAlumnos = new System.Windows.Forms.ListBox();
            this.btnSuscribir = new System.Windows.Forms.Button();
            this.btnDesuscribir = new System.Windows.Forms.Button();
            this.lstNotificaciones = new System.Windows.Forms.ListBox();
            this.tvSuscripciones = new System.Windows.Forms.TreeView();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.materiaBindingSource = new System.Windows.Forms.BindingSource(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.materiaBindingSource)).BeginInit();
            this.SuspendLayout();
            // 
            // lstMaterias
            // 
            this.lstMaterias.FormattingEnabled = true;
            this.lstMaterias.ItemHeight = 16;
            this.lstMaterias.Location = new System.Drawing.Point(29, 74);
            this.lstMaterias.Margin = new System.Windows.Forms.Padding(4);
            this.lstMaterias.Name = "lstMaterias";
            this.lstMaterias.Size = new System.Drawing.Size(291, 260);
            this.lstMaterias.TabIndex = 0;
            this.lstMaterias.SelectedValueChanged += new System.EventHandler(this.lstMaterias_SelectedValueChanged);
            this.lstMaterias.DoubleClick += new System.EventHandler(this.lstMaterias_DoubleClick);
            // 
            // lstAlumnos
            // 
            this.lstAlumnos.FormattingEnabled = true;
            this.lstAlumnos.ItemHeight = 16;
            this.lstAlumnos.Location = new System.Drawing.Point(376, 74);
            this.lstAlumnos.Margin = new System.Windows.Forms.Padding(4);
            this.lstAlumnos.Name = "lstAlumnos";
            this.lstAlumnos.Size = new System.Drawing.Size(291, 260);
            this.lstAlumnos.TabIndex = 1;
            this.lstAlumnos.SelectedValueChanged += new System.EventHandler(this.lstAlumnos_SelectedValueChanged);
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
            this.label1.Size = new System.Drawing.Size(121, 16);
            this.label1.TabIndex = 7;
            this.label1.Text = "listado de materias";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(373, 35);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(108, 16);
            this.label2.TabIndex = 8;
            this.label2.Text = "Lista de alumnos";
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
            this.label4.Size = new System.Drawing.Size(220, 16);
            this.label4.TabIndex = 10;
            this.label4.Text = "Tree alumnos suscriptos a materias";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(51, 54);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(199, 16);
            this.label5.TabIndex = 11;
            this.label5.Text = "(doble click para cambiar el día)";
            // 
            // materiaBindingSource
            // 
            this.materiaBindingSource.DataSource = typeof(PatronObserver2.Materia);
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
            this.Controls.Add(this.lstAlumnos);
            this.Controls.Add(this.lstMaterias);
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "Form1";
            this.Text = "Patron Observer 2";
            ((System.ComponentModel.ISupportInitialize)(this.materiaBindingSource)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListBox lstMaterias;
        private System.Windows.Forms.ListBox lstAlumnos;
        private System.Windows.Forms.Button btnSuscribir;
        private System.Windows.Forms.Button btnDesuscribir;
        private System.Windows.Forms.ListBox lstNotificaciones;
        private System.Windows.Forms.TreeView tvSuscripciones;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.BindingSource materiaBindingSource;
    }
}

