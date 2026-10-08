using System.Collections.Generic;

namespace Anegamiento
{
    public class TerrenoAnegado : ComponenteTerreno
    {
        public TerrenoAnegado(int cuadrante) : base(cuadrante)
        {
        }

        private List<ComponenteTerreno> _hijos = new List<ComponenteTerreno>();

        public override double PorcentajeAgua
        {
            get
            {
                var tot = default(double);
                foreach (var t in _hijos)
                    tot += t.PorcentajeAgua;
                return tot / 4d;
            }
        }

        public override double PorcentajeTierra
        {
            get
            {
                var tot = default(double);
                foreach (var t in _hijos)
                    tot += t.PorcentajeTierra;
                return tot / 4d;
            }
        }

        public override object AgregarHijo(ComponenteTerreno componente)
        {
            _hijos.Add(componente);
            return default;
        }

        public override object EliminarHijo(ComponenteTerreno componente)
        {
            _hijos.Remove(componente);
            return default;
        }

        public override List<ComponenteTerreno> ObtenerHijos()
        {
            return _hijos;
        }
    }
}