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
            Server mainServer = new Server();

            mainServer.Initialize();

            while (true)
            {
                Console.ReadKey();
            }
        }
    }
}
