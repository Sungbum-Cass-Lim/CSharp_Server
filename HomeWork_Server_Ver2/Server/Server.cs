using System;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;

namespace Server_Homework
{
    public class Server
    {
        private Server() { }
        private static readonly Lazy<Server> _Instance = new Lazy<Server>(() => new Server());
        public static Server Instance { get { return _Instance.Value; } }

        public Socket ServerSocket = null;
        public int IDCount = 0;

        public List<ClientData> ClientList = new List<ClientData>();
        public Dictionary<int, ClientData> ClientDictionary = new Dictionary<int, ClientData>();

        static void Main(string[] args)
        {
            Console.WriteLine("Server State: Start");
            Server.Instance.Initialize();

            while (true)
            {

            }
        }

        public void Initialize()
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
            ClientData ClinetSocket = new ClientData(IDCount, ServerSocket.EndAccept(Result));

            ClientList.Add(ClinetSocket);
            ClientDictionary.Add(ClinetSocket.UserId, ClinetSocket);

            Send(ClinetSocket.UserId, "Welcome To This Server!");

            IDCount++;
            Listen();
        }

        #region Send/Receive Func
        public void Send(int Id, string Msg)
        {
            
        }
        public void SendOther(int Id, string Msg)
        {
            
        }
        public void SendAll(int Id, string Msg)
        {
            
        }

        public void Receive(IAsyncResult Result)
        {
            
        }

        public void Disconnect(int Id)
        {
            ClientDictionary[Id].UserSocket.Shutdown(SocketShutdown.Both);
            ClientDictionary[Id].UserSocket.Close();

            ClientList.Remove(ClientDictionary[Id]);
            ClientDictionary.Remove(Id);

            Console.WriteLine($"Disconnect Clinet ID: {Id}");
        }
        #endregion
    }
}