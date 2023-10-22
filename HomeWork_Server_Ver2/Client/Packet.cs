using System.IO;
using System.Reflection.PortableExecutable;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Server_Homework
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public unsafe struct TcpPacketHeader
    {
        public fixed int Id[2];
        public fixed char Message[128];
    }

    public class Packet
    {

    }

    public class PacketConverter
    {
        public PacketConverter(ref TcpPacketHeader Header)
        {
            TcpPacketHeader Had = new TcpPacketHeader();

            unsafe
            {
                IntPtr a;

                fixed (int* TcpHeader = &Header)
                {

                }
            }
        }

        public void PacketToByte()
        {

        }

        public void ByteToPacket()
        {

        }
    }
}