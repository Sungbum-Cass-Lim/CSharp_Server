using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;

namespace Server_Homework
{
    public class Server
    {
        private Socket ServerSocket = null;
        private PacketManager PacketManager = null;
        private int IDCount = 0;

        private readonly object LockObj = new object();

        private List<ClientSocket> ClientSocketList = new List<ClientSocket>();
        private ConcurrentDictionary<int, ClientSocket> ClientSocketDictionary = new ConcurrentDictionary<int, ClientSocket>();

        #region Server Start
        public void Initialize()
        {
            Console.WriteLine("Server State: Start");

            PacketManager = new PacketManager(this);

            ServerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            ServerSocket.Bind(new IPEndPoint(IPAddress.Any, 7000));

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
            lock (this)
            {
                ClientSocket ClientSocket = new ClientSocket().Initialize(this, IDCount, ServerSocket.EndAccept(Result));

                ClientSocketDictionary.TryAdd(IDCount, ClientSocket);
                ClientSocketList.Add(ClientSocket); // TODO: Lock 필요

                ClientSocket.Send(IDCount, "Welcome Bro"); // 클라 첫 접속 메세지
                Console.WriteLine($"Server State: Accept Client Socket Number {IDCount}");

                IDCount++;
                ServerSocket.BeginAccept(Accept, null);
            }
        }
        #endregion

        #region Server Send
        public void Unicast(int Id, string Msg) // 지정 전송
        {
            ClientSocketDictionary[Id].Send(Id, Msg);
        }

        public void Broadcast(int Id, string Msg) // 모두 전송
        {
            foreach (ClientSocket ClientSocket in ClientSocketList)
            {
                ClientSocket.Send(Id, Msg);
            }
        }

        public void Multicast(int Id, string Msg) // 해당 Id 빼고 모두 전송
        {
            foreach (ClientSocket ClientSocket in ClientSocketList)
            {
                if (ClientSocket.GetId() != Id)
                {
                    ClientSocket.Send(Id, Msg);
                }
            }
        }
        #endregion

        public void AddPacket(Packet Packet) // PacketQueue에 Packet 추가
        {
            PacketManager.AddPacket(Packet);
        }

        public void DisconnectScoket(int SocketID)
        {
            Console.WriteLine($"Disconnect Clinet ID: {SocketID}");

            // Lock을 안써도 상호 배제와 삭제 가능
            if (ClientSocketDictionary.TryRemove(SocketID, out ClientSocket? ClientSocket)) // 이 부분을 아예 건너뜀 왜?
            {
                ClientSocket.Send(SocketID, "Q");
                ClientSocket.Close();

                ClientSocketList.Remove(ClientSocket);
            } 
        }
    }
}