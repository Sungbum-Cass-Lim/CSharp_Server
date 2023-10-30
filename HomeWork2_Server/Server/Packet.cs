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
    public struct Header
    {
        public int OwnerId;
        public int HeaderLength;
    }

    [Serializable, StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Data
    {
        public byte[] Message;
    }

    [Serializable, StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct DataInfo
    {
        public SendType SendType;
        public int MessageLength;
    }

    public class Packet
    {
        private Header TcpHeader = new Header();
        private Data TcpData = new Data();
        private DataInfo TcpDataInfo = new DataInfo();

        private byte[] WriteBuffer;

        public Packet(Header Header, Data Data, DataInfo Info)
        {
            TcpHeader = Header;
            TcpData = Data;
            TcpDataInfo = Info;
        }
        public unsafe Packet(int Id = 0, string Message = "", SendType SendType = SendType.BroadCast)
        {
            try
            {
                TcpHeader.OwnerId = Id;
                TcpHeader.HeaderLength = sizeof(Header);

                TcpData.Message = Encoding.UTF8.GetBytes(Message);

                TcpDataInfo.SendType = SendType;
                TcpDataInfo.MessageLength = TcpData.Message.Length;
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
            return TcpDataInfo.SendType;
        }
        public int GetID()
        {
            return TcpHeader.OwnerId;
        }
        public int GetDataLength()
        {
            return TcpDataInfo.MessageLength;
        }
        public string GetMessage()
        {
            return Encoding.UTF8.GetString(TcpData.Message);
        }
        #endregion
    }

    public unsafe class PacketConverter
    {
        public static byte[] ConvertPacketToByte(Header Header, Data Data, DataInfo Info)
        {
            var HeaderByteArray = new Span<byte>(&Header, Header.HeaderLength);
            var DataByteArray = new Span<byte>(Data.Message, Data.DataLength).ToArray();

            byte[] ReturnBuffer = new byte[HeaderByteArray.Length + Data.DataLength];

            Buffer.BlockCopy(HeaderByteArray.ToArray(), 0, ReturnBuffer, 0, Header.HeaderLength);
            Buffer.BlockCopy(DataByteArray.ToArray(), 0, ReturnBuffer, Header.HeaderLength, Data.DataLength);

            return ReturnBuffer;
        }

        public static Packet ConvertByteToPacket(byte[] PacketBuffer)
        {
            TcpHeader ReturnHeader = new TcpHeader();
            TcpData ReturnData = new TcpData();

            var HeaderBuffer = new Span<byte>(PacketBuffer).Slice(0, sizeof(TcpHeader)).ToArray();
            var DataBuffer = new Span<byte>(PacketBuffer).Slice(sizeof(TcpHeader), sizeof(TcpData)).ToArray();

            fixed(byte* HeaderByte = HeaderBuffer.ToArray())
            {
                ReturnHeader = *(TcpHeader*)HeaderByte;
            }
            fixed (byte* DataByte = DataBuffer.ToArray())
            {
                ReturnData = *(TcpData*)DataByte;
            }

            Packet ReturnPacket = new Packet(ReturnHeader, ReturnData);
            return ReturnPacket;
        }
    }
}