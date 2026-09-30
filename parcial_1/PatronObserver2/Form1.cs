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

namespace PatronObserver2
{
    public partial class Form1 : Form
    {
        public void Notificar(string s)
        {
            foreach (var linea in s.Split('\n'))
            {
                this.lstNotificaciones.Items.Add(linea);
            }
        }

        private List<IPublisherMateria> _materias;
        private List<ISuscriberAlumno> _alumnos;

        private IPublisherMateria _materia;
        private ISuscriberAlumno _alumno;

        public Form1()
        {
            InitializeComponent();

            _materias = new List<IPublisherMateria>();
            _alumnos = new List<ISuscriberAlumno>();
            simularDatos();
        }

        private void simularDatos()
        {
            _materias.Add(new Materia("Ingeniería de Software", DiaSemana.Lunes));
            _materias.Add(new Materia("Base de Datos", DiaSemana.Martes));
            _materias.Add(new Materia("Programación", DiaSemana.Miercoles));
            _materias.Add(new Materia("Redes", DiaSemana.Jueves));
            _alumnos.Add(new Alumno("Diego", "Maradona"));
            _alumnos.Add(new Alumno("Leonel", "Messi"));
            _alumnos.Add(new Alumno("Mario", "Kempes"));

            mostrarMaterias();
            mostrarAlumnos();
            mostrarSuscripciones();
        }

        private void mostrarSuscripciones()
        {
            tvSuscripciones.BeginUpdate();
            tvSuscripciones.Nodes.Clear();
            foreach (var materia in _materias)
            {
                var nodo = tvSuscripciones.Nodes.Add(materia.ToString());
                foreach (var alumno in materia.suscribers)
                {
                    nodo.Nodes.Add(alumno.ToString());
                }
            }
            tvSuscripciones.ExpandAll();
            tvSuscripciones.EndUpdate();
        }

        private void mostrarMaterias()
        {
            this.lstMaterias.DataSource = null;
            this.lstMaterias.DataSource = _materias;
        }

        private void mostrarAlumnos()
        {
            this.lstAlumnos.DataSource = null;
            this.lstAlumnos.DataSource = _alumnos;
        }

        private void lstMaterias_SelectedValueChanged(object sender, EventArgs e)
        {
            _materia = (IPublisherMateria)((ListBox)sender).SelectedItem;
        }

        private void lstAlumnos_SelectedValueChanged(object sender, EventArgs e)
        {
            _alumno = (ISuscriberAlumno)((ListBox)sender).SelectedItem;
        }

        private void btnSuscribir_Click(object sender, EventArgs e)
        {
            if (_materia != null && _alumno != null)
            {
                try
                {
                    _materia.suscribe(_alumno);
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
                MessageBox.Show("Debe seleccionar materia y alumno");
            }
        }

        private void btnDesuscribir_Click(object sender, EventArgs e)
        {
            if (_materia != null && _alumno != null)
            {
                try
                {
                    _materia.unsuscribe(_alumno);
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
                MessageBox.Show("Debe seleccionar materia y alumno");
            }
        }

        private void lstMaterias_DoubleClick(object sender, EventArgs e)
        {
            string texto = Interaction.InputBox("Ingrese el nuevo día (lunes a domingo): ");
            texto = texto.Trim().Replace("é", "e").Replace("á", "a");

            DiaSemana dia;
            if (Enum.TryParse(texto, true, out dia) && Enum.IsDefined(typeof(DiaSemana), dia))
            {
                ((Materia)_materia).DiaSemana = dia;
                mostrarMaterias();
                mostrarSuscripciones();
            }
            else if (texto != "")
            {
                MessageBox.Show("Día inválido. Valores: " + string.Join(", ", Enum.GetNames(typeof(DiaSemana))));
            }
        }
    }
}
