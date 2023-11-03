using System;
using System.ComponentModel.DataAnnotations;
using System.Dynamic;
using System.Net.NetworkInformation;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Xml.Linq;

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

            fixed(byte* HeaderBytes = HeaderByte)
            {
                Buffer.MemoryCopy(&TargetHeader, HeaderBytes, 0, HeaderLength);
            }

            var HeaderBuffer = new Memory<byte>(HeaderByte);
            return HeaderBuffer;
        }

        public unsafe Header Deserialize(Memory<byte> ReadBuffer)
        {
            Header Header = this;

            fixed(byte* HeaderPtr = ReadBuffer.Span)
            {
                Header = *(Header*)HeaderPtr;
            }

            return Header;
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

        public unsafe Memory<byte> Serialize()
        {
            Data TartgetData = this;
            var MessageEncodingValue = Encoding.UTF8.GetBytes(TartgetData.Message);

            var DataBuffer = new Memory<byte>(MessageEncodingValue);
            return DataBuffer;
        }

        public unsafe Data Deserialize(Memory<byte> ReadBuffer, int MsgLength)
        {
            Data Data = this;

            fixed(byte* DataPtr = ReadBuffer.Span)
            {
                Data.Message = Encoding.UTF8.GetString(DataPtr, MsgLength);
            }

            return Data;
        }
    }

    public class Packet
    {
        private Header TcpHeader;
        private Data TcpData;

        public Packet(int Id = 0, string Message = "", SendType SendType = SendType.BroadCast)
        {
            try
            {
                TcpHeader.Initialize(Message.Length, Id, SendType);
                TcpData.Initialize(Message);
            }
            catch (Exception E)
            {
                Console.WriteLine(E);
            }
        }

        public byte[] Write()
        {
            byte[] WriteBuffer = TcpConverter.PacketToByte(TcpHeader, TcpData);
            return WriteBuffer;
        }

        public Header ReadHeader(byte[] ReadBuffer)
        {
            return new Header();
        }

        public Data ReadData(byte[] ReadBuffer)
        {
            return new Data();
        }

        #region Access PacketData Func
        public int GetHeaderLength()
        {
            return TcpHeader.HeaderLength;
        }
        public SendType GetSendType()
        {
            return TcpHeader.SendType;
        }
        public int GetID()
        {
            return TcpHeader.OwnerId;
        }
        public int GetDataLength()
        {
            return TcpHeader.MessageLength;
        }
        public string GetMessage()
        {
            return TcpData.Message;
        }
        #endregion
    }

    public unsafe class TcpConverter
    {
        public static byte[] PacketToByte(Header Header, Data Data) // Packet -> Byte
        {
            byte[] PacketBuffer = new byte[Header.HeaderLength + Header.MessageLength];

            fixed(byte* PacketBufferPtr = Header.Serialize().Span)
            {
                Buffer.BlockCopy(PacketBufferPtr, 0, PacketBuffer, 0, 0);
            }

            return PacketBuffer;
        }

        public static Header ByteToHeader(Span<byte> Buffer, int Start) // Byte -> Packet
        {
            var SpanHeaderBuffer = new Span<byte>(Buffer);
            byte[] HeaderBuffer = SpanHeaderBuffer.Slice(Start, sizeof(Header)).ToArray();

            fixed(byte* HeaderByte = HeaderBuffer)
            {
                return *(Header*)HeaderByte;
            }
        }

        public static Packet ByteToData(byte[] PacketBuffer) // Byte -> Packet
        {
            var SpanPacketBuffer = new Span<byte>(PacketBuffer);

            Header ReturnHeader = new Header();
            Data ReturnData = new Data();

            byte[] HeaderBuffer = SpanPacketBuffer.Slice(0, sizeof(Header)).ToArray();

            fixed (byte* HeaderByte = HeaderBuffer)
            {
                ReturnHeader = *(Header*)HeaderByte;
            }

            ReturnData.Message = SpanPacketBuffer.Slice(sizeof(Header), ReturnHeader.MessageLength).ToArray();

            Packet ReturnPacket = new Packet(ReturnHeader, ReturnData);
            return ReturnPacket;
        }
    }
}