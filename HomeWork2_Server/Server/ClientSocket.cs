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

        private Dictionary<int, Action<Header, Memory<byte>>> payloadCallback = new Dictionary<int, Action<Header, Memory<byte>>>();

        public ClientSocket Initialize(Server server, int id, Socket socket)
        {
            _AddCallBackProcess();

            mainServer = server;
            myId = id;
            mySocket = socket;
            myStringBuilder = new StringBuilder();

            _ReceiveLoopAsync();
            return this;
        }

        public int GetId()
        {
            return myId;
        }

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

        private async Task _ReceiveLoopAsync()
        {
            Memory<byte> readBuffer = new Memory<byte>(new byte[BUFFER_SIZE]);

            int readOffset = 0;
            int totalRecvByte = 0;

            while (true)
            {
                if (readBuffer.Length - READ_SIZE <= totalRecvByte)
                {
                    readBuffer = readBuffer.MultiplyBufferSize(2);
                }

                totalRecvByte += await mySocket.ReceiveAsync(readBuffer.Slice(totalRecvByte, READ_SIZE),
                        SocketFlags.None);

                while (readOffset + Header.headerLength < totalRecvByte)
                {
                    Header header = new Header();

                    // 데이터가 깨져서 역직렬화가 안된 상태(에외처리 필요)
                    if (false == header.TryDeserialize(readBuffer.Slice(readOffset, Header.headerLength)))
                    {
                        break;
                    }

                    // 아직 페이로드 크기만큼 데이터가 안들어온 상태(리턴)
                    if (totalRecvByte - Header.headerLength < header.payloadLength)
                    {
                        break;
                    }

                    // callback 내에서 throw 하는 경우는 ???
                    _PayloadCallBack(ref header, readBuffer.Slice(readOffset + Header.headerLength, header.payloadLength));

                    readOffset += Header.headerLength + header.payloadLength;
                }
            }
        }

        private void _PayloadCallBack(ref Header header, Memory<byte> buffer)
        {
            if (false == payloadCallback.TryGetValue(header.payloadTag, out var Callback))
            {
                throw new Exception();
            }

            Callback(header, buffer);
        }

        private void _MessageInfoProcess(Header header, Memory<byte> buffer)
        {
            MessageInfo msgInfo = new MessageInfo();
            if (false == msgInfo.TryDeserialize(header.payloadLength, buffer))
            {
                Console.WriteLine($"{msgInfo.GetType()} Deserialize Error");
                return;
            }

            mainServer.BroadCast(header, msgInfo);
            myStringBuilder.Append($"Receive:{msgInfo.userId}({(SendType)msgInfo.sendType}) -> ");
        }

        private void _MessageProcess(Header header, Memory<byte> buffer)
        {
            Message msg = new Message();
            if (false == msg.TryDeserialize(header.payloadLength, buffer))
            {
                Console.WriteLine($"{msg.GetType()} Deserialize Error");
                return;
            }

            mainServer.BroadCast(header, msg);

            myStringBuilder.Append(msg.message);
            Console.WriteLine(myStringBuilder.ToString());

            myStringBuilder.Clear();
        }

        private void _AddCallBackProcess()
        {
            try
            {
                payloadCallback.Add((int)PayloadTag.msgInfo, _MessageInfoProcess);
                payloadCallback.Add((int)PayloadTag.msg, _MessageProcess);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}