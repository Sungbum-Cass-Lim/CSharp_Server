using System.Collections.Concurrent;

namespace Server_Homework
{
    public class PacketProcessor
    {
        private Server MainServer;
        private ConcurrentDictionary<SendType, Memory<byte>> PacketDictionay = new ConcurrentDictionary<SendType, Memory<byte>>();
        private ConcurrentDictionary<SendType, Action<Memory<byte>>> CallBackDictionry = new ConcurrentDictionary<SendType, Action<Memory<byte>>>();

        public PacketProcessor(Server Serve)
        {
            MainServer = Serve;
        }

        public void AddPacket(Packet Packet)
        {

        }

        public void PacketProcess()
        {

        }

        public async Task Unicast(int Id, string Msg) // 지정 전송
        {
            ClientSocketDictionary[Id].Send(Id, Msg);
        }

        public async Task Broadcast(int Id, string Msg) // 모두 전송
        {
            foreach (ClientSocket ClientSocket in ClientSocketList)
            {
                await ClientSocket.Send(Id, Msg);
            }
        }

        public async Task Multicast(int Id, string Msg) // 해당 Id 빼고 모두 전송
        {
            foreach (ClientSocket ClientSocket in ClientSocketList)
            {
                if (ClientSocket.GetId() != Id)
                {
                    await ClientSocket.Send(Id, Msg);
                }
            }
        }
    }
}