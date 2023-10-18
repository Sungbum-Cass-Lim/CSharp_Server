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

        public List<ClientData> ClientList = new List<ClientData>();
        public Dictionary<int, Socket> ClientDictionary = new Dictionary<int, Socket>();

        public TcpPacketHeader HeaderSturct;
        public TcpPacketData DataSturct;

        public int PacketSize = 0;
        public Packet ReceivePacket = new Packet();
        public Packet SendPacket = new Packet();
        public byte[] SendBuffer;
        public byte[] RecvBuffer;

        static void Main(string[] args)
        {
            Console.WriteLine("Server State: Start");
            Server.Instance.Initialize();

            while(true)
            { 
            
            }
        }

        public void Initialize()
        {
            Console.WriteLine("Server State: Bind");
            ServerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            ServerSocket.Bind(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 7000));

            PacketSize = Marshal.SizeOf(HeaderSturct) + Marshal.SizeOf(DataSturct);
            SendBuffer = new byte[PacketSize];
            RecvBuffer = new byte[PacketSize];

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

            ClientData ClinetSocket =  new ClientData(ClientList.Count, ServerSocket.EndAccept(Result));

            ClinetSocket.UserSocket.BeginReceive(RecvBuffer, 0, PacketSize, SocketFlags.None, Receive, null);

            ClientList.Add(ClinetSocket);
            ClientDictionary.Add(ClinetSocket.UserId, ClinetSocket.UserSocket);

            Listen();
        }

        #region Send/Receive Func
        public void SendMe()
        {

        }
        public void SendOther()
        {

        }
        public void SendAll()
        {

        }

        public void Receive(IAsyncResult Result)
        {
            ReceivePacket.Read(RecvBuffer);

            Console.WriteLine(ReceivePacket.PacketData.UserId);
            Console.WriteLine(ReceivePacket.PacketData.Message);
        }
        #endregion
    }
}