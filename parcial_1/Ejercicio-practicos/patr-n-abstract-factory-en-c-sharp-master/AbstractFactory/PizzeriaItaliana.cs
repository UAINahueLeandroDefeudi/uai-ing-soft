
namespace AbstractFactory
{
    public class PizzeriaItaliana : Pizzeria
    {
        public override Empanada CrearEmpanada()
        {
            return new EmpanadaCapresse();
        }

        public override Pizza CrearPizza()
        {
            return new PizzaNapolitana();
        }
    }
}