using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Observer1.UI
{
    public partial class Form1 : Form
    {
        private void Notificar(string s)
        {
            foreach (var linea in s.Split('\n'))
            {
                this.lstNotificaciones.Items.Add(linea);
            }
        }
        private List<ISujetoProducto> _productos;
        private List<IObserverUsuario> _usuarios;


        private ISujetoProducto _producto;
        private IObserverUsuario _usuario;


        public Form1()
        {
            InitializeComponent();

            _productos = new List<ISujetoProducto>();
            _usuarios = new List<IObserverUsuario>();
            simularDatos();
        }

      
        private void simularDatos()
        {
            _productos.Add(new Producto("producto a", 100));
            _productos.Add(new Producto("producto b", 200));
            _productos.Add(new Producto("producto c", 300));
            _productos.Add(new Producto("producto d", 400));
            agregarUsuario(new Usuario("Diego", "Maradona"));
            agregarUsuario(new Usuario("Leonel", "Messi"));
            agregarUsuario(new Usuario("Paulo", "Silas"));

            mostrarProductos();
            mostrarUsuarios();
            mostrarSuscripciones();
        }

        private void agregarUsuario(Usuario usuario)
        {
            usuario.NotificacionRecibida += usuario_NotificacionRecibida;
            _usuarios.Add(usuario);
        }

        private void usuario_NotificacionRecibida(Usuario usuario, string mensaje)
        {
            Notificar(mensaje);
        }

        private void mostrarSuscripciones()
        {
            tvSuscripciones.BeginUpdate();
            tvSuscripciones.Nodes.Clear();
            foreach (var producto in _productos)
            {
                var nodo = tvSuscripciones.Nodes.Add(producto.ToString());
                foreach (var usuario in producto.Suscriptores)
                {
                    nodo.Nodes.Add(usuario.ToString());
                }
            }
            tvSuscripciones.ExpandAll();
            tvSuscripciones.EndUpdate();
        }

        private void mostrarProductos()
        {
            this.lstProductos.DataSource = null;
            this.lstProductos.DataSource = _productos;

        }
        private void mostrarUsuarios()
        {
            this.lstUsuarios.DataSource = null;
            this.lstUsuarios.DataSource = _usuarios;
        }

        private void lstProductos_SelectedValueChanged(object sender, EventArgs e)
        {

             _producto = (ISujetoProducto)((ListBox)sender).SelectedItem;

        }

        private void lstUsuarios_SelectedValueChanged(object sender, EventArgs e)
        {
            _usuario = (IObserverUsuario)((ListBox)sender).SelectedItem;
        }

        private void btnSuscribir_Click(object sender, EventArgs e)
        {
            if (_producto!=null && _usuario != null)
            {
                try
                {
                    _producto.Agregar(_usuario);
                    mostrarSuscripciones();
                    MessageBox.Show("Suscripción correcta");

                }
                catch (Exception ee)
                {

                    MessageBox.Show(ee.Message);
                }
            }
            else
            {
                MessageBox.Show("Debe seleccionar producto y usuario");
            }
        }

        private void btnDesuscribir_Click(object sender, EventArgs e)
        {
            if (_producto != null && _usuario != null)
            {
                try
                {
                    _producto.Quitar(_usuario);
                    mostrarSuscripciones();
                    MessageBox.Show("Desuscripción correcta");

                }
                catch (Exception ee)
                {

                    MessageBox.Show(ee.Message);
                }
               
            }
            else
            {
                MessageBox.Show("Debe seleccionar producto y usuario");
            }
        }

        private void lstProductos_DoubleClick(object sender, EventArgs e)
        {

            double p;

            if (double.TryParse(Interaction.InputBox("Ingrese el nuevo precio: "), out p))
            {
                ((Producto)_producto).Precio = p;
                if (_producto.Suscriptores.Count == 0)
                {
                    Notificar("No hay suscripciones");
                }
                Notificar("---------------------------------------------------------------------------------");
                mostrarProductos();
                mostrarSuscripciones();
            }

         
        }
    }
}
