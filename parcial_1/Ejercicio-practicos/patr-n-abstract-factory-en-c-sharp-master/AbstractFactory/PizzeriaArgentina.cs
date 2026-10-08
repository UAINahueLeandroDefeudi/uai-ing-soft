
namespace AbstractFactory
{
    public class PizzeriaArgentina : Pizzeria
    {
        public override Empanada CrearEmpanada()
        {
            return new EmpanadaDeCarne();
        }

        public override Pizza CrearPizza()
        {
            return new PizzaCancha();
        }
    }
}