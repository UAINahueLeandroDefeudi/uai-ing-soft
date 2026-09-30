using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AbstractFactory
{
    public class PizzeriaChina : Pizzeria
    {
        public override Pizza CrearPizza()
        {
            return new PizzaSushi();
        }

        public override Empanada CrearEmpanada()
        {
            return new EmpanadaCeviche();
        }
    }
}
