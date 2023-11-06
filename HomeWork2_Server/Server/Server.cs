﻿using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;

namespace Server_Homework
{
    public class Server
    {
        private Socket ServerSocket = null;
        private PacketProcessor PacketProcessor = null;
        private Task AcceptLoopTask;

        private int IDCount = 1;

        private readonly object LockObj = new object();

        private List<ClientSocket> ClientSocketList = new List<ClientSocket>();
        private ConcurrentDictionary<int, ClientSocket> ClientSocketDictionary = new ConcurrentDictionary<int, ClientSocket>();

        #region Server Start
        public void Initialize()
        {
            Console.WriteLine("Server State: Start");

            PacketProcessor = new PacketProcessor(this);

            ServerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            ServerSocket.Bind(new IPEndPoint(IPAddress.Any, 7000));

            Console.WriteLine("Server State: Bind");
            Listen();
        }

        public void Listen()
        {
            Console.WriteLine("Server State: Listen");

            ServerSocket.Listen(5);
            AcceptLoopTask = AcceptLoop(); // Accept Loop
        }
        #endregion

        #region ServerAsyncFunc
        public async Task AcceptLoop()
        {
            while (true)
            {
                await AcceptAsync();
                Console.WriteLine($"Server State: Accept Client Socket Number {IDCount - 1}");
            }
        }

        private async Task AcceptAsync()
        {
            try
            {
                ClientSocket NewClientSocket;
                NewClientSocket = new ClientSocket().Initialize(this, IDCount, await ServerSocket.AcceptAsync());

                if (ClientSocketDictionary.TryAdd(IDCount, NewClientSocket))
                {
                    ClientSocketList.Add(NewClientSocket); // ConcurrentDictionary를 통해 lock
                    NewClientSocket.Send(IDCount, "Welcome Bro"); // 클라 첫 접속 메세지

                    IDCount++;
                }
            }
            catch(Exception E)
            { 
                Console.WriteLine(E); 
            }
        }
        #endregion

        public void AddPacket(Packet Packet) // PacketQueue에 Packet 추가
        {
            PacketProcessor.AddPacket(Packet);
        }

        public void DisconnectScoket(int SocketID)
        {
            Console.WriteLine($"Disconnect Clinet ID: {SocketID}");

            // Lock을 안써도 상호 배제와 삭제 가능
            if (ClientSocketDictionary.TryRemove(SocketID, out ClientSocket ClientSocket)) // 이 부분을 아예 건너뜀 왜?
            {
                ClientSocket.Send(SocketID, "Q");
                ClientSocket.Close();

                ClientSocketList.Remove(ClientSocket);
            }
        }
    }
}