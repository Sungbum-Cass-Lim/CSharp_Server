using System.IO;
using System.Reflection.PortableExecutable;
using System.Runtime.InteropServices;

namespace Server_Homework
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct TcpPacketHeader
    {
        [MarshalAs(UnmanagedType.I4)]
        public int SourcePort;
        [MarshalAs(UnmanagedType.I4)]
        public int DestinationPort;
        [MarshalAs(UnmanagedType.I4)]
        public int SequenceNumber;
        [MarshalAs(UnmanagedType.I4)]
        public int AcknowledgementNumber;

        public TcpPacketHeader(int SCPort, int DSPort, int SEQNum, int ACKNum)
        {
            SourcePort = SCPort;
            DestinationPort = DSPort;
            SequenceNumber = SEQNum;
            AcknowledgementNumber = ACKNum;
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct TcpPacketData
    {
        [MarshalAs(UnmanagedType.I4)]
        public int UserId;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public string Message;

        public TcpPacketData(int Id, string Msg)
        {
            UserId = Id;
            Message = Msg;
        }
    }
    public class Packet
    {
        public TcpPacketHeader PacketHeader;
        public TcpPacketData PacketData;

        public byte[] Write(TcpPacketHeader Header, TcpPacketData Data)
        {
            byte[] HeaderBytes = WriteHeader(Header);
            byte[] DataBytes = WriteData(Data);

            byte[] ReturnData = HeaderBytes.Concat(DataBytes).ToArray();
            return ReturnData;
        }
        private byte[] WriteHeader (TcpPacketHeader Header) 
        {
            int BufferSize = Marshal.SizeOf(Header);
            IntPtr Buffer = Marshal.AllocHGlobal(BufferSize);
            Marshal.StructureToPtr(Header, Buffer, false);

            byte[] Data = new byte[BufferSize];
            Marshal.Copy(Buffer, Data, 0, Data.Length);
            Marshal.FreeHGlobal(Buffer);

            return Data;
        }
        private byte[] WriteData (TcpPacketData Msg)
        {
            int BufferSize = Marshal.SizeOf(Msg);
            IntPtr Buffer = Marshal.AllocHGlobal(BufferSize);
            Marshal.StructureToPtr(Msg, Buffer, false);

            byte[] Data = new byte[BufferSize];
            Marshal.Copy(Buffer, Data, 0, Data.Length);
            Marshal.FreeHGlobal(Buffer);

            return Data;
        }

        public Packet Read(byte[] ReceiveData)
        {
            ReadHeader(ReceiveData); // ReadStart

            return this;
        }
        private void ReadHeader(byte[] ReceiveData)
        {
            int HeaderSize = Marshal.SizeOf(PacketHeader);
            IntPtr HeaderPtr = IntPtr.Zero;

            HeaderPtr = Marshal.AllocHGlobal(HeaderSize);
            Marshal.Copy(ReceiveData, 0, HeaderPtr, HeaderSize);

            PacketHeader = (TcpPacketHeader)Marshal.PtrToStructure(HeaderPtr, PacketHeader.GetType());

            Marshal.FreeHGlobal(HeaderPtr);
            ReadData(ReceiveData, HeaderSize);
        }
        private void ReadData(byte[] ReceiveData, int HeaderSize)
        {
            int DataSize = Marshal.SizeOf(PacketData);
            IntPtr DataPtr = IntPtr.Zero;

            DataPtr = Marshal.AllocHGlobal(DataSize);
            Marshal.Copy(ReceiveData, HeaderSize, DataPtr, DataSize);

            PacketData = (TcpPacketData)Marshal.PtrToStructure(DataPtr, PacketData.GetType());

            Marshal.FreeHGlobal(DataPtr);
        }
    }
}