using System;
using System.Net.Sockets;
using System.Text;

namespace Server_Homework
{
    public class ClientData
    {
        private int UserId = default(int);
        private Socket ClientSocket = default(Socket);

        public ClientData(int Id, Socket Socket)
        {
            this.UserId = Id;
            this.ClientSocket = Socket;
        }
    }
}