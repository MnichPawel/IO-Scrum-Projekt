using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Kalkulator_Serwer
{
    public class Parser {
        static public Queue<string> ParseString(string str)
        {
            Dictionary<char, int> precedence = new Dictionary<char, int>();
            precedence.Add('+', 2);
            precedence.Add('-', 2);
            precedence.Add('/', 3);
            precedence.Add('*', 3);
            precedence.Add('^', 4);

            Queue<string> to_return_queue = new Queue<string>();

            Stack<string> operacje = new Stack<string>();

            for(int i = 0; i < str.Length; ++i)
            {
                char znak = str[i];
                if (Char.IsWhiteSpace(znak) || znak == ',')
                {
                    //ignoruj
                }
                else if (Char.IsDigit(znak))
                {
                    string liczba = "";
                    /*if(operacje.Peek() == "-")
                    {
                        operacje.Pop();
                        liczba += "-";
                    }*/
                    liczba += znak;

                    int temp_i = i + 1;
                    while(temp_i < str.Length && (str[temp_i] == '.' || Char.IsDigit(str[temp_i])))
                    {
                        liczba += str[temp_i];
                        temp_i++;
                    }
                    i = temp_i - 1;
                    to_return_queue.Enqueue(liczba);
                }
                else if (znak == 'r' && str[i + 1] == 'o' && str[i + 2] == 'o' && str[i + 3] == 't')
                {
                    operacje.Push("root");
                    i += 3;
                }
                else if(znak == ',') { 
                    while(operacje.Peek().ToCharArray()[0] != '(')
                    {
                        to_return_queue.Enqueue(operacje.Pop());
                    }
                
                }
                else if (znak == '+' || znak == '-' || znak == '*' || znak == '/' || znak == '^')
                {
                    while(
                        operacje.Count > 0 && 
                        operacje.Peek().Length == 1 &&
                        operacje.Peek().ToCharArray()[0] != '(' &&
                        (
                            precedence[operacje.Peek().ToCharArray()[0]] > precedence[znak] ||
                            (precedence[operacje.Peek().ToCharArray()[0]] == precedence[znak] && znak != '^')
                        )
                        
                    )
                    {
                        to_return_queue.Enqueue(operacje.Pop());
                    }
                    operacje.Push(znak.ToString());
                }
                else if(znak == '(')
                {
                    operacje.Push(znak.ToString());
                }
                else if (znak == ')')
                {
                    while (
                       operacje.Peek().ToCharArray()[0] != '('
                    )
                    {
                        to_return_queue.Enqueue(operacje.Pop());
                    }

                    if(operacje.Peek().ToCharArray()[0] == '(')
                    {
                        operacje.Pop();
                    }

                    if(operacje.Peek() == "root")
                    {
                        to_return_queue.Enqueue(operacje.Pop());
                    }
                }
            }

            while(operacje.Count > 0)
            {
                to_return_queue.Enqueue(operacje.Pop());
            }

            return to_return_queue;
        }
    }
}