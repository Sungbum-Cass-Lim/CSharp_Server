using System;

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
