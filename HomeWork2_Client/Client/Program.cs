using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;


namespace Server_Homework
{
    internal class Program
    {
        static unsafe void Main(string[] args)
        {
            Client MainClient = new Client();

            string Message = null;
            MainClient.CreateSocket();

            while (true)
            {
                if ((Message = Console.ReadLine()) != null)
                {
                    MainClient.Send(Message);
                    Message = null;
                }
            }
        }
    }
}
