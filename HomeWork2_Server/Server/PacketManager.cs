using System.Collections.Concurrent;

namespace Server_Homework
{
    public class PacketManager
    {
        private Server MainServer;
        private ConcurrentQueue<Packet> PacketQueue = new ConcurrentQueue<Packet>();

        public PacketManager(Server Serve)
        {
            MainServer = Serve;
        }

        public void AddPacket(Packet Packet)
        {
            PacketQueue.Enqueue(Packet);

            PacketProcess(); // TODO: 임시 메세지 처리 방법
        }

        public void PacketProcess()
        {
            Packet TargetPacket = null;

            if (PacketQueue.TryDequeue(out TargetPacket))
            {
                if(TargetPacket.GetMessage() == "Q" || TargetPacket.GetMessage() == "q") 
                {
                    MainServer.DisconnectScoket(TargetPacket.GetID()); // 소켓 접속 종료
                    return;
                }

                switch (TargetPacket.GetSendType())
                {
                    case SendType.BroadCast:
                        MainServer.Broadcast(TargetPacket.GetID(), TargetPacket.GetMessage());
                        return;

                    case SendType.MultiCast:
                        MainServer.Multicast(TargetPacket.GetID(), TargetPacket.GetMessage());
                        return;

                    case SendType.UniCast:
                        MainServer.Unicast(TargetPacket.GetID(), TargetPacket.GetMessage());
                        return;

                    default:
                        return;
                }
            }
        }
    }
}