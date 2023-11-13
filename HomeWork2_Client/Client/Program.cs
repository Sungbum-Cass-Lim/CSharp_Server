using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Server_Homework
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            Client mainClient = new Client();
            mainClient.ConnectAsync();

            while (true)
            {
                string inputMsg = Console.ReadLine();

                if(inputMsg == "Q" || inputMsg == "q")
                {
                    mainClient.SocketDisconnect();
                    break;
                }

                //info payload
                Header infoHeader = new Header(MessageInfo.msgInfoLength, PayloadTag.msgInfo);
                MessageInfo msgInfo = new MessageInfo(mainClient.GetId(), SendType.broadCast);
                await mainClient.Send(infoHeader, msgInfo);

                Header msgHeader = new Header(inputMsg.Length, PayloadTag.msg);
                Message msg = new Message(inputMsg);
                await mainClient.Send(msgHeader, msg);
            }
        }
    }
}
