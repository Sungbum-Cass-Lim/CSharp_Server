using System;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;

namespace Server_Homework
{
    public class Client
    {
        private int MyId = default(int);
        private Socket MySocket = default(Socket);
        private Task ReceiveLoopTask;

        private byte[] Buffer = new byte[128];
        private bool IsConnect = false;

        public void CreateSocket()
        {
            MySocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            Connect();
        }

        private async void Connect()
        {
            Console.WriteLine("State: Try Connect...");
            await MySocket.ConnectAsync(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 7000)); // 서버 연결 시도

            Console.WriteLine("State: Success Connect!"); // 서버 연결 성공
            ReceiveLoopTask = ReceiveLoop();
        }

        public void Send(string Msg)
        {
            Packet SendPacket = new Packet(MyId, Msg, SendType.BroadCast);
            MySocket.Send(SendPacket.Write());
        }

        #region ServerAsyncFunc
        public async Task ReceiveLoop()
        {
            while (true)
            {
                try
                {
                    await ReceiveAsync();
                }
                catch(Exception E)
                {
                    Console.WriteLine(E);
                }
            }
        }

        private async Task ReceiveAsync()
        {
            Packet RecvPacket = new Packet();
            await MySocket.ReceiveAsync(Buffer, SocketFlags.None);

            RecvPacket = RecvPacket.Read(Buffer);

            if (IsConnect == false)
            {
                MyId = RecvPacket.GetID();
                IsConnect = true;
            }
            Console.WriteLine($"ID:{RecvPacket.GetID()} -> Message:{RecvPacket.GetMessage()}");

            if (RecvPacket.GetMessage() == "Q" || RecvPacket.GetMessage() == "q") // 종료 메세지면 다시 받기 멈춤
            {
                Disconnect();
                ReceiveLoopTask.Wait();
            }    
        }
        #endregion

        public void Disconnect()
        {
            Console.WriteLine($"Disconnect Server");

            MySocket.Shutdown(SocketShutdown.Receive);
            MySocket.Close();
        }
    }
}