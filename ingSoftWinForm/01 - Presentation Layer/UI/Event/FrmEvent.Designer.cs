namespace UI.Event
{
    partial class FrmEvent
    {
        private System.ComponentModel.IContainer components = null;

        private System.Windows.Forms.Label lblDesdeCaption;
        private System.Windows.Forms.Label lblHastaCaption;
        private System.Windows.Forms.DateTimePicker dtpDesde;
        private System.Windows.Forms.DateTimePicker dtpHasta;

        private System.Windows.Forms.Label lblTipoCaption;
        private System.Windows.Forms.Label lblEventoCaption;
        private System.Windows.Forms.Label lblPrioridadCaption;
        private System.Windows.Forms.ComboBox cboTipo;
        private System.Windows.Forms.ComboBox cboEvento;
        private System.Windows.Forms.ComboBox cboPrioridad;

        private System.Windows.Forms.Button btnFiltrar;
        private System.Windows.Forms.Button btnLimpiar;

        private System.Windows.Forms.DataGridView dgvEventos;
        private System.Windows.Forms.Label lblTotal;

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
            this.lblDesdeCaption = new System.Windows.Forms.Label();
            this.lblHastaCaption = new System.Windows.Forms.Label();
            this.dtpDesde = new System.Windows.Forms.DateTimePicker();
            this.dtpHasta = new System.Windows.Forms.DateTimePicker();
            this.lblTipoCaption = new System.Windows.Forms.Label();
            this.lblEventoCaption = new System.Windows.Forms.Label();
            this.lblPrioridadCaption = new System.Windows.Forms.Label();
            this.cboTipo = new System.Windows.Forms.ComboBox();
            this.cboEvento = new System.Windows.Forms.ComboBox();
            this.cboPrioridad = new System.Windows.Forms.ComboBox();
            this.btnFiltrar = new System.Windows.Forms.Button();
            this.btnLimpiar = new System.Windows.Forms.Button();
            this.dgvEventos = new System.Windows.Forms.DataGridView();
            this.lblTotal = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.dgvEventos)).BeginInit();
            this.SuspendLayout();
            //
            // lblDesdeCaption
            //
            this.lblDesdeCaption.AutoSize = true;
            this.lblDesdeCaption.Location = new System.Drawing.Point(16, 20);
            this.lblDesdeCaption.Name = "lblDesdeCaption";
            this.lblDesdeCaption.Size = new System.Drawing.Size(44, 15);
            this.lblDesdeCaption.TabIndex = 0;
            this.lblDesdeCaption.Text = "Desde";
            //
            // dtpDesde
            //
            this.dtpDesde.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.dtpDesde.Location = new System.Drawing.Point(88, 16);
            this.dtpDesde.Name = "dtpDesde";
            this.dtpDesde.Size = new System.Drawing.Size(120, 23);
            this.dtpDesde.TabIndex = 1;
            //
            // lblHastaCaption
            //
            this.lblHastaCaption.AutoSize = true;
            this.lblHastaCaption.Location = new System.Drawing.Point(228, 20);
            this.lblHastaCaption.Name = "lblHastaCaption";
            this.lblHastaCaption.Size = new System.Drawing.Size(38, 15);
            this.lblHastaCaption.TabIndex = 2;
            this.lblHastaCaption.Text = "Hasta";
            //
            // dtpHasta
            //
            this.dtpHasta.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.dtpHasta.Location = new System.Drawing.Point(300, 16);
            this.dtpHasta.Name = "dtpHasta";
            this.dtpHasta.Size = new System.Drawing.Size(120, 23);
            this.dtpHasta.TabIndex = 3;
            //
            // lblTipoCaption
            //
            this.lblTipoCaption.AutoSize = true;
            this.lblTipoCaption.Location = new System.Drawing.Point(16, 56);
            this.lblTipoCaption.Name = "lblTipoCaption";
            this.lblTipoCaption.Size = new System.Drawing.Size(31, 15);
            this.lblTipoCaption.TabIndex = 4;
            this.lblTipoCaption.Text = "Tipo";
            //
            // cboTipo
            //
            this.cboTipo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboTipo.Location = new System.Drawing.Point(88, 52);
            this.cboTipo.Name = "cboTipo";
            this.cboTipo.Size = new System.Drawing.Size(120, 23);
            this.cboTipo.TabIndex = 5;
            //
            // lblEventoCaption
            //
            this.lblEventoCaption.AutoSize = true;
            this.lblEventoCaption.Location = new System.Drawing.Point(228, 56);
            this.lblEventoCaption.Name = "lblEventoCaption";
            this.lblEventoCaption.Size = new System.Drawing.Size(45, 15);
            this.lblEventoCaption.TabIndex = 6;
            this.lblEventoCaption.Text = "Evento";
            //
            // cboEvento
            //
            this.cboEvento.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboEvento.Location = new System.Drawing.Point(300, 52);
            this.cboEvento.Name = "cboEvento";
            this.cboEvento.Size = new System.Drawing.Size(160, 23);
            this.cboEvento.TabIndex = 7;
            //
            // lblPrioridadCaption
            //
            this.lblPrioridadCaption.AutoSize = true;
            this.lblPrioridadCaption.Location = new System.Drawing.Point(480, 56);
            this.lblPrioridadCaption.Name = "lblPrioridadCaption";
            this.lblPrioridadCaption.Size = new System.Drawing.Size(60, 15);
            this.lblPrioridadCaption.TabIndex = 8;
            this.lblPrioridadCaption.Text = "Prioridad";
            //
            // cboPrioridad
            //
            this.cboPrioridad.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboPrioridad.Location = new System.Drawing.Point(552, 52);
            this.cboPrioridad.Name = "cboPrioridad";
            this.cboPrioridad.Size = new System.Drawing.Size(120, 23);
            this.cboPrioridad.TabIndex = 9;
            //
            // btnFiltrar
            //
            this.btnFiltrar.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            this.btnFiltrar.Location = new System.Drawing.Point(710, 15);
            this.btnFiltrar.Name = "btnFiltrar";
            this.btnFiltrar.Size = new System.Drawing.Size(158, 25);
            this.btnFiltrar.TabIndex = 10;
            this.btnFiltrar.Text = "Filtrar";
            this.btnFiltrar.UseVisualStyleBackColor = true;
            this.btnFiltrar.Click += new System.EventHandler(this.BtnFiltrar_Click);
            //
            // btnLimpiar
            //
            this.btnLimpiar.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            this.btnLimpiar.Location = new System.Drawing.Point(710, 51);
            this.btnLimpiar.Name = "btnLimpiar";
            this.btnLimpiar.Size = new System.Drawing.Size(158, 25);
            this.btnLimpiar.TabIndex = 11;
            this.btnLimpiar.Text = "Limpiar filtros";
            this.btnLimpiar.UseVisualStyleBackColor = true;
            this.btnLimpiar.Click += new System.EventHandler(this.BtnLimpiar_Click);
            //
            // dgvEventos
            //
            this.dgvEventos.AllowUserToAddRows = false;
            this.dgvEventos.AllowUserToDeleteRows = false;
            this.dgvEventos.Anchor = System.Windows.Forms.AnchorStyles.Top
                | System.Windows.Forms.AnchorStyles.Bottom
                | System.Windows.Forms.AnchorStyles.Left
                | System.Windows.Forms.AnchorStyles.Right;
            this.dgvEventos.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvEventos.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvEventos.Location = new System.Drawing.Point(16, 88);
            this.dgvEventos.MultiSelect = false;
            this.dgvEventos.Name = "dgvEventos";
            this.dgvEventos.ReadOnly = true;
            this.dgvEventos.RowHeadersVisible = false;
            this.dgvEventos.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvEventos.Size = new System.Drawing.Size(852, 348);
            this.dgvEventos.TabIndex = 12;
            //
            // lblTotal
            //
            this.lblTotal.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            this.lblTotal.AutoSize = true;
            this.lblTotal.Location = new System.Drawing.Point(16, 446);
            this.lblTotal.Name = "lblTotal";
            this.lblTotal.Size = new System.Drawing.Size(12, 15);
            this.lblTotal.TabIndex = 13;
            this.lblTotal.Text = "-";
            //
            // FrmEvent
            //
            this.AcceptButton = this.btnFiltrar;
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(884, 471);
            this.Controls.Add(this.lblDesdeCaption);
            this.Controls.Add(this.dtpDesde);
            this.Controls.Add(this.lblHastaCaption);
            this.Controls.Add(this.dtpHasta);
            this.Controls.Add(this.lblTipoCaption);
            this.Controls.Add(this.cboTipo);
            this.Controls.Add(this.lblEventoCaption);
            this.Controls.Add(this.cboEvento);
            this.Controls.Add(this.lblPrioridadCaption);
            this.Controls.Add(this.cboPrioridad);
            this.Controls.Add(this.btnFiltrar);
            this.Controls.Add(this.btnLimpiar);
            this.Controls.Add(this.dgvEventos);
            this.Controls.Add(this.lblTotal);
            this.Name = "FrmEvent";
            this.Text = "Bitácora";
            ((System.ComponentModel.ISupportInitialize)(this.dgvEventos)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion
    }
}
