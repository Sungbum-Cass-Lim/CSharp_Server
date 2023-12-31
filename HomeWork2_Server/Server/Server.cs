﻿using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Server_Homework
{
    public class Server
    {
        private Socket serverSocket;

        private int iDCount = 1;

        private List<ClientSocket> clientSocketList = new List<ClientSocket>();
        private ConcurrentDictionary<int, ClientSocket> clientSocketDictionary = new ConcurrentDictionary<int, ClientSocket>();

        public void Initialize()
        {
            Console.WriteLine("Server State: Start");

            serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            serverSocket.Bind(new IPEndPoint(IPAddress.Any, 7000));

            Console.WriteLine("Server State: Bind");
            Listen();
        }

        public void Listen()
        {
            Console.WriteLine("Server State: Listen");

            serverSocket.Listen(5);
            AcceptLoop(); // Accept Loop
        }

        public async void AcceptLoop()
        {
            while (true)
            {
                await _AcceptAsync();
                Console.WriteLine($"Server State: Accept Client Socket Number {iDCount - 1}");
            }
        }

        private async Task _AcceptAsync()
        {
            try
            {
                ClientSocket NewClientSocket = new ClientSocket(this, iDCount, await serverSocket.AcceptAsync());

                if (clientSocketDictionary.TryAdd(iDCount, NewClientSocket))
                {
                    var _ = _StartReceive(NewClientSocket);

                    Header header = new Header(InitData.initDataLength, (int)PayloadTag.initInfo);
                    InitData payload = new InitData(iDCount);

                    clientSocketList.Add(NewClientSocket); // ConcurrentDictionary를 통해 lock
                    await NewClientSocket.Send(header, payload); // 클라 첫 접속 메세지

                    iDCount++;
                }
            }
            catch (Exception e)
            {
                //Console.WriteLine(e.ToString());
                //throw;
                throw new Exception(e.Message);
            }
        }

        private async Task _StartReceive(ClientSocket socket)
        {
            try
            {
                await socket.ReceiveLoopAsync();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            finally
            {
                socket.Close();
                //clientSocketDictionary.Remove(socket);
            }
        }

        public async void BroadCast<T>(Header header, T payload) where T : IPayload
        {
            foreach(var clientSocket in clientSocketList)
            {
                await clientSocket.Send(header, payload);
            }
        }

        public void RemoveClientSocket(int socketId)
        {
            if(clientSocketDictionary.TryRemove(socketId, out var clientSocket))
            {
                clientSocketList.Remove(clientSocket);
            }
        }
    }
}