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

        private byte[] Buffer = new byte[128];
        private bool IsConnect = false;

        public void CreateSocket()
        {
            MySocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            Connect();
        }

        private void Connect()
        {
            Console.WriteLine("State: Try Connect...");

            MySocket.BeginConnect(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 7000), EndConnect, null);
        }

        private void EndConnect(IAsyncResult Result)
        {
            Console.WriteLine("State: Success Connect!");

            MySocket.BeginReceive(Buffer, 0, new Packet().GetPacketLength(), SocketFlags.None, Receive, null); // 비동기 Receive 시작
        }

        public void Send(string Msg)
        {
            Packet SendPacket = new Packet(1, 1, MyId, Msg);
            MySocket.Send(SendPacket.Write());
        }

        private void Receive(IAsyncResult Result)
        {
            Console.WriteLine("Receive");

            Packet RecvPacket = new Packet();
            RecvPacket.Read(Buffer);

            Console.WriteLine($"ID:{RecvPacket.GetID()} -> Message:{RecvPacket.GetMessage()}");
            if (IsConnect == false)
            {
                MyId = RecvPacket.GetID();
                IsConnect = true;
            }

            if (RecvPacket.GetMessage() == "Q" || RecvPacket.GetMessage() == "q") // 접속 종료
                Disconnect();

            else
                MySocket.BeginReceive(Buffer, 0, RecvPacket.GetPacketLength(), SocketFlags.None, Receive, null); // 비동기 Receive 시작
        }

        public void Disconnect()
        {
            MySocket.Shutdown(SocketShutdown.Receive);
            MySocket.Close();

            Console.WriteLine($"Disconnect Server");
        }
    }
}