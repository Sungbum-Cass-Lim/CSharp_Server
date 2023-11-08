using System;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;

namespace Server_Homework
{
    //일단 클라가 먼저 접속, 종료를 보내는 가정
    public enum SocketState
    {
        NONE = 0,
        ESTABLISHED,
        CLOSE_WAIT,
        LAST_ACK,
        CLOSED
    }

    public class ClientSocket
    {
        private const int BUFFER_SIZE = 8;
        private const int READ_SIZE = 2;

        private Server mainServer = null;

        private int myId;
        private SocketState myState = SocketState.NONE;
        private Socket mySocket;
        private Task receiveLoopTask;

        public ClientSocket Initialize(Server server, int id, Socket socket)
        {
            mainServer = server;
            myId = id;
            myState = SocketState.ESTABLISHED;
            mySocket = socket;

            receiveLoopTask = _ReceiveLoop();
            return this;
        }

        public int GetId()
        {
            return myId;
        }

        public async Task Send(int id, string msg)
        {
            mySocket.Shutdown(SocketShutdown.Both);
            mySocket.Close();

            Packet sendPacket = new Packet();

            Header tcpHeader = new Header().Initialize(msg.Length, id, SendType.broadCast);
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

                    if (_DataProcess(ref readbuffer, ref readOffset, totalRecv, tcpHeader) == false)
                        continue;

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
                //클라쪽에서 강제로 종료 했을 시 예외처리
                catch (SocketException se)
                {
                    Console.WriteLine("----------------------------------------------------------------");
                    Console.WriteLine($"Socket:{myId} -> {se.Message}");
                    Console.WriteLine($"Socket:{myId} -> Disconnect");
                    Console.WriteLine("----------------------------------------------------------------");

                    _SocketDisconnect();

                    return;
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

            //클라 ReceiveShotDown
            if (tcpData.message == "Q" || tcpData.message == "q")
            {
                mySocket.Shutdown(SocketShutdown.Receive);
                Task Send = mainServer.Unicast(tcpHeader.ownerId, tcpData.message);
            }

            else
            {
                Task Send = mainServer.Broadcast(tcpHeader.ownerId, tcpData.message);
            }

            return true;
        }

        private void _SocketDisconnect()
        {
            mySocket.Shutdown(SocketShutdown.Both);
            mySocket.Close();

            mainServer.RemoveClientSocketData(myId);

            receiveLoopTask.Wait();
        }
    }
}