using System;
using System.ComponentModel.DataAnnotations;
using System.Dynamic;
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

    [Serializable, StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Header // 1
    {
        public int OwnerId;
        public SendType SendType;
        public int MessageLength;
        public int HeaderLength;
    }

    [Serializable, StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Data // 3
    {
        public byte[] Message;
    }

    public class Packet
    {
        private Header TcpHeader = new Header();
        private Data TcpData = new Data();

        private byte[] WriteBuffer;

        public Packet(Header Header, Data Data)
        {
            TcpHeader = Header;
            TcpData = Data;
        }
        public unsafe Packet(int Id = 0, string Message = "", SendType SendType = SendType.BroadCast)
        {
            try
            {
                TcpHeader.OwnerId = Id;
                TcpHeader.HeaderLength = sizeof(Header);
                TcpHeader.SendType = SendType;

                TcpData.Message = Encoding.UTF8.GetBytes(Message);

                TcpHeader.MessageLength = TcpData.Message.Length;
            }
            catch (Exception E)
            {
                Console.WriteLine(E);
            }
        }

        public byte[] Write()
        {
            this.WriteBuffer = PacketConverter.ConvertPacketToByte(TcpHeader, TcpData);
            return WriteBuffer;
        }

        public Packet Read(byte[] ReadBuffer)
        {
            return PacketConverter.ConvertByteToPacket(ReadBuffer);
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
            return Encoding.UTF8.GetString(TcpData.Message);
        }
        #endregion
    }

    public unsafe class PacketConverter
    {
        public static byte[] ConvertPacketToByte(Header Header, Data Data)
        {
            var HeaderByteArray = new Span<byte>(&Header, Header.HeaderLength).ToArray();
            var DataByteArray = Data.Message;

            int ReturnBufferSize = HeaderByteArray.Length + DataByteArray.Length;

            byte[] ReturnBuffer = new byte[ReturnBufferSize];

            Buffer.BlockCopy(HeaderByteArray, 0, ReturnBuffer, 0, Header.HeaderLength);
            Buffer.BlockCopy(DataByteArray, 0, ReturnBuffer, Header.HeaderLength, Header.MessageLength);
            
            return ReturnBuffer;
        }

        public static Packet ConvertByteToPacket(byte[] PacketBuffer)
        {
            var SpanBuffer = new Span<byte> (PacketBuffer);

            Header ReturnHeader = new Header();
            Data ReturnData = new Data();

            byte[] HeaderBuffer = SpanBuffer.Slice(0, sizeof(Header)).ToArray(); 

            fixed(byte* HeaderByte = HeaderBuffer)
            {
                ReturnHeader = *(Header*)HeaderByte;
            }

            ReturnData.Message = SpanBuffer.Slice(sizeof(Header), ReturnHeader.MessageLength).ToArray();

            Packet ReturnPacket = new Packet(ReturnHeader, ReturnData);
            return ReturnPacket;
        }
    }
}