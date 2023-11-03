using System;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;

namespace Server_Homework
{
    public class Client
    {
        private const int BUFFER_SIZE = 35;

        private int MyId;
        private Socket MySocket;
        private Task ReceiveLoopTask;

        private byte[] ReadBuffer = new byte[BUFFER_SIZE * 2];
        private byte[] ReceiveBuffer = new byte[BUFFER_SIZE];
        private byte[] SaveBuffer = new byte[BUFFER_SIZE];

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
            int StartReadOffset = 0;

            while (true)
            {
                if (ReadBuffer[StartReadOffset] == 0) // 버퍼 초기화 단계
                {
                    ReadBuffer = new byte[BUFFER_SIZE * 2];
                    ReceiveBuffer = new byte[BUFFER_SIZE];

                    await MySocket.ReceiveAsync(ReceiveBuffer, SocketFlags.None);

                    //첫번째 배열에 ID가 있을 때
                    if (SaveBuffer[0] != 0)
                    {
                        Buffer.BlockCopy(SaveBuffer, 0, ReadBuffer, 0, BUFFER_SIZE);
                        Buffer.BlockCopy(ReceiveBuffer, 0, ReadBuffer, BUFFER_SIZE - StartReadOffset, BUFFER_SIZE);
                    }

                    //Save버퍼에 값이 없을 때(완전히 나누어 떨어졌을 때)
                    else
                    {
                        Buffer.BlockCopy(ReceiveBuffer, 0, ReadBuffer, 0, BUFFER_SIZE);
                    }

                    StartReadOffset = 0;
                    SaveBuffer = new byte[BUFFER_SIZE];
                }

                try // 버퍼 읽기 단계
                {
                    StartReadOffset = ReadBufferAsync(StartReadOffset);
                }
                catch (Exception E) // 오류 캐치 단계
                {
                    Console.WriteLine(E);
                }
            }
        }

        private unsafe int ReadBufferAsync(int ReadOffset)
        {
            int AddOffset = 0;
            int StartReadOffset = ReadOffset;

            // 헤더 크기보단 남은 버퍼가 컸지만, 메세지가 짤렸을 때
            Header PacketHeader = PacketConverter.ConvertByteToPacketHeader(ReadBuffer, StartReadOffset);
            if (PacketHeader.MessageLength > BUFFER_SIZE - (PacketHeader.HeaderLength + StartReadOffset))
            {
                Buffer.BlockCopy(ReceiveBuffer, StartReadOffset, SaveBuffer, 0, BUFFER_SIZE - StartReadOffset);
                ReadBuffer = new byte[BUFFER_SIZE * 2];
                ReceiveBuffer = new byte[BUFFER_SIZE];

                //예외 처리
                return ReadOffset;
            }

            else
            {
                byte[] SliceBuffer = new Span<byte>(ReadBuffer).
                    Slice(ReadOffset, PacketHeader.HeaderLength + PacketHeader.MessageLength).ToArray();

                AddOffset = SliceBuffer.Length;

                Packet RecvPacket = new Packet();
                RecvPacket = RecvPacket.Read(SliceBuffer);

                if (IsConnect == false)
                {
                    MyId = RecvPacket.GetID();
                    IsConnect = true;
                }
                Console.WriteLine($"ID:{RecvPacket.GetID()} -> Message:{RecvPacket.GetMessage()}");
                TryDisconnet(RecvPacket.GetMessage());

                //예외 처리
                return ReadOffset + AddOffset;
            }
        }
        #endregion

        public void TryDisconnet(string Message)
        {
            // 종료 메세지면 다시 받기 멈춤
            if (Message == "Q" || Message == "q")
            {
                Disconnect();
                ReceiveLoopTask.Wait();
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