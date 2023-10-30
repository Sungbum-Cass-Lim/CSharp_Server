using System;
using System.Net.Sockets;
using System.Text;

namespace Server_Homework
{
    public class ClientSocket
    {
        private Server MainServer = null;

        private int MyId = default(int);
        private Socket MySocket = default(Socket);
        private Task ReceiveLoopTask;

        private byte[] Buffer = new byte[128];

        public ClientSocket Initialize(Server Server, int Id, Socket socket)
        {
            MainServer = Server;
            MyId = Id;
            MySocket = socket;

            ReceiveLoopTask = ReceiveLoop();
            return this;
        }

        public int GetId()
        {
            return MyId;
        }

        #region Send
        public void Send(int Id, string Msg)
        {
            Packet SendPakcet = new Packet(Id, Msg);

            MySocket.Send(SendPakcet.Write());
        }
        #endregion

        #region ServerAsyncFunc
        public async Task ReceiveLoop()
        {
            while (true)
            {
                await ReceiveAsync();
                Console.WriteLine($"New ReceiveMessage");
            }
        }

        private async Task ReceiveAsync()
        {
            Packet RecvPacket = new Packet();
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
             * 2.  header 를 제외한 수신된 데이터가 header 의 length 보다는 길어야 함.
             * 3. 적게 수신된 경우, 이어 받을 수 있도록 구현해야 함.
             * 
             * 추가: 소켓이 끊어졌을 때의 처리가 안되어 있음.
             */

            byte[] buffer = new byte[1024];

            await MySocket.ReceiveAsync(buffer, SocketFlags.None);

            RecvPacket = RecvPacket.Read(Buffer);
            MainServer.AddPacket(RecvPacket); // 받으면 Server에 있는 PacketQueue에 추가

            if (RecvPacket.GetMessage() == "Q" || RecvPacket.GetMessage() == "q") // 종료 메세지면 다시 받기 멈춤
                ReceiveLoopTask.Wait();
        }
        #endregion

        public void Close()
        {
            MySocket.Shutdown(SocketShutdown.Receive);
            MySocket.Close();
        }
    }
}