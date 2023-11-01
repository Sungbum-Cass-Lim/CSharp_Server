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
        public int HeaderLength;
    }

    [Serializable, StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct DataInfo // 2
    {
        public SendType SendType;
        public int MessageLength;
        public int InfoLength;
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
        private DataInfo TcpDataInfo = new DataInfo();

        private byte[] WriteBuffer;

        public Packet(Header Header, DataInfo Info, Data Data)
        {
            TcpHeader = Header;
            TcpDataInfo = Info;
            TcpData = Data;
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
                TcpDataInfo.InfoLength = sizeof(DataInfo);
            }
            catch (Exception E)
            {
                Console.WriteLine(E);
            }
        }

        public byte[] Write()
        {
            this.WriteBuffer = PacketConverter.ConvertPacketToByte(TcpHeader, TcpData, TcpDataInfo);
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
            var HeaderByteArray = new Span<byte>(&Header, Header.HeaderLength).ToArray();
            var InfoByteArray = new Span<byte>(&Info, Info.InfoLength).ToArray();
            var DataByteArray = Data.Message;

            int ReturnBufferSize = HeaderByteArray.Length + InfoByteArray.Length + DataByteArray.Length;

            byte[] ReturnBuffer = new byte[ReturnBufferSize];

            Buffer.BlockCopy(HeaderByteArray, 0, ReturnBuffer, 0, Header.HeaderLength);
            Buffer.BlockCopy(InfoByteArray, 0, ReturnBuffer, Header.HeaderLength, Info.InfoLength);
            Buffer.BlockCopy(DataByteArray, 0, ReturnBuffer, Header.HeaderLength + Info.InfoLength, Info.MessageLength);
            
            return ReturnBuffer;
        }

        public static Packet ConvertByteToPacket(byte[] PacketBuffer)
        {
            var SpanBuffer = new Span<byte> (PacketBuffer);

            Header ReturnHeader = new Header();
            DataInfo ReturnDataInfo = new DataInfo();
            Data ReturnData = new Data();

            byte[] HeaderBuffer = SpanBuffer.Slice(0, sizeof(Header)).ToArray(); 
            byte[] DataInfoBuffer = SpanBuffer.Slice(sizeof(Header), sizeof(DataInfo)).ToArray();

            fixed(byte* HeaderByte = HeaderBuffer)
            {
                ReturnHeader = *(Header*)HeaderByte;
            }
            fixed (byte* InfoByte = DataInfoBuffer)
            {
                ReturnDataInfo = *(DataInfo*)InfoByte;
            }

            ReturnData.Message = SpanBuffer.Slice(sizeof(Header) + sizeof(DataInfo), ReturnDataInfo.MessageLength).ToArray();

            Packet ReturnPacket = new Packet(ReturnHeader, ReturnDataInfo, ReturnData);
            return ReturnPacket;
        }
    }
}