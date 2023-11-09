using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.SymbolStore;
using System.Dynamic;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace Server_Homework
{
    public enum SendType
    {
        broadCast = 1,
        multiCast = 2,
        uniCast = 3,
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Header
    {
        public static readonly int HeaderSize = Unsafe.SizeOf<Header>();

        public int messageLength;
        public int messageId;






        public int ownerId;
        public SendType sendType;

        public Header(int msgLength, int id, SendType type)
        {
            messageLength = msgLength;
            ownerId = id;
            sendType = type;
        }

        public unsafe Memory<byte> Serialize()
        {
            Header targetHeader = this;
            byte[] headerByte = new byte[HeaderSize];

            fixed (byte* headerBytes = headerByte)
            {
                Buffer.MemoryCopy(&targetHeader, headerBytes, HeaderSize, HeaderSize);
            }

            return new Memory<byte>(headerByte);
        }

        //불필요한 Header재생성을 막기 위해 outX(※ Dictionary의 TryAdd참고)
        public unsafe bool TryDeserialize(Memory<byte> readBuffer)
        {
            if (readBuffer.Length < HeaderSize)
                return false;

            fixed (byte* headerPtr = readBuffer.Span)
            {
                this = *(Header*)headerPtr;
            }
            return true;
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Data
    {
        public string message;

        public Data(string msg)
        {
            message = msg;
        }

        public Memory<byte> Serialize()
        {
            var messageEncodingValue = Encoding.UTF8.GetBytes(message);

            return new Memory<byte>(messageEncodingValue);
        }

        public bool TryDeserialize(Memory<byte> readBuffer, int msgLength)
        {
            if (readBuffer.Length < msgLength)
                return false;

            message = Encoding.UTF8.GetString(readBuffer.Span);
            return true;
        }
    }
}