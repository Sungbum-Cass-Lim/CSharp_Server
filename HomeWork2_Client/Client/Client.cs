using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Runtime;
using System.Threading.Channels;

namespace Server_Homework
{
    public class Client
    {
        private const int BUFFER_SIZE = 1024;
        private const int READ_SIZE = 128;

        private int myId;
        private Socket mySocket;
        private StringBuilder myStringBuilder;
        private bool isConnect = false;

        private Dictionary<PayloadTag, Action<Header, Memory<byte>>> payloadCallback = new Dictionary<PayloadTag, Action<Header, Memory<byte>>>();

        public int GetId() => myId;

        public Client()
        {
            try
            {
                payloadCallback.Add(PayloadTag.initInfo, _InitInfoProcess);
                payloadCallback.Add(PayloadTag.msgInfo, _MessageInfoProcess);
                payloadCallback.Add(PayloadTag.msg, _MessageProcess);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

            myStringBuilder = new StringBuilder();
            mySocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }

        public async void ConnectAsync()
        {
            try
            {
                Console.WriteLine("State: Try Connect...");
                await mySocket.ConnectAsync(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 7000)); // 서버 연결 시도

                isConnect = true;

                Console.WriteLine("State: Success Connect!"); // 서버 연결 성공
                await _ReceiveLoopAsync();
            }
            catch (SocketException se)
            {
                throw new Exception(se.Message);
            }
        }

        public async Task Send<T>(Header header, T payload) where T : IPayload
        {
            try
            {
                //확장 메서드
                await mySocket.SendAsync(header, payload);
            }
            catch (ObjectDisposedException e) { }
            catch (SocketException e) { }
            catch (Exception e)
            {
                Console.WriteLine(e);
                SocketDisconnect();
            }
        }


        private async Task _ReceiveLoopAsync()
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

                if (recvByte <= 0 && isConnect == true)
                {
                    SocketDisconnect();
                    break;
                }
                totalRecvByte += recvByte;

                while (readOffset + Header.headerLength < totalRecvByte)
                {
                    Header header = new Header();

                    // 수신 데이터가 이상하여 역직렬화가 안된 상태(큰 악성 패킷 || 패킷 깨짐)
                    if (false == header.TryDeserialize(readBuffer.Slice(readOffset, Header.headerLength)))
                    {
                        Console.WriteLine("Header Deserialize Error");
                        SocketDisconnect();

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
            //Tag에 대한 CallBack함수가 null인 상황
            if (false == payloadCallback.TryGetValue(header.payloadTag, out var Callback))
            {
                Console.WriteLine($"NullExeption:{header.payloadTag}TagMatchMethod");
                SocketDisconnect();

                return;
            }

            Callback(header, buffer);
        }

        private void _InitInfoProcess(Header header, Memory<byte> buffer)
        {
            InitData initInfo = new InitData();
            if (false == initInfo.TryDeserialize(header.payloadLength, buffer))
            {
                Console.WriteLine("InitInfo Deserialize Error");
                SocketDisconnect();

                return;
            }

            myId = initInfo.myUserId;

            Console.WriteLine($"Welcome UserId {myId}");
        }

        private void _MessageInfoProcess(Header header, Memory<byte> buffer)
        {
            MessageInfo msgInfo = new MessageInfo();
            if (false == msgInfo.TryDeserialize(header.payloadLength, buffer))
            {
                Console.WriteLine("MessageInfo Deserialize Error");
                SocketDisconnect();

                return;
            }

            myStringBuilder.Clear();
            myStringBuilder.Append($"Receive:{msgInfo.userId}({msgInfo.sendType.ToString()}) -> ");
        }

        private void _MessageProcess(Header header, Memory<byte> buffer)
        {
            Message msg = new Message();
            if (false == msg.TryDeserialize(header.payloadLength, buffer))
            {
                Console.WriteLine("Message Deserialize Error");
                SocketDisconnect();

                return;
            }

            myStringBuilder.Append(msg.message);
            Console.WriteLine(myStringBuilder.ToString());

            myStringBuilder.Clear();
        }

        public void SocketDisconnect()
        {
            isConnect = false;

            Console.WriteLine("Socket Disconnect..");
            mySocket.Shutdown(SocketShutdown.Both);
            mySocket.Close();
        }
    }
}