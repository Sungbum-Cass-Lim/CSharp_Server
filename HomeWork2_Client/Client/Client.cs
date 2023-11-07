using System;
using System.Diagnostics.SymbolStore;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Server_Homework
{
    //일단 클라가 먼저 접속, 종료를 보내는 가정
    public enum SocketState
    {
        NONE = 0,
        SYN_SENT,
        ESTABLISHED_NULL_ID,
        ESTABLISHED_ID,
        FIN_WAIT_1,
        FIN_WAIT_2,
        TIME_WAIT,
        CLOSED
    }

    public class Client
    {
        private const int BUFFER_SIZE = 8;
        private const int READ_SIZE = 2;

        private int myId;
        private SocketState myState = SocketState.NONE;
        private Socket mySocket;
        private Task _ReceiveLoopTask;

        public async Task CreateSocket()
        {
            mySocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            await _Connect();
            return;
        }

        private async Task _Connect()
        {
            Console.WriteLine("State: Try Connect...");
            _ChangeSocketState(SocketState.SYN_SENT);
            await mySocket.ConnectAsync(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 7000)); // 서버 연결 시도

            _ChangeSocketState(SocketState.ESTABLISHED_NULL_ID);
            Console.WriteLine("State: Success Connect!"); // 서버 연결 성공
            _ReceiveLoopTask = _ReceiveLoop();
            return;
        }

        public async Task Send(string msg)
        {
            //Socket 연결 안되있을 때 보내려 하면 리턴
            if (myState == SocketState.ESTABLISHED_NULL_ID)
                return;

            Packet sendPacket = new Packet();

            msg = $"Send:{myId} -> {msg}";

            Header tcpHeader = new Header().Initialize(msg.Length, myId, SendType.broadCast);
            Data tcpData = new Data().Initialize(msg);

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
            Memory<byte> readBuffer = new Memory<byte>(new byte[BUFFER_SIZE]);

            int readOffset = 0;
            int totalRecv = 0;

            while (true)
            {
                try
                {
                    //Receive받은 데이터의 크기 가중
                    totalRecv += await mySocket.ReceiveAsync(readBuffer.Slice(totalRecv, READ_SIZE),
                        SocketFlags.None);

                    //TODO: if 뺄 수 있나 해봐야함
                    if (_HeaderProcess(ref readBuffer, ref readOffset, totalRecv, out Header tcpHeader) == false)
                        continue;

                    if (myState == SocketState.ESTABLISHED_NULL_ID)
                    {
                        _ChangeSocketState(SocketState.ESTABLISHED_ID);
                        myId = tcpHeader.ownerId;
                    }

                    //TODO: 여기도 위 if문이랑 마찬가지 
                    if (_DataProcess(ref readBuffer, ref readOffset, totalRecv, tcpHeader) == false)
                        continue;

                    //패킷 하나를 처리 후 버퍼에 다음 패킷이 같이 들어왔을때
                    if (readBuffer.Span[readOffset] != 0)
                    {
                        var savebuffer = readBuffer.Slice(readOffset, totalRecv - readOffset);
                        int newBufferSize = (savebuffer.Length < BUFFER_SIZE ? BUFFER_SIZE : savebuffer.Length) * 2;
                        readBuffer = new Memory<byte>(new byte[newBufferSize]);
                        savebuffer.CopyTo(readBuffer);

                        totalRecv = savebuffer.Length;
                        readOffset = 0;

                        continue;
                    }

                    totalRecv = 0;
                    readOffset = 0;
                    readBuffer = new Memory<byte>(new byte[BUFFER_SIZE]);
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

        #region DisconnectFlow
        private void _SendDisconnectFin()
        {

        }
        
        private void _SendDisconnectAck()
        {
            mySocket.Shutdown(SocketShutdown.Send);
        }

        private void _ReceiveDisconnectFin()
        {
            mySocket.Shutdown(SocketShutdown.Receive);
        }

        private void _ReceiveDisconnectAck()
        {

        }

        private void _ChangeSocketState(SocketState state)
        {
            myState = state;
        }
        #endregion
    }
}
