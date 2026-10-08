using System;
using System.Collections.Generic;

namespace Anegamiento
{
    public class TerrenoTierra : ComponenteTerreno
    {
        public TerrenoTierra(int cuadrante) : base(cuadrante)
        {
        }

        public override double PorcentajeAgua
        {
            get
            {
                return 0d;
            }
        }

        public override double PorcentajeTierra
        {
            get
            {
                return 100d;
            }
        }

        public override object AgregarHijo(ComponenteTerreno componente)
        {
            throw new NotImplementedException();
        }

        public override object EliminarHijo(ComponenteTerreno componente)
        {
            throw new NotImplementedException();
        }

        public override List<ComponenteTerreno> ObtenerHijos()
        {
            throw new NotImplementedException();
        }
    }
}