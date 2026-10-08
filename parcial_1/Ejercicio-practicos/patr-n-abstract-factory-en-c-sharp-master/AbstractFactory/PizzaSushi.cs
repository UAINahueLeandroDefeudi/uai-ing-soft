using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AbstractFactory
{
    public class PizzaSushi : Pizza
    {
        public PizzaSushi()
        {
            _descripcion = "Pizza especial de sushi";
        }
    }
}
