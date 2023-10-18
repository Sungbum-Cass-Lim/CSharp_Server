using System;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;

namespace Server_Homework
{
    public class Client
    {
        private Client() { }
        private static readonly Lazy<Client> _Instance = new Lazy<Client>(() => new Client());
        public static Client Instance { get { return _Instance.Value; } }

        public int MyId = 0;
        public Socket ClientSocket = null;

        public TcpPacketHeader HeaderSturct;
        public TcpPacketData DataSturct;

        public int PacketSize = 0;
        public Packet ReceivePacket = new Packet();
        public Packet SendPacket = new Packet();
        public byte[] SendBuffer;
        public byte[] RecvBuffer;

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

            PacketSize = Marshal.SizeOf(HeaderSturct) + Marshal.SizeOf(DataSturct);
            SendBuffer = new byte[PacketSize];
            RecvBuffer = new byte[PacketSize];

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
            Send("SendConnetMsg: Success Connect");

            ClientSocket.BeginReceive(RecvBuffer, 0, PacketSize, SocketFlags.None, Receive, null);
        }

        public void Send(string Msg)
        {
            HeaderSturct = new TcpPacketHeader(1,2,3,4);
            DataSturct = new TcpPacketData(MyId, Msg);

            byte[] SendData = SendPacket.Write(HeaderSturct, DataSturct);

            ClientSocket.Send(SendData);
        }

        public void Receive(IAsyncResult Result)
        {
            ClientSocket.EndReceive(Result);

            ReceivePacket.Read(RecvBuffer);
            MyId = ReceivePacket.PacketData.UserId;

            Console.WriteLine($"RecvConnectMsg:Welcome!, Your ID Number Is {ReceivePacket.PacketData.UserId}");

            ClientSocket.BeginReceive(RecvBuffer, 0, PacketSize, SocketFlags.None, Receive, null);
        }
    }
}