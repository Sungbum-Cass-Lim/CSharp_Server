using System;
using System.Dynamic;
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

    [Serializable, StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public unsafe struct TcpPacket
    {
        //Header
        public int SrcNum;
        public int AckNum;
        public SendType Type;
        public int PacketLength;

        //Data
        public int Id;
        public int MessageLength;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 50)]
        public fixed char Message[50]; // Message Data
    }

    public class Packet
    {
        private TcpPacket Pkt = new TcpPacket();

        private string Message;

        private byte[] WriteBuffer;
        private byte[] ReadBuffer;

        public unsafe Packet(int SrcNum = 0, int AckNum = 0, int Id = 0, string Msg = "", SendType SendType = SendType.BroadCast)
        {
            Pkt.SrcNum = SrcNum;
            Pkt.AckNum = AckNum;
            Pkt.Type = SendType;

            Pkt.PacketLength = sizeof(TcpPacket);
            Pkt.MessageLength = Encoding.Unicode.GetByteCount(Msg);

            byte[] CopyByteMsg = Encoding.UTF8.GetBytes(Msg);
            int i = 0;
            foreach (char C in Encoding.UTF8.GetChars(CopyByteMsg)) //TODO: Fixed 배열에 할당 방법을 몰라서 임시
            {
                Pkt.Message[i] = C;
                i++;
            }

            Pkt.Id = Id;
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

        #region Access PacketData Func
        public int GetPacketLength()
        {
            return Pkt.PacketLength;
        }
        public int GetID()
        {
            return Pkt.Id;
        }
        public int GetMessageLength()
        {
            return Pkt.MessageLength;
        }
        public string GetMessage()
        {
            return Message;
        }
        #endregion
    }

    public class PacketConverter
    {
        public static unsafe byte[] ConvertPacketToByte<T>(T Value) where T : unmanaged
        {
            var ByteArray = new Span<byte>(&Value, sizeof(T));

            return ByteArray.ToArray();
        }

        public static unsafe T ConvertByteToPacket<T>(byte[] PacketBuffer) where T : unmanaged
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