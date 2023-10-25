using System;
using System.Net.Sockets;
using System.Text;

namespace Server_Homework
{
    public class ClientSocket
    {
        private int MyId = default(int);
        private Socket MySocket = default(Socket);

        private byte[] Buffer = new byte[128];

        public ClientSocket Initialize(int Id, Socket socket)
        {
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

        public void SendOther(string Msg)
        {
            Program.MainServer.MultiCast(MyId, Msg);
        }
        public void SendAll(string Msg)
        {
            Program.MainServer.Broadcast(MyId, Msg);
        }
        #endregion

        #region Receive
        public void Receive(IAsyncResult Result)
        {
            Packet RecvPacket = new Packet();
            RecvPacket.Read(Buffer);

            Console.WriteLine($"ID:{RecvPacket.GetID()} -> Message:{RecvPacket.GetMessage()}"); // Send Message
            Send(MyId, RecvPacket.GetMessage());
            //SendOther(RecvPacket.GetMessage());

            if (RecvPacket.GetMessage() == "Q" || RecvPacket.GetMessage() == "q") // 접속 종료(오류 남)
                Disconnect();

            else
                MySocket.BeginReceive(Buffer, 0, RecvPacket.GetPacketLength(), SocketFlags.None, Receive, null);
        }
        #endregion

        public void Disconnect()
        {
            MySocket.Shutdown(SocketShutdown.Receive);
            MySocket.Close();

            Program.MainServer.RemoveSocket(this);
        }
    }
}