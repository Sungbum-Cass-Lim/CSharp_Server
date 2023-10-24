using System;
using System.Net.Sockets;
using System.Text;

namespace Server_Homework
{
    public class ClientData
    {
        public int UserId = default(int);
        public Socket UserSocket = default(Socket);

        public byte[] Buffer = new byte[128];

        public ClientData(int Id, Socket Socket)
        {
            this.UserId = Id;
            this.UserSocket = Socket;
        }
    }
}