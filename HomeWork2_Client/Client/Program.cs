using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Server_Homework
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            Client mainClient = new Client();

            mainClient.CreateSocket();

            while (true)
            {
                await mainClient.Send(Console.ReadLine());
            }
        }
    }
}
