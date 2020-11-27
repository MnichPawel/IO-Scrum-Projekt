using Kalkulator_Serwer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Net;

namespace Projekt_IO_Kalkulator
{
    class Program
    {
        static void Main(string[] args)
        {
            Server server = new Server(IPAddress.Parse("127.0.0.1"), 2000);

            server.WaitForClients(); //Serwer czeka na klientow
        }
    }
}
