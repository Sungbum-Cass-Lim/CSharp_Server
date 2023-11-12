using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;

namespace Server_Homework
{
    public class Client
    {
        private const int BUFFER_SIZE = 1024;
        private const int READ_SIZE = 128;

        private int myId;
        private Socket mySocket;
        private StringBuilder myStringBuilder;
        
        private Dictionary<int, Action<int, Memory<byte>>> payloadCallback = new Dictionary<int, Action<int, Memory<byte>>>();

        public async void CreateSocket()
        {
            _AddCallBackProcess();

            myStringBuilder = new StringBuilder();
            mySocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                await _ConnectAsync();
            }
            catch (SocketException se)
            {
                throw new Exception(se.Message);
            }
        }

        private async Task _ConnectAsync()
        {
            try
            {
                Console.WriteLine("State: Try Connect...");
                await mySocket.ConnectAsync(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 7000)); // 서버 연결 시도

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
                //info payload
                Header infoHeader = new Header(MessageInfo.msgInfoLength, (int)PayloadTag.msgInfo);
                MessageInfo msgInfo = new MessageInfo(myId, (int)SendType.broadCast);
                await mySocket.SendAsync(infoHeader, msgInfo);

                //msg payload
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

                    // 수신 데이터가 이상하여 역직렬화가 안된 상태(악성 패킷 || 패킷 깨짐)
                    if (false == header.TryDeserialize(readBuffer.Slice(readOffset, Header.headerLength)))
                    {
                        mySocket.SocketDisconnect();
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

            Callback(header.payloadLength, buffer);
        }

        private void _InitInfoProcess(int initInfoLength, Memory<byte> buffer)
        {
            InitData initInfo = new InitData();
            if (false == initInfo.TryDeserialize(initInfoLength, buffer))
            {
                Console.WriteLine($"{initInfo.GetType()} Deserialize Error");
            }

            myId = initInfo.myUserId;
            Console.WriteLine($"Welcome UserId {myId}");
        }

        private void _MessageInfoProcess(int msgInfoLength, Memory<byte> buffer)
        {
            MessageInfo msgInfo = new MessageInfo();
            if (false == msgInfo.TryDeserialize(msgInfoLength, buffer))
            {
                Console.WriteLine($"{msgInfo.GetType()} Deserialize Error");
            }

            myStringBuilder.Clear();

            myStringBuilder.Append($"Receive:{msgInfo.userId}({msgInfo.sendType.ToString()}) -> ");
        }

        private void _MessageProcess(int msgLength, Memory<byte> buffer)
        {
            Message msg = new Message();
            if (false == msg.TryDeserialize(msgLength, buffer))
            {
                Console.WriteLine($"{msg.GetType()} Deserialize Error");
            }

            myStringBuilder.Append(msg.message);
            Console.WriteLine(myStringBuilder.ToString());

            myStringBuilder.Clear();
        }   

        private void _AddCallBackProcess()
        {
            try
            {
                payloadCallback.Add((int)PayloadTag.initInfo, _InitInfoProcess);
                payloadCallback.Add((int)PayloadTag.msgInfo, _MessageInfoProcess);
                payloadCallback.Add((int)PayloadTag.msg, _MessageProcess);
            }
            catch(Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }

}