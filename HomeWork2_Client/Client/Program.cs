using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server_Homework
{
    internal class Program
    {
        public static Client MainClient = new Client();

        static void Main(string[] args)
        {
            string Message = default;
            MainClient.CreateSocket();

            while (true)
            {
                if((Message = Console.ReadLine()) != null)
                    MainClient.Send(Message);
            }
        }
    }
}
