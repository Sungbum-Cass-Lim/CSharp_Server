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
            Send("SendConnetMsg: Success Connect");
        }

        public void Send(string Msg)
        {
            TcpPacketHeader Header = new TcpPacketHeader(1,2,3,4);
            TcpPacketData Data = new TcpPacketData(MyId, Msg);

            Packet SendPacket = new Packet();
            byte[] SendData = SendPacket.Write(Header, Data);

            Packet RecvPacket = new Packet();

            ClientSocket.Send(SendData);
        }

        public void Receive(byte[] ReceiveData)
        {
            Packet ReceivePacket = new Packet();

            ReceivePacket = ReceivePacket.Read(ReceiveData);
            Console.WriteLine($"RecvConnectMsg:Welcome!, Your ID Number Is {ReceivePacket.PacketData.UserId}");
        }
    }
}