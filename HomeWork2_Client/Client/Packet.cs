using System;
using System.Buffers;
using System.ComponentModel;
using System.Data.SqlTypes;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection.PortableExecutable;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Unicode;

namespace Server_Homework
{
    [Serializable, StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public unsafe struct TcpPacket
    {
        //Header
        [MarshalAs(UnmanagedType.I4)] // Sequence Number
        public int SrcNum;
        [MarshalAs(UnmanagedType.I4)] // Acknowledgment Number
        public int AckNum;
        [MarshalAs(UnmanagedType.I4)] // Header Length
        public int PacketLength;

        //Data
        [MarshalAs(UnmanagedType.I4)] //Client Id
        public int Id;
        [MarshalAs(UnmanagedType.I4)] // Message Length
        public int MessageLength;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 50)]
        public fixed char Message[50]; // Message Data
    }

    public class Packet
    {
        public TcpPacket Pkt = new TcpPacket();

        public string Message;

        private byte[] WriteBuffer;
        private byte[] ReadBuffer;

        public unsafe Packet(int SrcNum = 0, int AckNum = 0, int Id = 0, string Msg = "")
        {
            Pkt.SrcNum = SrcNum;
            Pkt.AckNum = AckNum;

            Pkt.PacketLength = sizeof(TcpPacket);
            Pkt.MessageLength = Encoding.Unicode.GetByteCount(Msg);

            byte[] CopyByteMsg = Encoding.UTF8.GetBytes(Msg);

            int i = 0;
            foreach (char C in Encoding.UTF8.GetChars(CopyByteMsg))
            {
                Pkt.Message[i] = C;
                i++;
            }

            Pkt.Id = Id;
        }


        ~Packet()
        {
            // 만약 메모리가 할당 되고 해제가 안된다면 여기서 해제 진행
        }

        public byte[] Write()
        {
            this.WriteBuffer = PacketConverter.ConvertPacketToByte(Pkt);
            return WriteBuffer;
        }

        public unsafe TcpPacket Read(byte[] ReadBuffer)
        {
            this.ReadBuffer = ReadBuffer;
            Pkt = PacketConverter.ConvertByteToPacket<TcpPacket>(this.ReadBuffer);

            fixed (char* CopyString = Pkt.Message)
            {
                Message = new string(CopyString);
            }

            return Pkt;
        }
    }

    public class PacketConverter
    {
        public static unsafe byte[] ConvertPacketToByte<T>(T Value) where T : struct
        {
            //Sturct의 주소값을 Byte* 형식으로 변환
            byte* Pointer = (byte*)&Value;

            //Byte배열에 Struct크기 만큼의 공간 할당
            byte[] Bytes = new byte[sizeof(T)];

            //Byte배열에 Byte*의 주소값 할당
            for (int i = 0; i < sizeof(T); i++)
            {
                Bytes[i] = Pointer[i];
            }

            //Byte배열 형태로 반환
            return Bytes;
        }

        public static unsafe T ConvertByteToPacket<T>(byte[] PacketBuffer) where T : struct
        {
            //고정된 Byte*를 만들어 Byte배열 형태로 받아온 주소값을 할당
            fixed (byte* Pointer = PacketBuffer)
            {
                //주소값을 Struct 주소값 형태로 바꾸고 한번 더 *를 사용하여 T형태로 바꾼뒤 반환
                return *(T*)Pointer;
            }
        }
    }
}