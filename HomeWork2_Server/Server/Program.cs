using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server_Homework
{
    internal class Program
    {
        public static Server MainServer = new Server();

        static void Main(string[] args)
        {
            MainServer.Initialize();

            while (true)
            {
                Console.ReadKey();
            }
        }
    }
}
