using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PatronObserver2
{
    public class Materia : IPublisherMateria
    {
        private List<ISuscriberAlumno> _alumnos;

        public Materia(string nombre, DiaSemana diaSemana)
        {
            _alumnos = new List<ISuscriberAlumno>();
            Nombre = nombre;
            _diaSemana = diaSemana;
        }

        public string Nombre { get; set; }

        public IReadOnlyList<ISuscriberAlumno> suscribers
        {
            get { return _alumnos.AsReadOnly(); }
        }

        DiaSemana _diaSemana;
        public DiaSemana DiaSemana
        {
            get
            {
                return _diaSemana;
            }
            set
            {
                _diaSemana = value;
                this.notifySuscribers();
            }
        }

        public override string ToString()
        {
            return $"{Nombre} ({_diaSemana})";
        }

        public void notifySuscribers()
        {
            foreach (var alumno in _alumnos)
            {
                alumno.Actualizar(this);
            }

            Form1 f = (Form1)Application.OpenForms[0];

            if (_alumnos.Count == 0)
            {
                f.Notificar("No hay suscripciones");
            }

            f.Notificar("---------------------------------------------------------------------------------");
        }

        public void suscribe(ISuscriberAlumno suscriber)
        {
            if (!_alumnos.Contains(suscriber))
            {
                _alumnos.Add(suscriber);
            }
            else
            {
                throw new Exception($"Ya existe una suscripción para {suscriber}");
            }
        }

        public void unsuscribe(ISuscriberAlumno suscriber)
        {
            if (_alumnos.Contains(suscriber))
            {
                _alumnos.Remove(suscriber);
            }
            else
            {
                throw new Exception($"No existe una suscripción para {suscriber}");
            }
        }
    }
}
