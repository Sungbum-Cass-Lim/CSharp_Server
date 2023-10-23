using System.IO;
using System.Reflection.PortableExecutable;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Server_Homework
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public unsafe struct TcpPacket
    {
        //Header
        [MarshalAs(UnmanagedType.I4)]
        public int SrcNum;
        [MarshalAs(UnmanagedType.I4)]
        public int AckNum;
        [MarshalAs(UnmanagedType.I4)]
        public int Size;

        //Data
        [MarshalAs(UnmanagedType.I4)]
        public int Id;
        [MarshalAs(UnmanagedType.LPWStr)]
        public string Message;
    }

    public class Packet
    {

    }

    public static class PacketConverter
    {
        public static unsafe byte[] ConvertPacketToByte<T>(T Value) where T : unmanaged
        {
            //Sturct의 주소값을 Byte* 형식으로 변환
            byte* Pointer = (byte*)&Value;

            //Byte배열에 Struct크기 만큼의 공간 할당
            byte[] Bytes = new byte[sizeof(T)];

            //Byte배열에 Byte*의 주소값 할당
            for(int i = 0; i < sizeof(T); i++)
            {
                Bytes[i] = Pointer[i];
            }

            //Byte배열 형태로 반환
            return Bytes;
        }

        public static unsafe T ConvertByteToPacket<T>(byte[] PacketBuffer) where T : unmanaged
        {
            //고정된 Byte*를 만들어 Byte배열 형태로 받아온 주소값을 할당
            fixed(byte* Pointer = PacketBuffer)
            {
                //주소값을 Struct 주소값 형태로 바꾸고 한번 더 *를 사용하여 T형태로 바꾼뒤 반환
                return *(T*)Pointer;
            }
        }
    }
}