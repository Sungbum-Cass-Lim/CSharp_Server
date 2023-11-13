using System.Net.Sockets;

namespace Server_Homework
{
    //소켓 관련 Extionsion Method
    public static class SocketExtensions
    {
        public static ValueTask<int> SendAsync<T>(this Socket socket, Header header, T payload) where T : IPayload
        {
            var packetBuffer = new Memory<byte>(new byte[Header.headerLength + header.payloadLength]);

            var headerMemory = header.Serialize();
            var payloadMemory = payload.Serialize(header);

            headerMemory.CopyTo(packetBuffer.Slice(0, Header.headerLength));
            payloadMemory.CopyTo(packetBuffer.Slice(Header.headerLength, header.payloadLength));

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
            try
            {
                var resizeBuffer = new Memory<byte>(new byte[buffer.Length * multipleValue]);
                buffer.CopyTo(resizeBuffer);
                return resizeBuffer;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
