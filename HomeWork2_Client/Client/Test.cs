using Server_Homework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    struct Header
    {
        //public readonly int headerSize = Unsafe.SizeOf<Header>();

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
            int headerSize = Unsafe.SizeOf<Header>();
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
                    if (total_recv_bytes < headerSize) {
                        continue;
                    }

                    var header = Header.Parse(buf.Slice(0, total_recv_bytes));

                    if (total_recv_bytes - headerSize < header.Length)
                    {
                        continue;
                    }

                    var payload = buf.Slice(headerSize, header.Length);
                    this.Callback(header.Tag, payload);
                }
            }
        }

        void Callback(int tag, Memory<byte> data)
        {
            if (false == this._callback.TryGetValue(tag, out var callback)) {
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
    }
}
