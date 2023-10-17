using System;
using System.Net;
using System.Net.Sockets;

namespace Server_Homework
{
    public class Server
    {
        private Server() { }
        private static readonly Lazy<Server> _Instance = new Lazy<Server>(() => new Server());
        public static Server Instance { get { return _Instance.Value; } }

        public Socket ServerSocket = null;

        public List<ClientData> ClientDatas = new List<ClientData>();

        static void Main(string[] args)
        {
            Console.WriteLine("Server State: Start");
            Server.Instance.Bind();

            while(true)
            { 
            
            }
        }

        public void Bind()
        {
            Console.WriteLine("Server State: Bind");

            ServerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            ServerSocket.Bind(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 7000));

            Listen();
        }

        public void Listen()
        {
            Console.WriteLine("Server State: Listen");

            ServerSocket.Listen(5);
            ServerSocket.BeginAccept(Accept, null);
        }

        public void Accept(IAsyncResult Result)
        {
            Console.WriteLine("Server State: Accept Client Socket");

            ClientData ClinetSocket =  new ClientData(ClientDatas.Count, ServerSocket.EndAccept(Result));
            ClientDatas.Add(ClinetSocket);

            Listen();
        }
    }
}