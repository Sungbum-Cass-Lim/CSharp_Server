using System;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;

namespace Server_Homework
{
    public class Client
    {
        const short MAX_PACKET_SIZE = 1024;
        const short MAX_BUFFER_SIZE = 1024;

        private Client() { }
        private static readonly Lazy<Client> _Instance = new Lazy<Client>(() => new Client());
        public static Client Instance { get { return _Instance.Value; } }

        public Socket ClientSocket = null;

        public int MyId = 0;
        public bool IsConnect = false;

        public byte[] SendBuffer = new byte[MAX_BUFFER_SIZE];
        public byte[] RecvBuffer = new byte[MAX_BUFFER_SIZE];

        static void Main(string[] args)
        {
            Console.WriteLine("State: Create Socket");
            Client.Instance.CreateSocket();

            while(true)
            {
                string Message = Console.ReadLine();

                if (Message != null)
                {
                    Client.Instance.Send(Message);
                }
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
            ClientSocket.BeginReceive(RecvBuffer, 0, MAX_PACKET_SIZE, SocketFlags.None, ConnectReceive, null);
        }

        public void Send(string Msg)
        {
            Console.WriteLine("Send");
            TcpPacketHeader HeaderSturct = new TcpPacketHeader(1, 2, 3, 4);
            TcpPacketData DataSturct = new TcpPacketData(MyId, Msg);

            byte[] SendData = new Packet().Write(HeaderSturct, DataSturct);

            ClientSocket.Send(SendData);
        }

        public void Receive(IAsyncResult Result)
        {
            ClientSocket.EndReceive(Result);

            Packet RecvPacket = new Packet();
            RecvPacket.Read(RecvBuffer);

            ClientSocket.BeginReceive(RecvBuffer, 0, MAX_PACKET_SIZE, SocketFlags.None, Receive, null);
        }

        #region OneCallFunc
        public void ConnectReceive(IAsyncResult Result)
        {
            ClientSocket.EndReceive(Result);

            Packet RecvPacket = new Packet();
            RecvPacket.Read(RecvBuffer);

            MyId = RecvPacket.PacketData.UserId;

            Console.WriteLine($"Your ID: {MyId}, {RecvPacket.PacketData.Message}");
            ClientSocket.BeginReceive(RecvBuffer, 0, MAX_PACKET_SIZE, SocketFlags.None, Receive, null);

            IsConnect = true;
        }
        #endregion
    }
}