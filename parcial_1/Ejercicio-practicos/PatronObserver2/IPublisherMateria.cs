using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PatronObserver2
{
    public interface IPublisherMateria
    {
        IReadOnlyList<ISuscriberAlumno> suscribers { get; }

        void suscribe(ISuscriberAlumno suscriber);

        void unsuscribe(ISuscriberAlumno suscriber);

        void notifySuscribers();               
    }
}
