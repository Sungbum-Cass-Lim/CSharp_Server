using System;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;

namespace Server_Homework
{
    public class Server
    {
        //TODO: Packet 수정하면서 크기도 효율적일 수 있게 변경해야함
        const short MAX_PACKET_SIZE = 1024;
        const short MAX_BUFFER_SIZE = 1024;
        const short MAX_ID_COUNT = 256;

        private Server() { }
        private static readonly Lazy<Server> _Instance = new Lazy<Server>(() => new Server());
        public static Server Instance { get { return _Instance.Value; } }

        public Socket ServerSocket = null;
        public int IDCount = 0;

        public List<ClientData> ClientList = new List<ClientData>();
        public Dictionary<int, Socket> ClientDictionary = new Dictionary<int, Socket>();

        public byte[] SendBuffer = new byte[MAX_BUFFER_SIZE];
        public byte[] RecvBuffer = new byte[MAX_BUFFER_SIZE];

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

            ClinetSocket.UserSocket.BeginReceive(RecvBuffer, 0, MAX_PACKET_SIZE, SocketFlags.None, Receive, ClinetSocket.UserId);

            ClientList.Add(ClinetSocket);
            ClientDictionary.Add(ClinetSocket.UserId, ClinetSocket.UserSocket);

            Send(ClinetSocket.UserId, "Welcome To This Server!");

            IDCount++;
            Listen();
        }

        #region Send/Receive Func
        public void Send(int Id, string Msg)
        {
            TcpPacketHeader HeaderStrurct = new TcpPacketHeader(1, 2, 3, 4);
            TcpPacketData DataStrurct = new TcpPacketData(Id, Msg);

            byte[] SendData = new Packet().Write(HeaderStrurct, DataStrurct);

            ClientDictionary[Id].Send(SendData);
        }
        public void SendOther()
        {

        }
        public void SendAll()
        {

        }

        public void Receive(IAsyncResult Result)
        {
            int Id = (int)Result.AsyncState;
            ClientDictionary[Id].EndReceive(Result);

            Packet RecvPacket = new Packet();
            RecvPacket.Read(RecvBuffer);

            Console.WriteLine(RecvPacket.PacketData.UserId);
            Console.WriteLine(RecvPacket.PacketData.Message);

            RecvBuffer = new byte[MAX_BUFFER_SIZE];
            ClientDictionary[Id].BeginReceive(RecvBuffer, 0, MAX_PACKET_SIZE, SocketFlags.None, Receive, Id);
        }
        #endregion
    }
}