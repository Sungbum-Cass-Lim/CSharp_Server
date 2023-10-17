using System;
using System.Net;
using System.Net.Sockets;

namespace Server_Homework
{
    public class Client
    {
        private Client() { }
        private static readonly Lazy<Client> _Instance = new Lazy<Client>(() => new Client());
        public static Client Instance { get { return _Instance.Value; } }

        public Socket ClientSocket = null;

        static void Main(string[] args)
        {
            Console.WriteLine("State: Create Socket");
            Client.Instance.CreateSocket();

            while(true)
            {

            }
        }

        public void CreateSocket()
        {
            ClientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            Connect();
        }

        void Connect()
        {
            Console.WriteLine("State: Try Connect...");

            ClientSocket.BeginConnect(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 7000), EndConnect,null);
        }

        void EndConnect(IAsyncResult Result)
        {
            Console.WriteLine("State: Success Connect!");
        }
    }
}