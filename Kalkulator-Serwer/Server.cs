using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Xml;

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

        IPAddress ip_address = IPAddress.Parse("127.0.0.1");
        int port_number = 2000;

        int history_limit = 100;
        int timeout = 100000;

        int number_of_connected_clients = 0;

        /// <summary>
        /// Konstruktor ładujący ustawienia z pliku oraz uruchamiający TcpListener
        /// </summary>
        public Server()
        {
            LoadSettingsFromFile();

            listener = new TcpListener(ip_address, port_number);
        }

        /// <summary>
        /// Funkcja ładująca ustawienia z pliku XML
        /// </summary>
        public void LoadSettingsFromFile()
        {
            string file = "settings.xml";

            if (File.Exists(file))
            {
                try
                {
                    using (XmlReader reader = XmlReader.Create(file))
                    {
                        while (reader.Read())
                        {
                            if (reader.IsStartElement())
                            {
                                switch (reader.Name.ToString())
                                {
                                    case "IP":
                                        ip_address = IPAddress.Parse(reader.ReadString());
                                        Console.WriteLine("IP: " + ip_address.ToString());
                                        break;
                                    case "Port":
                                        port_number = int.Parse(reader.ReadString());
                                        Console.WriteLine("Port: " + port_number.ToString());
                                        break;
                                    case "HistoryLimit":
                                        history_limit = int.Parse(reader.ReadString());
                                        Console.WriteLine("Limit historii: " + history_limit.ToString());
                                        break;
                                    case "Timeout":
                                        timeout = int.Parse(reader.ReadString());
                                        Console.WriteLine("Timeout: " + timeout.ToString() + "ms");
                                        break;
                                }
                            }
                        }
                    }
                }
                catch(XmlException exception)
                {
                    Console.WriteLine("Wystąpiły problemy z plikiem settings.xml");
                }
            }
            else
            {
                Console.WriteLine("Brak pliku settings.xml");
            }
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

        /// <summary>
        /// Funkcja która oczekuje na wiadomości oraz na nie odpowiada, działa aż do przesłania ciągu znaków EXIT
        /// </summary>
        public void ServerLoop(NetworkStream stream)
        {
            number_of_connected_clients++;

            stream.ReadTimeout = timeout;
            byte[] buffer_get = new byte[1024];
            byte[] buffer_send;

            List<string> operation_history = new List<string>();

            //buffer_send = Encoding.UTF8.GetBytes("Wprowadzaj liczby pojedynczo. W odpowiedzi otrzymasz te liczby spierwiastkowane. Napisz EXIT jeśli chcesz wyjść. Napisz CLIENTS jeśli chcesz poznać liczbę klientów aktualnie podłączonych. Zależnie od ustawień komputera jako przecinek stosuje się , lub .\r\n");
            buffer_send = Encoding.UTF8.GetBytes("Napisz HELP aby uzyskać pomoc.\r\n");
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

                    //Sprawdź czy to nie komenda
                    if (str.Equals("CLIENTS"))
                    {
                        //Zwróć liczbę podłączonych klientów
                        output_string = "Liczba klientów: " + number_of_connected_clients.ToString() + "\r\n";
                    }
                    else if (str.Equals("HELP"))
                    {
                        //Zwróć pomoc
                        output_string = "Wprowadzaj działania matematyczne, aby otrzymać listę dostępnych funkcji napisz FUNCTIONS. Napisz EXIT jeśli chcesz wyjść. Napisz CLIENTS jeśli chcesz poznać liczbę klientów aktualnie podłączonych. Napisz HISTORY aby otrzymać historię twoich ostatnich wykonanych obliczeń. Jeśli chcesz użyć funkcji to jeśli jako argument użyte jest jakieś złożone obliczenie zamiast jednej liczby to należy je włożyć w nawiasy. \r\n";
                    }
                    else if (str.Equals("FUNCTIONS"))
                    {
                        //Zwróć możliwe funkcje
                        output_string = "root(x, y) \r\n"
                                      + "sin(x) \r\n"
                                      + "cos(x) \r\n"
                                      + "tan(x) \r\n"
                                      + "factorial(x) \r\n"
                                      + "log(x, y) \r\n"
                                      + "ln(x) \r\n"
                                      + "abs(x) \r\n"
                                      + "exp(x) \r\n";
                    }
                    else if (str.Equals("HISTORY")) 
                    {
                        output_string = "";
                        
                        for(int i = 0; i < operation_history.Count; ++i)
                        {
                            output_string += (i + 1).ToString() + ".) " + operation_history[i];
                        }
                    
                    }
                    else
                    {
                        try
                        {
                            Queue<string> converted_string = Parser.ParseString(str);
                            Obliczenia licz1 = new Obliczenia(converted_string);
                            licz1.Wykonaj(); //Obliczanie ostatecznego wyniku z obliczenia
                            double result = licz1.Wynik();

                            if (licz1.BylBlad())
                            {
                                output_string = licz1.TextBledu() + "\r\n";
                            }
                            else
                            {
                                output_string = str + " = " + result.ToString() + "\r\n";

                                operation_history.Add(output_string);

                                if (operation_history.Count > history_limit)
                                {
                                    operation_history.RemoveAt(0);
                                }
                            }
                        }
                        catch
                        {
                            //Jeśli byl nieznany blad to poinformuj uzytkownika
                            output_string = "Błąd przy przetwarzaniu dzialania.\r\n";
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
