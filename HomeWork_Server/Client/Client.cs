using System;
using System.Net;
using System.Net.Sockets;

namespace Server_Homework
{
    public class Client
    {
        public Socket ServerSocket = null;

        static void Main(string[] args)
        {
            Console.WriteLine("Server Start");
        }

        public void Bind()
        {
            ServerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            ServerSocket.Bind(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 7000));

            Listen();
        }

        public void Listen()
        {
            ServerSocket.Listen(5);
            ServerSocket.BeginAccept(Accept, null);
        }

        public void Accept(IAsyncResult Result)
        {
            Listen();
        }
    }
}