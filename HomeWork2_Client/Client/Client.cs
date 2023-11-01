using System;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;

namespace Server_Homework
{
    public class Client
    {
        private const int BUFFER_SIZE = 1024;

        private int MyId;
        private Socket MySocket;
        private Task ReceiveLoopTask;

        private byte[] Buffer = new byte[BUFFER_SIZE];
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
                    await MySocket.ReceiveAsync(Buffer, SocketFlags.None);
                    await ReceiveAsync(0);
                }
                catch(Exception E)
                {
                    Console.WriteLine(E);
                }
                finally
                {
                    Buffer = new byte[BUFFER_SIZE];
                }
            }
        }

        private async Task ReceiveAsync(int ReadOffset)
        {
            int StartReadOffset = ReadOffset;
            int NextReadOffset = 0;

            Header PacketHeader = PacketConverter.ConvertByteToPacketHeader(Buffer, StartReadOffset);
            byte[] SliceBuffer = new Span<byte>(Buffer).
                Slice(ReadOffset, PacketHeader.HeaderLength + PacketHeader.MessageLength).ToArray();

            Packet RecvPacket = new Packet();
            RecvPacket = RecvPacket.Read(SliceBuffer);

            if (IsConnect == false)
            {
                MyId = RecvPacket.GetID();
                IsConnect = true;
            }
            Console.WriteLine($"ID:{RecvPacket.GetID()} -> Message:{RecvPacket.GetMessage()}");

            // 종료 메세지면 다시 받기 멈춤
            if (RecvPacket.GetMessage() == "Q" || RecvPacket.GetMessage() == "q") 
            {
                Disconnect();
                ReceiveLoopTask.Wait();
            }    

            NextReadOffset = SliceBuffer.Length;

            if (Buffer[NextReadOffset] != 0)
            { 
                await ReceiveAsync(NextReadOffset);
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