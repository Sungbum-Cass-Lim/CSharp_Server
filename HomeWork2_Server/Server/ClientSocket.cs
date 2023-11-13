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
        private const int BUFFER_SIZE = 1024;
        private const int READ_SIZE = 128;

        private Server mainServer = null;

        private int myId;
        private Socket mySocket;
        private StringBuilder myStringBuilder;
        private bool isConnect = false;

        private Dictionary<PayloadTag, Action<Header, Memory<byte>>> payloadCallback = new Dictionary<PayloadTag, Action<Header, Memory<byte>>>();

        public ClientSocket(Server server, int id, Socket socket)
        {
            try
            {
                payloadCallback.Add(PayloadTag.msgInfo, _MessageInfoProcess);
                payloadCallback.Add(PayloadTag.msg, _MessageProcess);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

            mainServer = server;
            myId = id;
            isConnect = true;
            mySocket = socket;
            myStringBuilder = new StringBuilder();

            _ReceiveLoopAsync();
        }

        public int GetId() => myId;

        public async Task Send<T>(Header header, T payload) where T : IPayload
        {
            try
            {
                //확장 메서드
                await mySocket.SendAsync(header, payload);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                mySocket.SocketDisconnect();
            }
        }

        private async void _ReceiveLoopAsync()
        {
            Memory<byte> readBuffer = new Memory<byte>(new byte[BUFFER_SIZE]);

            int readOffset = 0;
            int totalRecvByte = 0;

            while (isConnect == true)
            {
                if (readBuffer.Length - READ_SIZE <= totalRecvByte)
                {
                    readBuffer = readBuffer.MultiplyBufferSize(2);
                }

                int recvByte = await mySocket.ReceiveAsync(readBuffer.Slice(totalRecvByte, READ_SIZE),
                        SocketFlags.None);

                if(recvByte <= 0) 
                {
                    _SocketDisconnect();
                    break;
                }
                totalRecvByte += recvByte;

                #region LogCode
                Console.WriteLine("----------------------------------------------------------------------");
                Console.WriteLine($"ReadOffset {readOffset}");
                Console.WriteLine($"RecvByte {recvByte}");
                Console.WriteLine($"TotalByte {totalRecvByte}");
                Console.WriteLine("----------------------------------------------------------------------");
                #endregion

                while (readOffset + Header.headerLength < totalRecvByte)
                {
                    Header header = new Header();

                    // 수신 데이터가 이상하여 역직렬화가 안된 상태(큰 악성 패킷 || 패킷 깨짐)
                    if (false == header.TryDeserialize(readBuffer.Slice(readOffset, Header.headerLength)))
                    {
                        Console.WriteLine("Header Deserialize Error");
                        _SocketDisconnect();

                        break;
                    }

                    // 아직 페이로드 크기만큼 데이터가 안들어온 상태(리턴)
                    if (totalRecvByte - (Header.headerLength + readOffset) < header.payloadLength)
                    {
                        break;
                    }

                    _PayloadCallBack(header, readBuffer.Slice(readOffset + Header.headerLength, header.payloadLength));

                    readOffset += Header.headerLength + header.payloadLength;
                }
            }
        }

        private void _PayloadCallBack(Header header, Memory<byte> buffer)
        {
            if (false == payloadCallback.TryGetValue(header.payloadTag, out var Callback))
            {
                Console.WriteLine($"NullExeption:{header.payloadTag}TagMatchMethod");
                _SocketDisconnect();

                return;
            }

            Callback(header, buffer);
        }

        private void _MessageInfoProcess(Header header, Memory<byte> buffer)
        {
            MessageInfo msgInfo = new MessageInfo();
            if (false == msgInfo.TryDeserialize(header.payloadLength, buffer))
            {
                Console.WriteLine($"{msgInfo.GetType()} Deserialize Error");
                _SocketDisconnect();

                return;
            }

            mainServer.BroadCast(header, msgInfo);
            myStringBuilder.Append($"Receive:{msgInfo.userId}({msgInfo.sendType}) -> ");
        }

        private void _MessageProcess(Header header, Memory<byte> buffer)
        {
            Message msg = new Message();
            if (false == msg.TryDeserialize(header.payloadLength, buffer))
            {
                Console.WriteLine($"{msg.GetType()} Deserialize Error");
                _SocketDisconnect();

                return;
            }

            mainServer.BroadCast(header, msg);

            myStringBuilder.Append(msg.message);
            Console.WriteLine(myStringBuilder.ToString());

            myStringBuilder.Clear();
        }
        private void _SocketDisconnect()
        {
            isConnect = false;

            Console.WriteLine($"Socket{myId} Disconnect..");
            mySocket.Shutdown(SocketShutdown.Both);
            mySocket.Close();

            mainServer.RemoveClientSocket(myId);
        }
    }
}