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

        public async Task Receive()
        {
            Packet ReceivePacket = new Packet();
            Memory<byte> ReadBuffer = new Memory<byte>(new byte[BUFFER_SIZE]);

            int HeaderSize = Unsafe.SizeOf<Header>();
            int TotalRecvByte = 0;
            int MaxReadByte = 32; 

            while (true)
            {
                TotalRecvByte = await MySocket.ReceiveAsync(new Memory<byte>().Slice(TotalRecvByte, MaxReadByte), 
                    SocketFlags.None);

                //버퍼의 한계까지 데이터를 받았지만 HeaderSize보다 작을 때
                if (TotalRecvByte == BUFFER_SIZE && TotalRecvByte < HeaderSize)
                {
                    var NewBuffer = new Memory<byte>(new byte[ReadBuffer.Length * 2]);
                    ReadBuffer.CopyTo(NewBuffer);
                    ReadBuffer = NewBuffer;

                    continue;
                }
                
                //그냥 받은 데이터가 헤더 사이즈보다 작을 때
                else if (TotalRecvByte < HeaderSize)
                    continue;
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
