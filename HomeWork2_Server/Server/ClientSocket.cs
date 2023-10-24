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

            return this;
        }

        #region Send
        public void Send(string Msg)
        {
            Packet SendPakcet = new Packet(1, 1, MyId, Msg);

            MySocket.Send(SendPakcet.Write());
        }
        public void SendOther(string Msg)
        {

        }
        public void SendAll(string Msg)
        {

        }
        #endregion

        #region Receive
        public void BeginReceive()
        {
            MySocket.BeginReceive(Buffer, 0, new Packet().Pkt.PacketLength, SocketFlags.None, Receive, null);
        }

        public void Receive(IAsyncResult Result)
        {
            Packet RecvPacket = new Packet();
            RecvPacket.Read(Buffer);

            Console.WriteLine($"ID:{RecvPacket.Pkt.Id} -> Message:{RecvPacket.Message}"); // Send Message
            Send(RecvPacket.Message);

            if (RecvPacket.Message == "Q" || RecvPacket.Message == "q") // 접속 종료(오류 남)
                Disconnect();

            MySocket.BeginReceive(Buffer, 0, RecvPacket.Pkt.PacketLength, SocketFlags.None, Receive, null);
        }
        #endregion

        public void Disconnect()
        {
            MySocket.Shutdown(SocketShutdown.Receive);
            MySocket.Close();

            Program.MainServer.RemoveSocket(MyId);
        }
    }
}