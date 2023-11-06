using Client;
using System;
using System.ComponentModel.DataAnnotations;
using System.Dynamic;
using System.Net.NetworkInformation;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace Server_Homework
{
    public enum SendType
    {
        BroadCast = 1,
        MultiCast = 2,
        UniCast = 3,
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Header
    {
        public int HeaderLength;
        public int MessageLength;
        public int OwnerId;
        public SendType SendType;

        public void Initialize(int MsgLength, int Id, SendType Type)
        {
            HeaderLength = Unsafe.SizeOf<Header>();
            MessageLength = MsgLength;
            OwnerId = Id;
            SendType = Type;
        }

        public unsafe Memory<byte> Serialize()
        {
            Header TargetHeader = this;
            byte[] HeaderByte = new byte[HeaderLength];

            fixed (byte* HeaderBytes = HeaderByte)
            {
                Buffer.MemoryCopy(&TargetHeader, HeaderBytes, 0, HeaderLength);
            }

            return new Memory<byte>(HeaderByte);
        }

        public unsafe Header Deserialize(Memory<byte> ReadBuffer)
        {
            fixed (byte* HeaderPtr = ReadBuffer.Span)
            {
                return *(Header*)HeaderPtr;
            }
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Data
    {
        public string Message;

        public void Initialize(string Msg)
        {
            Message = Msg;
        }

        public Memory<byte> Serialize()
        {
            Data TartgetData = this;
            var MessageEncodingValue = Encoding.UTF8.GetBytes(TartgetData.Message);

            return new Memory<byte>(MessageEncodingValue);
        }

        public Data Deserialize(Memory<byte> ReadBuffer, int MsgLength)
        {
            Data Data = this;
            Data.Message = Encoding.UTF8.GetString(ReadBuffer.Span);

            return Data;
        }
    }

    public class Packet
    {
        public Memory<byte> WritePacket(Header Header, Data Data) // Packet -> Byte
        {
            var PacketBuffer = new Memory<byte>(new byte[Header.HeaderLength + Header.MessageLength]);

            PacketBuffer.Slice(0, Header.HeaderLength).CopyTo(Header.Serialize());
            PacketBuffer.Slice(Header.HeaderLength, Header.MessageLength).CopyTo(Data.Serialize());

            return PacketBuffer;
        }

        public Header ReadHeader(Memory<byte> Buffer) // Byte -> Packet
        {
            var HeaderBuffer = Buffer.Slice(0, Unsafe.SizeOf<Header>());

            Header Header = new Header().Deserialize(HeaderBuffer);
            return Header;
        }

        public Data ReadData(Memory<byte> Buffer, Header Header) // Byte -> Packet
        {
            var DataBuffer = Buffer.Slice(Header.HeaderLength, Header.MessageLength);

            Data Data = new Data().Deserialize(DataBuffer, Header.MessageLength);
            return Data;
        }
    }
}