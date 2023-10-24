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

            string InputMsg = null;

            while (true)
            {
                if ((InputMsg = Console.ReadLine()) != null)
                {
                    Client.Instance.Send(InputMsg);
                    InputMsg = null;
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
            Packet SendPacket = new Packet(1, 1, MyId, Msg);
            ClientSocket.Send(SendPacket.Write());
        }

        public void Receive(IAsyncResult Result)
        {
            Console.WriteLine("Receive");

            Packet RecvPacket = new Packet();
            RecvPacket.Read(Buffer);

            Console.WriteLine($"ID:{RecvPacket.Pkt.Id} -> Message:{RecvPacket.Message}");
            if (IsConnect == false)
            {
                MyId = RecvPacket.Pkt.Id;
                IsConnect = true;
            }

            if (RecvPacket.Message == "Q" || RecvPacket.Message == "q") // 접속 종료
                Disconnect();

            else
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