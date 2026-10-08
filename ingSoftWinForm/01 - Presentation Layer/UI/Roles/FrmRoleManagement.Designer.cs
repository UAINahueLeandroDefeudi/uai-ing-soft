namespace UI.Roles
{
    partial class FrmRoleManagement
    {
        private System.ComponentModel.IContainer components = null;

        private System.Windows.Forms.Label lblRoles;
        private System.Windows.Forms.TreeView tvRoles;
        private System.Windows.Forms.Label lblUsuario;
        private System.Windows.Forms.ComboBox cbUsuarios;
        private System.Windows.Forms.Button btnAsignarRolUsuario;
        private System.Windows.Forms.Button btnQuitarRolUsuario;
        private System.Windows.Forms.Label lblEfectivos;
        private System.Windows.Forms.TreeView tvEfectivos;
        private System.Windows.Forms.Button btnAsignarPermisoRol;
        private System.Windows.Forms.Button btnQuitarPermisoRol;
        private System.Windows.Forms.Label lblCatalogo;
        private System.Windows.Forms.TreeView tvCatalogo;
        private System.Windows.Forms.Label lblNombreRol;
        private System.Windows.Forms.TextBox txtNombreRol;
        private System.Windows.Forms.Button btnCrearRol;
        private System.Windows.Forms.Button btnEliminarRol;

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
            lblRoles = new Label();
            tvRoles = new TreeView();
            lblUsuario = new Label();
            cbUsuarios = new ComboBox();
            btnAsignarRolUsuario = new Button();
            btnQuitarRolUsuario = new Button();
            lblEfectivos = new Label();
            tvEfectivos = new TreeView();
            btnAsignarPermisoRol = new Button();
            btnQuitarPermisoRol = new Button();
            lblCatalogo = new Label();
            tvCatalogo = new TreeView();
            lblNombreRol = new Label();
            txtNombreRol = new TextBox();
            btnCrearRol = new Button();
            btnEliminarRol = new Button();
            SuspendLayout();
            //
            // lblRoles
            //
            lblRoles.AutoSize = true;
            lblRoles.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblRoles.Location = new Point(15, 12);
            lblRoles.Name = "lblRoles";
            lblRoles.TabIndex = 0;
            lblRoles.Text = "Estructura Jerárquica de Roles (Árbol de Permisos)";
            //
            // tvRoles
            //
            tvRoles.HideSelection = false;
            tvRoles.Location = new Point(15, 38);
            tvRoles.Name = "tvRoles";
            tvRoles.Size = new Size(400, 280);
            tvRoles.TabIndex = 1;
            //
            // lblUsuario
            //
            lblUsuario.AutoSize = true;
            lblUsuario.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblUsuario.Location = new Point(430, 12);
            lblUsuario.Name = "lblUsuario";
            lblUsuario.TabIndex = 2;
            lblUsuario.Text = "Seleccionar Usuario";
            //
            // cbUsuarios
            //
            cbUsuarios.DropDownStyle = ComboBoxStyle.DropDownList;
            cbUsuarios.Location = new Point(430, 38);
            cbUsuarios.Name = "cbUsuarios";
            cbUsuarios.Size = new Size(240, 28);
            cbUsuarios.TabIndex = 3;
            cbUsuarios.SelectedIndexChanged += CbUsuarios_SelectedIndexChanged;
            //
            // btnAsignarRolUsuario
            //
            btnAsignarRolUsuario.Location = new Point(430, 80);
            btnAsignarRolUsuario.Name = "btnAsignarRolUsuario";
            btnAsignarRolUsuario.Size = new Size(240, 45);
            btnAsignarRolUsuario.TabIndex = 4;
            btnAsignarRolUsuario.Text = "Asignar Rol a Usuario";
            btnAsignarRolUsuario.UseVisualStyleBackColor = true;
            btnAsignarRolUsuario.Click += BtnAsignarRolUsuario_Click;
            //
            // btnQuitarRolUsuario
            //
            btnQuitarRolUsuario.Location = new Point(430, 132);
            btnQuitarRolUsuario.Name = "btnQuitarRolUsuario";
            btnQuitarRolUsuario.Size = new Size(240, 45);
            btnQuitarRolUsuario.TabIndex = 5;
            btnQuitarRolUsuario.Text = "Quitar Rol a Usuario";
            btnQuitarRolUsuario.UseVisualStyleBackColor = true;
            btnQuitarRolUsuario.Click += BtnQuitarRolUsuario_Click;
            //
            // lblEfectivos
            //
            lblEfectivos.AutoSize = true;
            lblEfectivos.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblEfectivos.Location = new Point(685, 12);
            lblEfectivos.Name = "lblEfectivos";
            lblEfectivos.TabIndex = 6;
            lblEfectivos.Text = "Permisos Efectivos del Usuario Seleccionado";
            //
            // tvEfectivos
            //
            tvEfectivos.HideSelection = false;
            tvEfectivos.Location = new Point(685, 38);
            tvEfectivos.Name = "tvEfectivos";
            tvEfectivos.Size = new Size(320, 280);
            tvEfectivos.TabIndex = 7;
            //
            // btnAsignarPermisoRol
            //
            btnAsignarPermisoRol.Location = new Point(15, 328);
            btnAsignarPermisoRol.Name = "btnAsignarPermisoRol";
            btnAsignarPermisoRol.Size = new Size(200, 40);
            btnAsignarPermisoRol.TabIndex = 8;
            btnAsignarPermisoRol.Text = "Asignar Permiso a Rol";
            btnAsignarPermisoRol.UseVisualStyleBackColor = true;
            btnAsignarPermisoRol.Click += BtnAsignarPermisoRol_Click;
            //
            // btnQuitarPermisoRol
            //
            btnQuitarPermisoRol.Location = new Point(225, 328);
            btnQuitarPermisoRol.Name = "btnQuitarPermisoRol";
            btnQuitarPermisoRol.Size = new Size(190, 40);
            btnQuitarPermisoRol.TabIndex = 9;
            btnQuitarPermisoRol.Text = "Quitar Permiso de Rol";
            btnQuitarPermisoRol.UseVisualStyleBackColor = true;
            btnQuitarPermisoRol.Click += BtnQuitarPermisoRol_Click;
            //
            // lblCatalogo
            //
            lblCatalogo.AutoSize = true;
            lblCatalogo.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblCatalogo.Location = new Point(15, 385);
            lblCatalogo.Name = "lblCatalogo";
            lblCatalogo.TabIndex = 10;
            lblCatalogo.Text = "Catálogo General de Permisos y Roles Disponibles";
            //
            // tvCatalogo
            //
            tvCatalogo.HideSelection = false;
            tvCatalogo.Location = new Point(15, 411);
            tvCatalogo.Name = "tvCatalogo";
            tvCatalogo.Size = new Size(400, 290);
            tvCatalogo.TabIndex = 11;
            //
            // lblNombreRol
            //
            lblNombreRol.AutoSize = true;
            lblNombreRol.Location = new Point(430, 411);
            lblNombreRol.Name = "lblNombreRol";
            lblNombreRol.TabIndex = 12;
            lblNombreRol.Text = "Nombre del nuevo rol";
            //
            // txtNombreRol
            //
            txtNombreRol.Location = new Point(430, 437);
            txtNombreRol.MaxLength = 50;
            txtNombreRol.Name = "txtNombreRol";
            txtNombreRol.Size = new Size(240, 27);
            txtNombreRol.TabIndex = 13;
            //
            // btnCrearRol
            //
            btnCrearRol.Location = new Point(430, 475);
            btnCrearRol.Name = "btnCrearRol";
            btnCrearRol.Size = new Size(240, 45);
            btnCrearRol.TabIndex = 14;
            btnCrearRol.Text = "Crear Rol";
            btnCrearRol.UseVisualStyleBackColor = true;
            btnCrearRol.Click += BtnCrearRol_Click;
            //
            // btnEliminarRol
            //
            btnEliminarRol.Location = new Point(430, 527);
            btnEliminarRol.Name = "btnEliminarRol";
            btnEliminarRol.Size = new Size(240, 45);
            btnEliminarRol.TabIndex = 15;
            btnEliminarRol.Text = "Eliminar Rol";
            btnEliminarRol.UseVisualStyleBackColor = true;
            btnEliminarRol.Click += BtnEliminarRol_Click;
            //
            // FrmRoleManagement
            //
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1020, 720);
            Controls.Add(lblRoles);
            Controls.Add(tvRoles);
            Controls.Add(lblUsuario);
            Controls.Add(cbUsuarios);
            Controls.Add(btnAsignarRolUsuario);
            Controls.Add(btnQuitarRolUsuario);
            Controls.Add(lblEfectivos);
            Controls.Add(tvEfectivos);
            Controls.Add(btnAsignarPermisoRol);
            Controls.Add(btnQuitarPermisoRol);
            Controls.Add(lblCatalogo);
            Controls.Add(tvCatalogo);
            Controls.Add(lblNombreRol);
            Controls.Add(txtNombreRol);
            Controls.Add(btnCrearRol);
            Controls.Add(btnEliminarRol);
            Name = "FrmRoleManagement";
            Text = "Gestión de Roles";
            Load += FrmRoleManagement_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
    }
}
