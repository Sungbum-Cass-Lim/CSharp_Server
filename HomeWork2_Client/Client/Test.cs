using Server_Homework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Reflection.PortableExecutable;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    struct Header
    {
        //public readonly int headerLength = Unsafe.SizeOf<Header>();

        public int Tag;
        public int Length;

        public static Header Parse(Memory<byte> buffer)
        {
            Header header = new Header();

            /*
             * buffer 로 부터 header 를 parsing.
             */
            return header;
        }
    }


    internal class Test
    {
        private Dictionary<int, Action<Memory<byte>>> _callback = new Dictionary<int, Action<Memory<byte>>>();

        public Test()
        {
            this._callback.Add(1, Callback1);
            this._callback.Add(2, Callback2);
        }

        async Task Foo()
        {
            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            var buf = new Memory<byte>(new byte[1024]);
            var total_recv_bytes = 0;

            const int RECV_SIZE = 16;
            int headerLength = Unsafe.SizeOf<Header>();
            while (true)
            {
                if ((buf.Length - total_recv_bytes) < RECV_SIZE)
                {
                    var newBuf = new Memory<byte>(new byte[buf.Length * 2]);
                    buf.CopyTo(newBuf);
                    buf = newBuf;
                }

                var bytes = await socket.ReceiveAsync(buf.Slice(total_recv_bytes, RECV_SIZE), SocketFlags.None);
                total_recv_bytes += bytes;

                while (true)
                {
                    if (total_recv_bytes < headerLength)
                    {
                        continue;
                    }

                    var header = Header.Parse(buf.Slice(0, total_recv_bytes));

                    if (total_recv_bytes - headerLength < header.Length)
                    {
                        continue;
                    }

                    var payload = buf.Slice(headerLength, header.Length);
                    this.Callback(header.Tag, payload);
                }
            }
        }

        void Callback(int tag, Memory<byte> data)
        {
            if (false == this._callback.TryGetValue(tag, out var callback))
            {
                /*
                *  모르는 tag.. -> 에러처리가 필요
                */
                return;
            }

            callback(data);
        }

        void Callback1(Memory<byte> data)
        {
            // struct Data1
        }

        void Callback2(Memory<byte> data)
        {
            // struct Data2

        }

        //var memory = new Memory<byte>(new byte[1024]);

        //var received = 0;

        //while (true)
        //{
        //var buffer = memory.Slice(received, 5);
        //var bytes = await mySocket.ReceiveAsync(buffer, SocketFlags.None);
        //    if (received + bytes < Header.Length)
        //    {
        //        received += bytes;
        //        continue;
        //    }    
        //}
        /*
         * 1. 수신된 bytes 가 header 보다는 같거나, 길어야 함.
         * 2. header 를 제외한 수신된 데이터가 header 의 length 보다는 길어야 함.
         * 3. 적게 수신된 경우, 이어 받을 수 있도록 구현해야 함.
         * 
         * 추가: 소켓이 끊어졌을 때의 처리가 안되어 있음.
         */

        //static Header()
        //{
        //    headerLength = Unsafe.SizeOf<Header>();
        //}

        /*
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

                while (Header.headerLength < totalRecvByte) {
                    Header header = new Header();

                    // 데이터가 깨진 상태에 대한 예외 처리 필요.
                    if (false == header.TryDeserialize(readBuffer.Slice(readOffset, totalRecvByte))) {
                        break;
                    }

                    if (남은 바이트 수 < header.messageLength) {
                        break;
                    }


                    // callback 내에서 throw 하는 경우는 ???
                    this.Callback(header.messageId, header.messageLength 만큼의 메모리);



                    readOffset += Header.headerLength + header.messageLength;
            

                    //Console.WriteLine($"Recive:{header.ownerId} -> {data.message}");
                    //await mainServer.Broadcast(header.ownerId, data.message); // <-- Callback 내에서 처리해야 함.
                }
            }*/
    }
}
