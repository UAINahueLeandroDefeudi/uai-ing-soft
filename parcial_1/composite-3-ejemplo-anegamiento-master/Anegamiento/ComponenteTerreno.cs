using System.Collections.Generic;

namespace Anegamiento
{
    public abstract class ComponenteTerreno
    {
        public int Cuadrante
        {
            get
            {
                return _cuadrante;
            }
        }

        private int _cuadrante;

        public ComponenteTerreno(int cuadrante)
        {
            _cuadrante = cuadrante;
        }

        public abstract double PorcentajeAgua { get; }
        public abstract double PorcentajeTierra { get; }

        public abstract object AgregarHijo(ComponenteTerreno componente);
        public abstract object EliminarHijo(ComponenteTerreno componente);
        public abstract List<ComponenteTerreno> ObtenerHijos();
    }
}