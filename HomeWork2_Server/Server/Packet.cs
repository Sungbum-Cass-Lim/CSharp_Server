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
        //testTag,
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
        public PayloadTag payloadTag;

        public Header(int payloadLength, PayloadTag payloadTag)
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
            //받아온 데이터 사이즈 체크
            if (buffer.Length > headerLength)
            {
                Console.WriteLine("Big Header Error");
                return false;
            }

            //데이터 깨짐 체크
            try
            {
                fixed (byte* headerPtr = buffer.Span)
                {
                    this = *(Header*)headerPtr;
                }
            }
            catch
            {
                Console.WriteLine("Broken Header Error");
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
            //데이터 사이즈 체크
            if (payloadLength != initDataLength || buffer.Length > payloadLength)
            {
                Console.WriteLine("Big InitData Error");
                return false;
            }

            //데이터 깨짐 체크
            try
            {
                fixed (byte* initInfoPtr = buffer.Span)
                {
                    this = *(InitData*)initInfoPtr;
                }
            }
            catch
            {
                Console.WriteLine("Broken InitData Error");
                return false;
            }

            return true;
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct MessageInfo : IPayload
    {
        public static readonly int msgInfoLength = Unsafe.SizeOf<MessageInfo>();

        public int userId;
        public SendType sendType;

        public MessageInfo(int id, SendType type)
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

            if (payloadLength != msgInfoLength || buffer.Length > payloadLength)
            {
                Console.WriteLine("Big MessageInfo Error");
                return false;
            }

            try
            {
                fixed (byte* messageInfoPtr = buffer.Span)
                {
                    this = *(MessageInfo*)messageInfoPtr;
                }
            }
            catch
            {
                Console.WriteLine("Broken MessageInfo Error");
                return false;
            }

            return true;
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Message : IPayload
    {
        public static readonly int MaxMessageLength = 4096;

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
            if (payloadLength > MaxMessageLength || buffer.Length > payloadLength)
            {
                Console.WriteLine("Big Message Error");
                return false;
            }

            try
            {
                message = Encoding.UTF8.GetString(buffer.Span);
            }
            catch
            {
                Console.WriteLine("Broken Message Error");
                return false;
            }

            return true;
        }
    }
}