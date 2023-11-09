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

        public ClientSocket Initialize(Server server, int id, Socket socket)
        {
            mainServer = server;
            myId = id;
            myState = SocketState.ESTABLISHED;
            mySocket = socket;

            _ReceiveLoopAsync();
            return this;
        }

        public int GetId()
        {
            return myId;
        }

        public async Task Send(int id, string msg)
        {
            Header header = new Header(msg.Length, id, SendType.broadCast);
            Data data = new Data(msg);

            try
            {
                //확장 메서드
                await mySocket.SendAsync(header, data);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Exception -> {e}");
                mySocket.SocketDisconnect();
            }
        }

        private async void _ReceiveLoopAsync()
        {
            Memory<byte> readBuffer = new Memory<byte>(new byte[BUFFER_SIZE]);

            int readOffset = 0;
            int totalRecvByte = 0;

            while (true)
            {
                if (readBuffer.Length - READ_SIZE <= totalRecvByte) {
                    readBuffer = readBuffer.MultiplyBufferSize(2);
                }

                totalRecvByte += await mySocket.ReceiveAsync(readBuffer.Slice(totalRecvByte, READ_SIZE),
                        SocketFlags.None);

                while (Header.HeaderSize < totalRecvByte) {
                    Header header = new Header();

                    // 데이터가 깨진 상태에 대한 예외 처리 필요.
                    if (false == header.TryDeserialize(readBuffer.Slice(readOffset, totalRecvByte))) {
                        break;
                    }

                    if (/*남은 바이트 수*/ < header.messageLength) {
                        break;
                    }


                    // callback 내에서 throw 하는 경우는 ???
                    this.Callback(header.messageId, header.messageLength 만큼의 메모리);



                    readOffset += Header.HeaderSize + header.messageLength;


                    //Console.WriteLine($"Recive:{header.ownerId} -> {data.message}");
                    //await mainServer.Broadcast(header.ownerId, data.message); // <-- Callback 내에서 처리해야 함.
                }
            }
        }
    }
}