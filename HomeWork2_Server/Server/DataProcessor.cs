using System.Collections.Concurrent;

namespace Server_Homework
{
    public class DataProcessor
    {
        private Server mainServer;
        private Task PacketProcessLoopTask;

        private ConcurrentDictionary<SendType, Data> PacketDictionary = new ConcurrentDictionary<SendType, Data>();
        private ConcurrentDictionary<SendType, Action<Data>> CallBackDictionary = new ConcurrentDictionary<SendType, Action<Data>>();

        public DataProcessor(Server Serve)
        {
            mainServer = Serve;
            PacketProcessLoopTask = PacketProcessLoop();
        }

        public void AddPacket(SendType Type, Data Data)
        {
            PacketDictionary.TryAdd(Type, Data);
        }

        public async Task PacketProcessLoop()
        {
            
        }
    }
}