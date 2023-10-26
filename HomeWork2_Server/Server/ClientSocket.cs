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

        private byte[] Buffer = new byte[128];

        public ClientSocket Initialize(Server Server, int Id, Socket socket)
        {
            MainServer = Server;
            MyId = Id;
            MySocket = socket;

            MySocket.BeginReceive(Buffer, 0, new Packet().GetPacketLength(), SocketFlags.None, Receive, null);
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

            MySocket.Send(SendPakcet.Write());
        }
        #endregion

        #region Receive
        public void Receive(IAsyncResult Result)
        {
            Packet RecvPacket = new Packet();
            RecvPacket.Read(Buffer);

            MainServer.AddPacket(RecvPacket); // 받으면 Server에 있는 PacketQueue에 추가

            if(RecvPacket.GetMessage() != "Q" && RecvPacket.GetMessage() != "q") // 종료 메세지면 다시 받기 멈춤
                MySocket.BeginReceive(Buffer, 0, RecvPacket.GetPacketLength(), SocketFlags.None, Receive, null);
        }
        #endregion

        public void Close()
        {
            MySocket.Shutdown(SocketShutdown.Receive);
            MySocket.Close();
        }
    }
}