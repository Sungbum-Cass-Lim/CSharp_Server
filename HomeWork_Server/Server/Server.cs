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
        public Dictionary<int, ClientData> ClientDictionary = new Dictionary<int, ClientData>();

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
            Console.WriteLine("Server State: Binddddd");

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
            ClientDictionary.Add(ClinetSocket.UserId, ClinetSocket);

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

            ClientDictionary[Id].UserSocket.Send(SendData);
        }
        public void SendOther(int Id, string Msg)
        {
            TcpPacketHeader HeaderStrurct = new TcpPacketHeader(1, 2, 3, 4);
            TcpPacketData DataStrurct = new TcpPacketData(Id, Msg);

            byte[] SendData = new Packet().Write(HeaderStrurct, DataStrurct);

            foreach (var Client in ClientList)
            {
                if (Client.UserId != Id)
                {
                    Client.UserSocket.Send(SendData);
                }
            }
        }
        public void SendAll(int Id, string Msg)
        {
            TcpPacketHeader HeaderStrurct = new TcpPacketHeader(1, 2, 3, 4);
            TcpPacketData DataStrurct = new TcpPacketData(Id, Msg);

            byte[] SendData = new Packet().Write(HeaderStrurct, DataStrurct);

            foreach (var Client in ClientList)
            {
                Client.UserSocket.Send(SendData);
            }
        }

        public void Receive(IAsyncResult Result)
        {
            int Id = (int)Result.AsyncState;

            Packet RecvPacket = new Packet();
            RecvPacket.Read(RecvBuffer);

            Console.Write($"ID: {RecvPacket.PacketData.UserId} -> ");
            Console.WriteLine($"Message: {RecvPacket.PacketData.Message}");

            Send(Id, RecvPacket.PacketData.Message);

            if (RecvPacket.PacketData.Message == "Q" || RecvPacket.PacketData.Message == "q")
            {
                Disconnect(RecvPacket.PacketData.UserId);
                return;
            }

            ClientDictionary[Id].UserSocket.BeginReceive(RecvBuffer, 0, MAX_PACKET_SIZE, SocketFlags.None, Receive, Id);
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