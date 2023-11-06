using System;
using System.Net.Sockets;
using System.Text;

namespace Server_Homework
{
    public class ClientSocket
    {
        private const int BUFFER_SIZE = 35;
        private Server MainServer = null;

        private int MyId;
        private Socket MySocket;
        private Task ReceiveTask;

        private byte[] DefultBuffer = new byte[BUFFER_SIZE];
        private byte[] SaveBuffer = new byte[BUFFER_SIZE];

        public ClientSocket Initialize(Server Server, int Id, Socket socket)
        {
            MainServer = Server;
            MyId = Id;
            MySocket = socket;

            ReceiveTask = ReceiveLoop();
            return this;
        }

        public int GetId()
        {
            return MyId;
        }

        #region Send
        public void Send(int Id, string Msg)
        {

        }
        #endregion

        #region ServerAsyncFunc
        public async Task ReceiveLoop()
        {

        }

        private async Task ReadBuffer(int ReadOffset)
        {
            //var memory = new Memory<byte>(new byte[1024]);

            //var received = 0;

            //while (true)
            //{
            //var buffer = memory.Slice(received, 5);
            //var bytes = await MySocket.ReceiveAsync(buffer, SocketFlags.None);
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

            int StartReadOffset = ReadOffset;
            int NextReadOffset = 0;

            Header PacketHeader = Converter.ByteToHeader(DefultBuffer, StartReadOffset);
            byte[] SliceBuffer = new Span<byte>(DefultBuffer).
                Slice(ReadOffset, PacketHeader.HeaderLength + PacketHeader.MessageLength).ToArray();

            Packet RecvPacket = new Packet();
            RecvPacket = RecvPacket.Read(SliceBuffer);

            MainServer.AddPacket(RecvPacket); // 받으면 Server에 있는 PacketQueue에 추가

            if (RecvPacket.GetMessage() == "Q" || RecvPacket.GetMessage() == "q") // 종료 메세지면 다시 받기 멈춤
                ReceiveLoopTask.Wait();

            NextReadOffset = ReadOffset + SliceBuffer.Length;

            Console.WriteLine($"New ReceiveMessage");
            if (DefultBuffer[NextReadOffset] != 0)
            {
                await ReadBuffer(NextReadOffset);
            }
        }
        #endregion

        public void Close()
        {
            MySocket.Shutdown(SocketShutdown.Receive);
            MySocket.Close();
        }
    }
}