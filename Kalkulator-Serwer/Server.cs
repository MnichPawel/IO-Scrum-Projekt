using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Kalkulator_Serwer
{
    public class Server
    {
        string znak = "";
        double x, y = 0;
        public delegate void TransmissionDataDelegate(NetworkStream stream);

        TcpListener listener;
        //TcpClient client;
        //NetworkStream stream;

        IPAddress ip_address;
        int port_number;

        int number_of_connected_clients = 0;

        /// <summary>
        /// Konstruktor pobierający adres IP oraz port
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        public Server(IPAddress ip, int port)
        {
            ip_address = ip;
            port_number = port;

            listener = new TcpListener(ip, port);
        }
        /// <summary>
        /// Funkcja która kończy się dopiero gdy do serwera podepnie się klient
        /// </summary>
        public void WaitForClients()
        {
            while (true)
            {
                listener.Start();
                TcpClient client = listener.AcceptTcpClient();
                NetworkStream stream = client.GetStream();
                TransmissionDataDelegate transmissionDelegate = new TransmissionDataDelegate(ServerLoop);

                transmissionDelegate.BeginInvoke(stream, TransmissionCallback, client);
            }
        }

        private void TransmissionCallback(IAsyncResult ar)
        {
            TcpClient tcpClient = (TcpClient)ar.AsyncState;
            tcpClient.Close();
        }
        
        //Funkcja wykorzystująca klasę ShuntingYardParser do przetwarzania ciągu wejściowego na listę tokenów
        public IEnumerable<Projekt_IO_Kalkulator.Token> InfixToPostfix(string message)
        {
            var text = Console.ReadLine();
            using (var reader = new StringReader(text))
            {
                var parser = new Projekt_IO_Kalkulator.Parser();
                var tokens = parser.Tokenize(reader).ToList();
                return parser.ShuntingYard(tokens);
            }
        }

        /// <summary>
        /// Funkcja która oczekuje na wiadomości oraz na nie odpowiada, działa aż do przesłania ciągu znaków EXIT
        /// </summary>
        public void ServerLoop(NetworkStream stream)
        {
            number_of_connected_clients++;

            stream.ReadTimeout = 100000;
            byte[] buffer_get = new byte[1024];
            byte[] buffer_send;

            //buffer_send = Encoding.UTF8.GetBytes("Wprowadzaj liczby pojedynczo. W odpowiedzi otrzymasz te liczby spierwiastkowane. Napisz EXIT jeśli chcesz wyjść. Napisz CLIENTS jeśli chcesz poznać liczbę klientów aktualnie podłączonych. Zależnie od ustawień komputera jako przecinek stosuje się , lub .\r\n");
            buffer_send = Encoding.UTF8.GetBytes("Wprowadzaj działania matematyczne. Napisz EXIT jeśli chcesz wyjść. Napisz CLIENTS jeśli chcesz poznać liczbę klientów aktualnie podłączonych. Zależnie od ustawień komputera jako przecinek stosuje się , lub .\r\n");
            stream.Write(buffer_send, 0, buffer_send.Length);

            while (true)
            {
                try
                {
                    int size_recv = stream.Read(buffer_get, 0, 1024);

                    string str = Encoding.UTF8.GetString(buffer_get, 0, size_recv);

                    //Ignorowane są przejścia do następnej lini
                    if (str.Equals("\r\n")) { continue; }

                    //Zakończ pętle jeśli otrzymano string EXIT
                    if (str.Equals("EXIT")) { break; }

                    string output_string;

                    //Jeśli konwersja na typ double się uda wyślij wynik, jeśli nie to poinformuj użytkownika
                    if (true) //Test czy dzialanie jest poprawne np.: CheckCorrectness(str)
                    {
                        obliczenia licz1 = new obliczenia(x,y,znak);
                        double result =licz1.dzialanie(x,y,znak); //Obliczanie ostatecznego wyniku z obliczenia np Calculate(str)
                        
                        output_string = str + " = " + result.ToString() + "\r\n";
                    }
                    else
                    {
                        //Zwróć liczbę podłączonych klientów
                        if (str.Equals("CLIENTS"))
                        {
                            output_string = "Liczba klientów: " + number_of_connected_clients.ToString() + "\r\n";
                        }
                        else
                        {
                            output_string = "Nie wprowadzono poprawnego działania.\r\n";
                        }
                    }

                    buffer_send = Encoding.UTF8.GetBytes(output_string);

                    stream.Write(buffer_send, 0, buffer_send.Length);
                }
                catch (IOException e)
                {
                    break;
                }
            }

            number_of_connected_clients--;
        }
    }
}
