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
                string inputMsg = Console.ReadLine();

                Header header = new Header(inputMsg.Length, (int)PayloadTag.msg);
                Message msg = new Message(inputMsg);

                await mainClient.Send(header, msg);
            }
        }
    }
}
