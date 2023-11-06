using System;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Server_Homework
{
    public class Client
    {
        private const int BUFFER_SIZE = 128;

        private int MyId;
        private Socket MySocket;
        private Task ReceiveTask;

        private bool IsConnect = false;

        public async Task CreateSocket()
        {
            MySocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            await Connect();
            return;
        }

        private async Task Connect()
        {
            Console.WriteLine("State: Try Connect...");
            await MySocket.ConnectAsync(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 7000)); // 서버 연결 시도

            Console.WriteLine("State: Success Connect!"); // 서버 연결 성공
            ReceiveTask = Receive();
            return;
        }

        public async Task Send(string Msg)
        {
            if (!IsConnect)
                return;

            try
            {
                Packet SendPacket = new Packet(MyId, Msg, SendType.BroadCast);
                await MySocket.SendAsync(SendPacket.WritePacket(), SocketFlags.None);
            }
            catch(Exception E) 
            {
                Console.WriteLine(E);
                return;
            }
        }

        public async Task Receive()
        {
            Memory<byte> ReadBuffer = new Memory<byte>(new byte[BUFFER_SIZE]);
            int TotalReceive = 0;

            while (true)
            {
                await MySocket.ReceiveAsync(new Memory<byte>(), SocketFlags.None);
            }
        }

        public void TryDisconnet(string Message)
        {
            // 종료 메세지면 다시 받기 멈춤
            if (Message == "Q" || Message == "q")
            {
                Disconnect();
                ReceiveTask.Wait();
            }
        }

        public void Disconnect()
        {
            Console.WriteLine($"Disconnect Server");

            MySocket.Shutdown(SocketShutdown.Receive);
            MySocket.Close();
        }
    }
}
