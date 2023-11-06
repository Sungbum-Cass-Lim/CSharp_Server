using System;
using System.Net.Sockets;
using System.Text;

namespace Server_Homework
{
    public class ClientSocket
    {
        private const int BUFFER_SIZE = 128;
        private Server MainServer = null;

        private int MyId;
        private Socket MySocket;
        private Task ReceiveTask;

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

        public async Task Send(int Id, string Msg)
        {
            Packet SendPacket = new Packet();
            Header TcpHeader = new Header().Initialize(Msg.Length, Id, SendType.BroadCast);
            Data TcpData = new Data().Initialize(Msg);

            try
            {
                await MySocket.SendAsync(SendPacket.WritePacket(TcpHeader, TcpData), SocketFlags.None);
            }
            catch (Exception E)
            {
                Console.WriteLine(E);
                return;
            }
        }

        public async Task ReceiveLoop()
        {

        }

        public void Close()
        {
            MySocket.Shutdown(SocketShutdown.Receive);
            MySocket.Close();
        }
    }
}