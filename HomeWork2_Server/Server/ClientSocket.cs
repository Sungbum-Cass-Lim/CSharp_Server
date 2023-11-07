﻿using System;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;

namespace Server_Homework
{
    public class ClientSocket
    {
        private const int BUFFER_SIZE = 8;
        private const int READ_SIZE = 2;

        private Server mainServer = null;

        private int myId;
        private Socket mySocket;
        private Task receiveLoopTask;

        public ClientSocket Initialize(Server server, int id, Socket socket)
        {
            mainServer = server;
            myId = id;
            mySocket = socket;

            receiveLoopTask = _ReceiveLoop();
            return this;
        }

        public int GetId()
        {
            return myId;
        }

        public async Task Send(int id, string msg)
        {
            Packet sendPacket = new Packet();

            Header tcpHeader = new Header().Initialize(msg.Length, id, SendType.broadCast);
            Data tcpData = new Data().Initialize(msg);

            try
            {
                await mySocket.SendAsync(sendPacket.WritePacket(tcpHeader, tcpData), SocketFlags.None);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return;
            }
        }

        private async Task _ReceiveLoop()
        {
            Memory<byte> readbuffer = new Memory<byte>(new byte[BUFFER_SIZE]);

            int readOffset = 0;
            int totalRecv = 0;

            while (true)
            {
                try
                {
                    //Receive받은 데이터의 크기 가중
                    totalRecv += await mySocket.ReceiveAsync(readbuffer.Slice(totalRecv, READ_SIZE),
                        SocketFlags.None);

                    if (_HeaderProcess(ref readbuffer, ref readOffset, totalRecv, out Header tcpHeader) == false)
                        continue;

                    if (_DataProcess(ref readbuffer, ref readOffset, totalRecv, tcpHeader) == false)
                        continue;

                    if (readbuffer.Span[readOffset] != 0)
                    {
                        var savebuffer = readbuffer.Slice(readOffset, totalRecv - readOffset);
                        int newBufferSize = (savebuffer.Length < BUFFER_SIZE ? BUFFER_SIZE : savebuffer.Length) * 2;
                        readbuffer = new Memory<byte>(new byte[newBufferSize]);
                        savebuffer.CopyTo(readbuffer);

                        totalRecv = savebuffer.Length;
                        readOffset = 0;

                        continue;
                    }

                    totalRecv = 0;
                    readOffset = 0;
                    readbuffer = new Memory<byte>(new byte[BUFFER_SIZE]);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }

        private bool _HeaderProcess(ref Memory<byte> buffer, ref int readOffset, int totalRecv, out Header tcpHeader)
        {
            Packet receivePacket = new Packet();

            tcpHeader = new Header();
            int headerSize = Unsafe.SizeOf<Header>();

            //버퍼의 한계까지 데이터를 받았지만 headerSize보다 작을 때
            if (totalRecv >= buffer.Length - READ_SIZE && totalRecv < headerSize)
            {
                var newBuffer = new Memory<byte>(new byte[buffer.Length * 2]);
                buffer.CopyTo(newBuffer);
                buffer = newBuffer;

                return false;
            }

            //그냥 받은 데이터가 Header 사이즈보다 작을 때
            else if (totalRecv < headerSize)
            {
                return false;
            }

            //Header버퍼를 다 받아왔다면 해당 버퍼를 Header로 변환
            var headerBuffer = buffer.Slice(0, headerSize);
            tcpHeader = receivePacket.ReadHeader(headerBuffer);
            readOffset = headerSize;

            return true;
        }

        private bool _DataProcess(ref Memory<byte> buffer, ref int readOffset, int totalRecv, Header tcpHeader)
        {
            Packet receivePacket = new Packet();

            int headerSize = Unsafe.SizeOf<Header>();

            //버퍼의 한계까지 데이터를 받았지만 MessageSize보다 작을 때
            if (totalRecv >= buffer.Length - READ_SIZE && totalRecv - headerSize < tcpHeader.messageLength)
            {
                var newBuffer = new Memory<byte>(new byte[buffer.Length * 2]);
                buffer.CopyTo(newBuffer);
                buffer = newBuffer;

                return false;
            }

            //그냥 받은 데이터가 message 사이즈보다 작을 때
            else if (totalRecv - headerSize < tcpHeader.messageLength)
                return false;

            //Data버퍼를 다 받아왔다면 해당 버퍼를 Data로 변환
            var dataBuffer = buffer.Slice(readOffset, tcpHeader.messageLength);
            Data tcpData = receivePacket.ReadData(dataBuffer);
            readOffset += tcpHeader.messageLength;

            //메세지가 출력되면 일단은 성공
            Console.WriteLine(tcpData.message);
            mainServer.Broadcast(tcpHeader.ownerId, tcpData.message);

            return true;
        }

        public void Close()
        {
            mySocket.Shutdown(SocketShutdown.Receive);
            mySocket.Close();
        }
    }
}