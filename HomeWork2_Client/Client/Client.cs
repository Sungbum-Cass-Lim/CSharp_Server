using System;
using System.Diagnostics.SymbolStore;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Server_Homework
{
    public class Client
    {
        private const int BUFFER_SIZE = 8;
        private const int READ_SIZE = 2;

        private int myId;
        private Socket mySocket;

        public async Task CreateSocket()
        {
            mySocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            await _ConnectAsync();
            return;
        }

        private async Task _ConnectAsync()
        {
            try
            {
                Console.WriteLine("State: Try Connect...");
                await mySocket.ConnectAsync(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 7000)); // 서버 연결 시도
            }
            catch (SocketException e)
            {
                Console.WriteLine(e);
                return;
            }

            Console.WriteLine("State: Success Connect!"); // 서버 연결 성공
            return;
        }

        public async Task Send(string msg)
        {
            Header header = new Header(msg.Length, myId, SendType.broadCast);
            Data data = new Data(msg);

            try
            {
                //확장 메서드
                await mySocket.SendAsync(header, data);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Exception -> {e}");
                _SocketDisconnect();
            }
        }

        private async Task _ReceiveLoopAsync()
        {
            Memory<byte> readBuffer = new Memory<byte>(new byte[BUFFER_SIZE]);

            int readOffset = 0;
            int totalRecv = 0;
            int recvByte = 0;

            while (true)
            {
                Header header = new Header();
                Data data = new Data();

                //Header Receive Loop
                while (false == header.TryDeserialize(readBuffer.Slice(readOffset, totalRecv)))
                {
                    //만약 다음 Receive할 공간이 ReadBuffer에 안남았다면
                    if(readBuffer.Length - READ_SIZE <= totalRecv)
                    {
                        readBuffer = readBuffer.MultiplyBufferSize(2);
                    }

                    //Receive받은 데이터의 크기 가중
                    recvByte = await mySocket.ReceiveAsync(readBuffer.Slice(totalRecv, READ_SIZE),
                        SocketFlags.None);
                    totalRecv += recvByte;
                }
                readOffset += Header.HeaderSize;

                //Data Receive Loop
                while(false == data.TryDeserialize(readBuffer.Slice(readOffset, totalRecv - readOffset), header.messageLength))
                {
                    //만약 다음 Receive할 공간이 ReadBuffer에 안남았다면
                    if (readBuffer.Length - READ_SIZE <= totalRecv)
                    {
                        readBuffer = readBuffer.MultiplyBufferSize(2);
                    }

                    //Receive받은 데이터의 크기 가중
                    recvByte = await mySocket.ReceiveAsync(readBuffer.Slice(totalRecv, READ_SIZE),
                        SocketFlags.None);
                    totalRecv += recvByte;
                }
                readOffset += header.messageLength;

                Console.WriteLine($"Recive:{header.ownerId} -> {data.message}");

                if (readBuffer.Span[readOffset] != 0)
                {

                    continue;
                }

                
            }
        }

        private void _SocketDisconnect()
        {
            mySocket.Shutdown(SocketShutdown.Both);
            mySocket.Close();
        }
    }
}
