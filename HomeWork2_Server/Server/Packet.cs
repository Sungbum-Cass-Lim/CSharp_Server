using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace Server_Homework
{
    public enum SendType
    {
        broadCast = 1,
        multiCast,
        uniCast,
    }

    public enum PayloadTag
    {
        initInfo,
        msgInfo,
        msg,
    }

    public interface IPayload
    {
        Memory<byte> Serialize(Header header);
        bool TryDeserialize(int payloadLength, Memory<byte> buffer);
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Header
    {
        public static readonly int headerLength = Unsafe.SizeOf<Header>();

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
            byte[] headerByte = new byte[headerLength];

            fixed (byte* headerBytes = headerByte)
            {
                Buffer.MemoryCopy(&header, headerBytes, headerLength, headerLength);
            }

            return new Memory<byte>(headerByte);
        }

        public unsafe bool TryDeserialize(Memory<byte> buffer)
        {
            if (buffer.Length < headerLength)
                return false;

            try
            {
                fixed (byte* headerPtr = buffer.Span)
                {
                    this = *(Header*)headerPtr;
                }
            }
            catch
            {
                return false;
            }

            return true;
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct InitData : IPayload
    {
        public static readonly int initDataLength = Unsafe.SizeOf<InitData>();

        public int myUserId;

        public InitData(int id)
        {
            myUserId = id;
        }

        public unsafe Memory<byte> Serialize(Header header)
        {
            InitData initInfo = this;
            byte[] headerByte = new byte[header.payloadLength];

            fixed (byte* headerBytes = headerByte)
            {
                Buffer.MemoryCopy(&initInfo, headerBytes, header.payloadLength, header.payloadLength);
            }

            return new Memory<byte>(headerByte);
        }

        public unsafe bool TryDeserialize(int payloadLength, Memory<byte> buffer)
        {
            //엄청 큰 버퍼나 악의적으로 들어오는 버퍼에 대한 예외처리가 되있어야 함
            if (buffer.Length < payloadLength)
                return false;

            fixed (byte* initInfoPtr = buffer.Span)
            {
                this = *(InitData*)initInfoPtr;
            }
            return true;
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct MessageInfo : IPayload
    {
        public static readonly int msgInfoLength = Unsafe.SizeOf<MessageInfo>();

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

        public unsafe bool TryDeserialize(int payloadLength, Memory<byte> buffer)
        {
            //엄청 큰 버퍼나 악의적으로 들어오는 버퍼에 대한 예외처리가 되있어야 함
            if (buffer.Length < payloadLength)
                return false;

            fixed (byte* messageInfoPtr = buffer.Span)
            {
                this = *(MessageInfo*)messageInfoPtr;
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

        public Memory<byte> Serialize(Header header)
        {
            var messageEncodingValue = Encoding.UTF8.GetBytes(message, 0, header.payloadLength);

            return new Memory<byte>(messageEncodingValue);
        }

        public bool TryDeserialize(int payloadLength, Memory<byte> buffer)
        {
            if (buffer.Length < payloadLength)
                return false;

            message = Encoding.UTF8.GetString(buffer.Span);
            return true;
        }
    }
}