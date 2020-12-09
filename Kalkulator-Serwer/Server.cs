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
            buffer_send = Encoding.UTF8.GetBytes("Napisz HELP aby uzyskać pomoc.");
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
                        output_string = "Wprowadzaj działania matematyczne, program obsługuje funkcje root(x, y). Napisz EXIT jeśli chcesz wyjść. Napisz CLIENTS jeśli chcesz poznać liczbę klientów aktualnie podłączonych. Jeśli chcesz użyć funkcji to jeśli jako argument użyte jest jakieś złożone obliczenie zamiast jednej liczby to należy je włożyć w nawiasy. \r\n";
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
                            }
                        }
                        catch
                        {
                            //Jeśli byl nieznany blad to poinformuj uzytkownika
                            output_string = "Błąd przy przetwarzaniu dzialania\r\n";
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
