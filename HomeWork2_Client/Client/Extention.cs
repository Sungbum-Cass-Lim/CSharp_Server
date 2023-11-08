using System.Net.Sockets;

namespace Server_Homework
{
    public static class SocketExtentions
    {
        public static ValueTask<int> SendAsync(this Socket socket, Header header, Data data)
        {
            var packetbuffer = new Memory<byte>(new byte[Header.HeaderSize + header.messageLength]);

            var headerMemory = header.Serialize();
            var dataMemory = data.Serialize();

            if (false == headerMemory.TryCopyTo(packetbuffer.Slice(0, Header.HeaderSize)))
                throw new Exception("Failed Header Copy");

            if (false == dataMemory.TryCopyTo(packetbuffer.Slice(Header.HeaderSize, header.messageLength)))
                throw new Exception("Failed Data Copy");

            return socket.SendAsync(packetbuffer, SocketFlags.None);
        }
    }

    public static class BufferExtentions
    {
        public static Memory<byte> MultiplyBufferSize(this Memory<byte> buffer, int multipleValue = 2)
        {
            var resizeBuffer = new Memory<byte>(new byte[buffer.Length * multipleValue]);
            buffer.CopyTo(resizeBuffer);
            return resizeBuffer;
        }
    }
}
