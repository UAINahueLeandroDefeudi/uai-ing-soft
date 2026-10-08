using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PatronObserver2
{
    public class Alumno : ISuscriberAlumno
    {
        public Alumno(string nombre, string apellido)
        {
            Nombre = nombre;
            Apellido = apellido;
        }

        public string Nombre { get; set; }

        public string Apellido { get; set; }

        public override string ToString()
        {
            return $"{Nombre} {Apellido}";
        }

        public void Actualizar(Materia m)
        {
            Form1 f = (Form1)Application.OpenForms[0];
            f.Notificar($"   * {this}");
        }
    }
}
