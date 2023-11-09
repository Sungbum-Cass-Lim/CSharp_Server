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

        private Dictionary<PayloadTag, Action<Memory<byte>>> PayloadCallback = new Dictionary<PayloadTag, Action<Memory<byte>>>();

        public async void CreateSocket()
        {
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

        public async Task Send<T>(Header header, T payload) where T : IMarshal
        {
            

            try
            {
                //확장 메서드
                await mySocket.SendAsync(header, data);
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

                while (Header.headerSize < totalRecvByte)
                {
                    Header header = new Header();

                    // 데이터가 깨진 상태에 대한 예외 처리 필요.
                    if (false == header.TryDeserialize(readBuffer.Slice(readOffset, totalRecvByte)))
                    {
                        break;
                    }

                    if (totalRecvByte < header.messageLength)
                    {
                        break;
                    }


                    // callback 내에서 throw 하는 경우는 ???
                    this.Callback(header.payloadTag, header.payloadLength);

                    readOffset += Header.headerSize + header.payloadLength;
                }
            }
        }

        private void _PayloadCallBack(PayloadTag tag, Memory<byte> buffer)
        {
            if (false == PayloadCallback.TryGetValue(tag, out var Callback))
            {
                throw new Exception();
            }

            Callback(buffer);
        }

        private void _MessageInfoProcess(Memory<byte> buffer, Header header)
        {
            MessageInfo.TryParseDataInfo(buffer, out var messageInfo);
        }

        private void _MessageProcess(Memory<byte> buffer, Header header)
        {
            Message.TryParseMessage(buffer, header.payloadLength, out var message);   
        }
    }
}
