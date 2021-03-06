﻿using System;
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
                else if (znak == 's' && str[i + 1] == 'i' && str[i + 2] == 'n')
                {
                    operacje.Push("sin");
                    i += 2;
                }
                else if (znak == 'c' && str[i + 1] == 'o' && str[i + 2] == 's')
                {
                    operacje.Push("cos");
                    i += 2;
                }
                else if (znak == 't' && str[i + 1] == 'a' && str[i + 2] == 'n')
                {
                    operacje.Push("tan");
                    i += 2;
                }
                else if (znak == 'f' && str[i + 1] == 'a' && str[i + 2] == 'c' && str[i + 3] == 't' && str[i + 4] == 'o' && str[i + 5] == 'r' && str[i + 6] == 'i' && str[i + 7] == 'a' && str[i + 8] == 'l')
                {
                    operacje.Push("factorial");
                    i += 8;
                }
                else if (znak == 'l' && str[i + 1] == 'o' && str[i + 2] == 'g')
                {
                    operacje.Push("log");
                    i += 2;
                }
                else if (znak == 'l' && str[i + 1] == 'n')
                {
                    operacje.Push("ln");
                    i += 1;
                }
                else if (znak == 'a' && str[i + 1] == 'b' && str[i + 2] == 's')
                {
                    operacje.Push("abs");
                    i += 2;
                }
                else if (znak == 'e' && str[i + 1] == 'x' && str[i + 2] == 'p')
                {
                    operacje.Push("exp");
                    i += 2;
                }
                else if ((znak == 'p' || znak == 'P') && (str[i + 1] == 'i' || str[i + 1] == 'I'))
                {
                    to_return_queue.Enqueue(Math.PI.ToString());
                    i += 1;
                }
                else if (znak == 'e' || znak == 'E')
                {
                    to_return_queue.Enqueue(Math.E.ToString());
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