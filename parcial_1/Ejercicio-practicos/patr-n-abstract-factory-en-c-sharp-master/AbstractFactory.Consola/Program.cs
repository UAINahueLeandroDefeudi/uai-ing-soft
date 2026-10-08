using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AbstractFactory.Consola
{
    class Program
    {
        static void Main(string[] args)
        {
            Pizzeria fabrica;
            fabrica = new PizzeriaArgentina();

            Pizza pizza = fabrica.CrearPizza();
            Empanada empanada = fabrica.CrearEmpanada();
            Console.WriteLine($"Pizza: {pizza.Descripcion}, Empanada: {empanada.Descripcion}");

            fabrica = new PizzeriaItaliana();
            pizza = fabrica.CrearPizza();
            empanada = fabrica.CrearEmpanada();
            Console.WriteLine($"Pizza: {pizza.Descripcion}, Empanada: {empanada.Descripcion}");

            fabrica = new PizzeriaChina();
            pizza = fabrica.CrearPizza();
            empanada = fabrica.CrearEmpanada();
            Console.WriteLine($"Pizza: {pizza.Descripcion}, Empanada: {empanada.Descripcion}");

            Console.ReadKey();
        }
    }
}
