using System;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;

namespace Server_Homework
{
    public class Server
    {
        private Socket ServerSocket = null;
        private int IDCount = 0;

        private List<ClientSocket> ClientList = new List<ClientSocket>();
        private Dictionary<int, ClientSocket> ClientDictionary = new Dictionary<int, ClientSocket>();

        private Queue<TcpPacket> SendPackets = new Queue<TcpPacket>();

        public void Initialize()
        {
            Console.WriteLine("Server State: Start");

            ServerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            ServerSocket.Bind(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 7000));

            Console.WriteLine("Server State: Bind");
            Listen();
        }

        public void Listen()
        {
            Console.WriteLine("Server State: Listen");

            ServerSocket.Listen(5);
            ServerSocket.BeginAccept(Accept, null);
        }

        private void Accept(IAsyncResult Result)
        {
            Console.WriteLine("Server State: Accept Client Socket");
            ClientSocket ClientSocket = new ClientSocket().Initialize(IDCount, ServerSocket.EndAccept(Result));

            ClientList.Add(ClientSocket);
            ClientDictionary.Add(IDCount, ClientSocket);

            ClientSocket.Send("Welcome Bro"); // 클라 첫 접속 메세지

            ClientSocket.BeginReceive(); // 비동기 Receive 시작

            IDCount++;
            Listen();
        }

        public void RemoveSocket(int Id)
        {
            ClientList.Remove(ClientDictionary[Id]);
            ClientDictionary.Remove(Id);

            Console.WriteLine($"Disconnect Clinet ID: {Id}");
        }
    }
}