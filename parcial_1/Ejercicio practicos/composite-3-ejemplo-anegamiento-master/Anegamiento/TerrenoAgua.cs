using System.Collections.Generic;

namespace Anegamiento
{
    public class TerrenoAgua : ComponenteTerreno
    {
        public TerrenoAgua(int cuadrante) : base(cuadrante)
        {
        }

        public override double PorcentajeAgua
        {
            get
            {
                return 100d;
            }
        }

        public override double PorcentajeTierra
        {
            get
            {
                return 0d;
            }
        }

        public override object AgregarHijo(ComponenteTerreno componente)
        {
            return default;
        }

        public override object EliminarHijo(ComponenteTerreno componente)
        {
            return default;
        }

        public override List<ComponenteTerreno> ObtenerHijos()
        {
            return default;
        }
    }
}