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
        private int IDCount = 0;

        private readonly object LockObj = new object();
        private List<ClientSocket> ClientSocketList = new List<ClientSocket>();
        //private ConcurrentBag<ClientSocket> ClientSocketList = new ConcurrentBag<ClientSocket>();

        public void Initialize()
        {
            Console.WriteLine("Server State: Start");

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
                ClientSocket ClientSocket = new ClientSocket().Initialize(IDCount, ServerSocket.EndAccept(Result));

                ClientSocketList.Add(ClientSocket);

                ClientSocket.Send(IDCount, "Welcome Bro"); // 클라 첫 접속 메세지
                Console.WriteLine($"Server State: Accept Client Socket Number {IDCount}");

                IDCount++;
                ServerSocket.BeginAccept(Accept, null);
            }
        }

        public void MultiCast(int Id, string Msg)
        {
            foreach (ClientSocket ClientSocket in ClientSocketList)
            {
                if (ClientSocket.GetId() != Id)
                {
                    ClientSocket.Send(Id, Msg);
                }
            }
        }

        public void Broadcast(int Id, string Msg)
        {
            foreach (ClientSocket ClientSocket in ClientSocketList)
            {
                ClientSocket.Send(Id, Msg);
            }
        }

        public void RemoveSocket(ClientSocket TargetSocket)
        {
            lock (LockObj)
            {
                Console.WriteLine($"Disconnect Clinet ID: {TargetSocket.GetId()}");
                ClientSocketList.RemoveAt(TargetSocket.GetId());
            }
        }
    }
}