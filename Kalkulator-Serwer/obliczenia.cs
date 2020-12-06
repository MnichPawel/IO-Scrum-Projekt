using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kalkulator_Serwer
{
    public class Obliczenia{
        public double wynik = 0.0;
        Queue<string> dzialanie;
        bool error;
        string error_text;

        public Obliczenia(Queue<string> dzialani)
        {
            dzialanie = dzialani;
        }

        public void Wykonaj(){
            Stack<double> numbers = new Stack<double>();

            Queue<string> str = dzialanie;

            while (str.Count > 0)
            {
                if (double.TryParse(str.Peek().Replace('.', ','), out double val))
                {
                    numbers.Push(val);
                }
                else if (str.Peek().Equals("+"))
                {
                    double val1 = numbers.Pop();
                    double val2 = numbers.Pop();
                    numbers.Push(Plus(val1, val2));
                }
                else if (str.Peek().Equals("-"))
                {
                    double val1 = numbers.Pop();
                    double val2 = numbers.Pop();
                    numbers.Push(Minus(val2, val1));
                }
                else if (str.Peek().Equals("*"))
                {
                    double val1 = numbers.Pop();
                    double val2 = numbers.Pop();
                    numbers.Push(Razy(val1, val2));
                }
                else if (str.Peek().Equals("/"))
                {
                    double val1 = numbers.Pop();
                    double val2 = numbers.Pop();
                    numbers.Push(Dziel(val2, val1));
                }
                else if (str.Peek().Equals("^"))
                {
                    double val1 = numbers.Pop();
                    double val2 = numbers.Pop();
                    numbers.Push(Pow(val2, val1));
                }
                else if (str.Peek().Equals("root"))
                {
                    double val1 = numbers.Pop();
                    double val2 = numbers.Pop();
                    numbers.Push(Root(val2, val1));
                }
                str.Dequeue();
            }


            wynik = numbers.Pop();
        }

        public double Plus(double x,double y) {
            return x + y;
        }
        public double Minus(double x, double y)
        {
            return x - y;
        }
        public double Razy(double x, double y)
        {
            return x * y;
        }
        public double Dziel(double x, double y)
        {
            if (y != 0)
            {
                return x / y;
            }
            else
            {
                error = true;
                error_text = "Nie mozna dzielic przez zero";
            }
            return 0.0;
        }
        public double Pow(double x, double y)
        {
            return Math.Pow(x, y);
        }
        public double Root(double x, double y)
        {
            double z = 1 / y;
            return Math.Pow(x, z);
        }
        public double Silnia(double x)
        {
            double w = 1;
            for (int i = 1; i <= x; i++){
                w *= i;
            }
            return w;
        }

        public bool BylBlad()
        {
            return error;
        }

        public string TextBledu()
        {
            return error_text;
        }

        public double Wynik()
        {
            return wynik;
        }
    }
}
