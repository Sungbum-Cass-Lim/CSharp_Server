using System;

namespace Server_Homework
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            Client MainClient = new Client();
            await MainClient.CreateSocket();

            while (true)
            {
                await MainClient.Send(Console.ReadLine());
            }
        }
    }
}
