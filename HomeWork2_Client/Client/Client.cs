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

        public Socket ClientSocket = null;
        public byte[] Buffer = new byte[128];

        public int MyId = 0;
        public bool IsConnect = false;

        static void Main(string[] args)
        {
            Console.WriteLine("State: Create Socket");
            Client.Instance.CreateSocket();

            while (true)
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

            ClientSocket.BeginConnect(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 7000), EndConnect, null);
        }

        void EndConnect(IAsyncResult Result)
        {
            Console.WriteLine("State: Success Connect!");

            ClientSocket.BeginReceive(Buffer, 0, new Packet().Pkt.PacketLength,
                SocketFlags.None, Receive, null); // 비동기 Receive 시작
        }

        public void Send(string Msg)
        {

        }

        public void Receive(IAsyncResult Result)
        {
            Console.WriteLine("Receive");

            Packet RecvPacket = new Packet();
            RecvPacket.Read(Buffer);

            Console.WriteLine($"ID:{RecvPacket.Pkt.Id} -> Message:{RecvPacket.Message}");

            ClientSocket.BeginReceive(Buffer, 0, RecvPacket.Pkt.PacketLength,
                SocketFlags.None, Receive, null); // 비동기 Receive 시작
        }

        public void Disconnect()
        {
            ClientSocket.Shutdown(SocketShutdown.Both);
            ClientSocket.Close();

            Console.WriteLine($"Disconnect Server");
        }

        #region OneCallFunc
        public void ConnectReceive(IAsyncResult Result)
        {

        }
        #endregion
    }
}