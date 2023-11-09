using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;

namespace Server_Homework
{
    public class Server
    {
        private Socket serverSocket;

        private int iDCount = 1;

        private List<ClientSocket> clientSocketList = new List<ClientSocket>();
        private ConcurrentDictionary<int, ClientSocket> clientSocketDictionary = new ConcurrentDictionary<int, ClientSocket>();

        #region Server Start
        public void Initialize()
        {
            AppDomain.CurrentDomain.ProcessExit += new EventHandler(_ProcessCloseEvent);
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
        #endregion

        #region ServerAsyncFunc
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
                ClientSocket NewClientSocket;
                NewClientSocket = new ClientSocket().Initialize(this, iDCount, await serverSocket.AcceptAsync());

                if (clientSocketDictionary.TryAdd(iDCount, NewClientSocket))
                {
                    clientSocketList.Add(NewClientSocket); // ConcurrentDictionary를 통해 lock
                    await NewClientSocket.Send(iDCount, "Welcome Bro"); // 클라 첫 접속 메세지

                    iDCount++;
                }
            }
            catch(Exception e)
            {
                throw new Exception(e.Message);
            }
        }
        #endregion

        public async Task Unicast(int id, string msg) // 지정 전송
        {
            await clientSocketDictionary[id].Send(id, msg);
        }

        public async Task Broadcast(int id, string msg) // 모두 전송
        {
            foreach (ClientSocket ClientSocket in clientSocketList)
            {
                await ClientSocket.Send(id, msg);
            }
        }

        public async Task Multicast(int id, string msg) // 해당 id 빼고 모두 전송
        {
            foreach (ClientSocket ClientSocket in clientSocketList)
            {
                if (ClientSocket.GetId() != id)
                {
                    await ClientSocket.Send(id, msg);
                }
            }
        }

        public void RemoveClientSocketData(int socketId)
        {
            if(clientSocketDictionary.TryRemove(socketId, out var clientSocket))
            {
                clientSocketList.Remove(clientSocket);
            }
        }

        private async void _ProcessCloseEvent(object? sender, EventArgs e)
        {
            await Broadcast(9999, "Q");
        }
    }
}