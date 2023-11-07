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
        private Task _ReceiveLoopTask;

        private bool isConnect = false;

        public async Task CreateSocket()
        {
            mySocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            await _Connect();
            return;
        }

        private async Task _Connect()
        {
            Console.WriteLine("State: Try Connect...");
            await mySocket.ConnectAsync(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 7000)); // 서버 연결 시도

            Console.WriteLine("State: Success Connect!"); // 서버 연결 성공
            _ReceiveLoopTask = _ReceiveLoop();
            return;
        }

        public async Task Send(string msg)
        {
            Packet sendPacket = new Packet();

            msg = $"Send:{myId} -> {msg}";

            Header tcpHeader = new Header().Initialize(msg.Length, myId, SendType.broadCast);
            Data tcpData = new Data().Initialize(msg);

            if (!isConnect)
                return;

            try
            {
                await mySocket.SendAsync(sendPacket.WritePacket(tcpHeader, tcpData), SocketFlags.None);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return;
            }
        }

        private async Task _ReceiveLoop()
        {
            Memory<byte> readbuffer = new Memory<byte>(new byte[BUFFER_SIZE]);

            int readOffset = 0;
            int totalRecv = 0;

            while (true)
            {
                try
                {
                    //Receive받은 데이터의 크기 가중
                    totalRecv += await mySocket.ReceiveAsync(readbuffer.Slice(totalRecv, READ_SIZE),
                        SocketFlags.None);

                    if (_HeaderProcess(ref readbuffer, ref readOffset, totalRecv, out Header tcpHeader) == false)
                        continue;

                    if (isConnect == false)
                    {
                        isConnect = true;
                        myId = tcpHeader.ownerId;
                    }

                    if (_DataProcess(ref readbuffer, ref readOffset, totalRecv, tcpHeader) == false)
                        continue;



                    //패킷 하나를 처리 후 버퍼에 다음 패킷이 같이 들어왔을때
                    if (readbuffer.Span[readOffset] != 0)
                    {
                        var savebuffer = readbuffer.Slice(readOffset, totalRecv - readOffset);
                        int newBufferSize = (savebuffer.Length < BUFFER_SIZE ? BUFFER_SIZE : savebuffer.Length) * 2;
                        readbuffer = new Memory<byte>(new byte[newBufferSize]);
                        savebuffer.CopyTo(readbuffer);

                        totalRecv = savebuffer.Length;
                        readOffset = 0;

                        continue;
                    }

                    totalRecv = 0;
                    readOffset = 0;
                    readbuffer = new Memory<byte>(new byte[BUFFER_SIZE]);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }

        private bool _HeaderProcess(ref Memory<byte> buffer, ref int readOffset, int totalRecv, out Header tcpHeader)
        {
            Packet receivePacket = new Packet();

            tcpHeader = new Header();
            int headerSize = Unsafe.SizeOf<Header>();

            //버퍼의 한계까지 데이터를 받았지만 headerSize보다 작을 때
            if (totalRecv >= buffer.Length - READ_SIZE && totalRecv < headerSize)
            {
                var newBuffer = new Memory<byte>(new byte[buffer.Length * 2]);
                buffer.CopyTo(newBuffer);
                buffer = newBuffer;

                return false;
            }

            //그냥 받은 데이터가 Header 사이즈보다 작을 때
            else if (totalRecv < headerSize)
            {
                return false;
            }

            //Header버퍼를 다 받아왔다면 해당 버퍼를 Header로 변환
            var headerBuffer = buffer.Slice(0, headerSize);
            tcpHeader = receivePacket.ReadHeader(headerBuffer);
            readOffset = headerSize;

            return true;
        }

        private bool _DataProcess(ref Memory<byte> buffer, ref int readOffset, int totalRecv, Header tcpHeader)
        {
            Packet receivePacket = new Packet();

            int headerSize = Unsafe.SizeOf<Header>();

            //버퍼의 한계까지 데이터를 받았지만 MessageSize보다 작을 때
            if (totalRecv >= buffer.Length - READ_SIZE && totalRecv - headerSize < tcpHeader.messageLength)
            {
                var newBuffer = new Memory<byte>(new byte[buffer.Length * 2]);
                buffer.CopyTo(newBuffer);
                buffer = newBuffer;

                return false;
            }

            //그냥 받은 데이터가 message 사이즈보다 작을 때
            else if (totalRecv - headerSize < tcpHeader.messageLength)
                return false;

            //Data버퍼를 다 받아왔다면 해당 버퍼를 Data로 변환
            var dataBuffer = buffer.Slice(readOffset, tcpHeader.messageLength);
            Data tcpData = receivePacket.ReadData(dataBuffer);
            readOffset += tcpHeader.messageLength;

            //메세지가 출력되면 일단은 성공
            Console.WriteLine(tcpData.message);
            return true;
        }

        private void TryDisconnet(string message)
        {
            // 종료 메세지면 다시 받기 멈춤
            if (message == "Q" || message == "q")
            {
                _Disconnect();
                _ReceiveLoopTask.Wait();
            }
        }

        private void _Disconnect()
        {
            Console.WriteLine($"DisConnect Server");

            mySocket.Shutdown(SocketShutdown.Receive);
            mySocket.Close();
        }
    }
}
