using System;
using System.Net.Sockets;
using System.Text;

namespace Server_Homework
{
    public class ClientSocket
    {
        private Server? MainServer = null;

        private int MyId = default(int);
        private Socket? MySocket = default(Socket);
        private Task? ReceiveLoopTask;

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
            Packet SendPakcet = new Packet(1, 1, Id, Msg);

            MySocket!.Send(SendPakcet.Write());
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
            await MySocket!.ReceiveAsync(Buffer, SocketFlags.None);

            RecvPacket.Read(Buffer);
            MainServer!.AddPacket(RecvPacket); // 받으면 Server에 있는 PacketQueue에 추가

            if (RecvPacket.GetMessage() == "Q" || RecvPacket.GetMessage() == "q") // 종료 메세지면 다시 받기 멈춤
                ReceiveLoopTask!.Wait();
        }
        #endregion

        public void Close()
        {
            MySocket!.Shutdown(SocketShutdown.Receive);
            MySocket!.Close();
        }
    }
}