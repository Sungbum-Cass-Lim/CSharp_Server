using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace Server_Homework
{
    public enum PayloadTag
    {
        info,
        msg,
    }

    public enum SendType
    {
        broadCast = 1,
        multiCast,
        uniCast,
    }

    interface IPayload
    {
        public abstract Memory<byte> Serialize(Header header);
        public abstract bool TryDeserialize(Header header, Memory<byte> buffer);
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Header
    {
        public static readonly int headerSize = Unsafe.SizeOf<Header>();

        public int payloadLength;
        public int payloadTag;

        public Header(int payloadLength, int payloadTag)
        {
            this.payloadLength = payloadLength;
            this.payloadTag = payloadTag;
        }

        public unsafe Memory<byte> Serialize()
        {
            Header header = this;
            byte[] headerByte = new byte[headerSize];

            fixed (byte* headerBytes = headerByte)
            {
                Buffer.MemoryCopy(&header, headerBytes, headerSize, headerSize);
            }

            return new Memory<byte>(headerByte);
        }

        public unsafe bool TryDeserialize(Memory<byte> buffer)
        {
            if (buffer.Length < headerSize)
                return false;

            fixed (byte* headerPtr = buffer.Span)
            {
                this = *(Header*)headerPtr;
            }
            return true;
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct MessageInfo : IPayload
    {
        public int userId;
        public int sendType;

        public MessageInfo(int id, int type)
        {
            userId = id;
            sendType = type;
        }

        public unsafe Memory<byte> Serialize(Header header)
        {
            MessageInfo messageInfo = this;
            byte[] headerByte = new byte[header.payloadLength];

            fixed (byte* headerBytes = headerByte)
            {
                Buffer.MemoryCopy(&messageInfo, headerBytes, header.payloadLength, header.payloadLength);
            }

            return new Memory<byte>(headerByte);
        }

        public unsafe bool TryDeserialize(Header header, Memory<byte> buffer)
        {
            //엄청 큰 버퍼나 악의적으로 들어오는 버퍼에 대한 예외처리가 되있어야 함
            if (buffer.Length < header.payloadLength)
                return false;

            fixed (byte* headerPtr = buffer.Span)
            {
                this = *(MessageInfo*)headerPtr;
            }
            return true;
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Message : IPayload
    {
        public string message;

        public Message(string msg)
        {
            message = msg;
        }

        public unsafe Memory<byte> Serialize(Header header)
        {
            var messageEncodingValue = Encoding.UTF8.GetBytes(message, header.payloadLength);

            return new Memory<byte>(messageEncodingValue);
        }

        public unsafe bool TryDeserialize(Header header ,Memory<byte> buffer)
        {
            if (buffer.Length < header.payloadLength)
                return false;

            message = Encoding.UTF8.GetString(buffer.Span);
            return true;
        }
    }
}