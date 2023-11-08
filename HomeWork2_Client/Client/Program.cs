using System;

namespace Server_Homework
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            Client mainClient = new Client();

            await mainClient.CreateSocket();

            while (true)
            {
                await mainClient.Send(Console.ReadLine());
            }
        }
    }
}
