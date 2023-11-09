using System.Net.Sockets;

namespace Server_Homework
{
    //소켓 관련 Extionsion Method
    public static class SocketExtensions
    {
        public static ValueTask<int> SendAsync(this Socket socket, Header header, Data data)
        {
            var packetBuffer = new Memory<byte>(new byte[Header.headerSize + header.messageLength]);

            var headerMemory = header.Serialize();
            var dataMemory = data.Serialize();

            headerMemory.CopyTo(packetBuffer.Slice(0, Header.headerSize));
            dataMemory.CopyTo(packetBuffer.Slice(Header.headerSize, header.messageLength));

            return socket.SendAsync(packetBuffer, SocketFlags.None);
        }

        public static void SocketDisconnect(this Socket socket)
        {
            socket.Shutdown(SocketShutdown.Both);
            socket.Close();
        }
    }

    //버퍼 관련 Extension Method
    public static class BufferExtensions
    {
        public static Memory<byte> MultiplyBufferSize(this Memory<byte> buffer, int multipleValue = 2)
        {
            var resizeBuffer = new Memory<byte>(new byte[buffer.Length * multipleValue]);
            buffer.CopyTo(resizeBuffer);
            return resizeBuffer;
        }
    }
}
