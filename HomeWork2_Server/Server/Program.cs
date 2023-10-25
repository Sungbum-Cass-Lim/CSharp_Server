using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server_Homework
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Server MainServer = new Server();

            MainServer.Initialize();

            while (true)
            {
                Console.ReadKey();
            }
        }
    }
}
