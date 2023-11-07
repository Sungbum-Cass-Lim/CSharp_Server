using System;
using System.ComponentModel.DataAnnotations;
using System.Dynamic;
using System.Net.NetworkInformation;
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
        public int headerLength;
        public int messageLength;
        public int ownerId;
        public SendType sendType;

        public Header Initialize(int msgLength, int id, SendType type)
        {
            headerLength = Unsafe.SizeOf<Header>();
            messageLength = msgLength;
            ownerId = id;
            sendType = type;

            return this;
        }

        public unsafe Memory<byte> Serialize()
        {
            Header targetHeader = this;
            byte[] headerByte = new byte[headerLength];

            fixed (byte* headerBytes = headerByte)
            {
                Buffer.MemoryCopy(&targetHeader, headerBytes, headerLength, headerLength);
            }

            return new Memory<byte>(headerByte);
        }

        public unsafe Header Deserialize(Memory<byte> readBuffer)
        {
            fixed (byte* headerPtr = readBuffer.Span)
            {
                return *(Header*)headerPtr;
            }
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Data
    {
        public string message;

        public Data Initialize(string msg)
        {
            message = msg;

            return this;
        }

        public Memory<byte> Serialize()
        {
            Data tartgetData = this;
            var messageEncodingValue = Encoding.UTF8.GetBytes(tartgetData.message);

            return new Memory<byte>(messageEncodingValue);
        }

        public Data Deserialize(Memory<byte> readbuffer)
        {
            Data data = this;
            data.message = Encoding.UTF8.GetString(readbuffer.Span);

            return data;
        }
    }

    public class Packet
    {
        public Memory<byte> WritePacket(Header header, Data data) // Packet -> Byte
        {
            var Packetbuffer = new Memory<byte>(new byte[header.headerLength + header.messageLength]);

            header.Serialize().CopyTo(Packetbuffer.Slice(0, header.headerLength));
            data.Serialize().CopyTo(Packetbuffer.Slice(header.headerLength, header.messageLength));

            return Packetbuffer;
        }

        public Header ReadHeader(Memory<byte> headerBuffer) // Byte -> Packet
        {
            Header header = new Header().Deserialize(headerBuffer);

            return header;
        }

        public Data ReadData(Memory<byte> dataBuffer) // Byte -> Packet
        {
            Data Data = new Data().Deserialize(dataBuffer);

            return Data;
        }
    }
}