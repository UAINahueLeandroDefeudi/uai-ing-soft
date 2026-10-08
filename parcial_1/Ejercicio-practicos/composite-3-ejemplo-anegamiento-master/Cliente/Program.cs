using Anegamiento;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cliente
{
    class Program
    {
        static void Main(string[] args)
        {

           
            ComponenteTerreno root = new TerrenoAnegado(0);

            ConsoleKeyInfo resp;
            do
            {
                Console.Write("Seleccione tipo de terreno  [(A)gua-(T)ierra-a(N)egado] ");
                resp = Console.ReadKey();
                Console.WriteLine("\n");
                if (resp.KeyChar == 'N') // Si es anegado lo divido en 4
                {
                    root = new TerrenoAnegado(0);
                    AnalizarTerreno(root);
                }
                else if (resp.KeyChar == 'T')
                {
                    root = new TerrenoTierra(0);
               
                }
                else if (resp.KeyChar == 'A')
                {
                    root = new TerrenoAgua(0);
                
                }
            }
            while (!(resp.KeyChar == 'A' || resp.KeyChar == 'T' || resp.KeyChar == 'N'));
            Console.WriteLine("total porcentaje agua: " + root.PorcentajeAgua);
            Console.WriteLine("total porcentaje tierra: " + root.PorcentajeTierra);
            Console.ReadKey();

        }
        static void AnalizarTerreno(ComponenteTerreno terreno)
{
    ConsoleKeyInfo resp;
    if (terreno is TerrenoAnegado)
    {
        // si esta anegado, lo divido en 4
        // para mejorar esto, se puder hacer polimorfico un método de division de terreno, 


        for (int x = 1; x <= 4; x++)
        {
            do
            {
                Console.Write("Seleccione tipo de terreno en cuadrante " + x.ToString() + " [(A)gua-(T)ierra-a(N)egado] ");
                resp = Console.ReadKey();
                        Console.WriteLine("\n");

                    }
            while (!(resp.KeyChar == 'A'|| resp.KeyChar == 'T' || resp.KeyChar == 'N'));
            if (resp.KeyChar == 'A')
            {
                terreno.AgregarHijo(new TerrenoAgua(x));
            }
            else if (resp.KeyChar == 'T')
            {
                terreno.AgregarHijo(new TerrenoTierra(x));
            }
            else if (resp.KeyChar == 'N')
            {
                terreno.AgregarHijo(new TerrenoAnegado(x));
            }
        }

        // analizo los hijos
        foreach (ComponenteTerreno hijo in terreno.ObtenerHijos())
            AnalizarTerreno(hijo);
    }
}
}    

}
