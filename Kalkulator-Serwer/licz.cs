using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kalkulator_Serwer
{
    public class licz{
        double x, y, wynik = 0.0;
        string znak = "";

     
        public double plus(double x,double y) {
            wynik = x + y;   
            return wynik;
        }
        public double minus(double x, double y)
        {
            wynik = x - y;
            return wynik;
        }
        public double razy(double x, double y)
        {
            wynik = x * y;
            return wynik;
        }
        public double dziel(double x, double y)
        {
            if (y != 0)
                wynik = x / y;
            else
                System.Console.WriteLine("nie mozna dzielić przez 0");
            return wynik;
        }
        public double pow(double x, double y)
        {
            wynik = Math.Pow(x, y);
            return wynik;
        }
        public double sqrt(double x, double y)
        {
            double z = 1 / y;
            wynik = Math.Pow(x, z);
            return wynik;
        }
        public double silnia(double x)
        {
            wynik = 1;
            for (int i = 1; i <= x; i++){
                wynik *= i;
            }
            return wynik;
        }
    }
}
