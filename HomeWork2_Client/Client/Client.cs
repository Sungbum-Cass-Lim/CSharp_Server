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
        private Task ReceiveLoopTask;

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
            ReceiveLoopTask = ReceiveLoop();
            return;
        }

        public async Task Send(string Msg)
        {
            Packet SendPacket = new Packet();
            Header TcpHeader = new Header().Initialize(Msg.Length, MyId, SendType.BroadCast);
            Data TcpData = new Data().Initialize(Msg);

            if (!IsConnect)
                return;

            try
            {
                await MySocket.SendAsync(SendPacket.WritePacket(TcpHeader, TcpData), SocketFlags.None);
            }
            catch(Exception E) 
            {
                Console.WriteLine(E);
                return;
            }
        }

        public async Task ReceiveLoop()
        {
            Packet ReceivePacket = new Packet();
            Memory<byte> ReadBuffer = new Memory<byte>(new byte[BUFFER_SIZE]);

            int HeaderSize = Unsafe.SizeOf<Header>();
            int ReadOffset = 0;
            int TotalRecvByte = 0;
            int MaxReadByte = 32; 

            while (true)
            {
                //Receive받은 데이터의 크기 가중
                TotalRecvByte += await MySocket.ReceiveAsync(new Memory<byte>().Slice(TotalRecvByte, MaxReadByte), 
                    SocketFlags.None);

                //버퍼의 한계까지 데이터를 받았지만 HeaderSize보다 작을 때
                if (TotalRecvByte == ReadBuffer.Length && TotalRecvByte < HeaderSize)
                {
                    var NewBuffer = new Memory<byte>(new byte[ReadBuffer.Length * 2]);
                    ReadBuffer.CopyTo(NewBuffer);
                    ReadBuffer = NewBuffer;

                    continue;
                }
                
                //그냥 받은 데이터가 Header 사이즈보다 작을 때
                else if (TotalRecvByte < HeaderSize)
                    continue;

                //Header버퍼를 다 받아왔다면 해당 버퍼를 Header로 변환
                var HeaderBuffer = ReadBuffer.Slice(ReadOffset, HeaderSize);
                Header TcpHeader = ReceivePacket.ReadHeader(HeaderBuffer);
                ReadOffset += HeaderSize;

                //버퍼의 한계까지 데이터를 받았지만 MessageSize보다 작을 때
                if (TotalRecvByte == ReadBuffer.Length && TotalRecvByte - HeaderSize < TcpHeader.MessageLength)
                {
                    var NewBuffer = new Memory<byte>(new byte[ReadBuffer.Length * 2]);
                    ReadBuffer.CopyTo(NewBuffer);
                    ReadBuffer = NewBuffer;

                    continue;
                }

                //그냥 받은 데이터가 Message 사이즈보다 작을 때
                else if (TotalRecvByte - HeaderSize < TcpHeader.MessageLength)
                    continue;

                //Data버퍼를 다 받아왔다면 해당 버퍼를 Data로 변환
                var DataBuffer = ReadBuffer.Slice(ReadOffset, TcpHeader.MessageLength);
                Data TcpData = ReceivePacket.ReadData(DataBuffer, TcpHeader);
                ReadOffset += TcpHeader.MessageLength;

                //메세지가 출력되면 일단은 성공
                Console.WriteLine(TcpData.Message);

                //남은 데이터 Save버퍼에 보관하여 ReadBuffer로 복사
                var SaveBuffer = ReadBuffer.Slice(ReadOffset, ReadBuffer.Length);
                ReadBuffer = new Memory<byte>(new byte[SaveBuffer.Length]);
                SaveBuffer.CopyTo(ReadBuffer);

                //받은 데이터 Read버퍼 크기로 초기화, 읽은 위치 초기화
                TotalRecvByte = ReadBuffer.Length;
                ReadOffset = 0;
            }
        }
        /* TODO: 내일 코드 분할 할 때 사용
        public async Task<Header> ReceiveHeader()
        {

        }

        public async Task<Data> ReceiveData()
        {

        }
        */

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
