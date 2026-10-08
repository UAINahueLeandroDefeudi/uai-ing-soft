using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Observer1.UI
{


    public class Usuario : IObserverUsuario
    {
       
        public Usuario(string nombre,string apellido)
        {
            Nombre = nombre;
            Apellido = apellido;
        }
        public string Nombre { get; set; }
        public string Apellido { get; set; }

        private readonly List<string> _notificaciones = new List<string>();
        public IReadOnlyList<string> Notificaciones
        {
            get { return _notificaciones.AsReadOnly(); }
        }

        public event Action<Usuario, string> NotificacionRecibida;


        public override string ToString()
        {
            return $"{Nombre} {Apellido}";
        }


        public void Actualizar(Producto p)
        {
            string mensaje = $"El usuario {this} recibio la notificacion:\n";
            mensaje += $"  - producto: {p}";
            _notificaciones.Add(mensaje);
            NotificacionRecibida?.Invoke(this, mensaje);
            
    
         }
        
      
    }
}
