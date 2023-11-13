using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Server_Homework
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            while (true)
            {
                Console.WriteLine("Connect to xxxxxx [ENTER]");
                Console.ReadKey();

                Client mainClient = new Client();
                mainClient.ConnectAsync();

                while (true)
                {
                    string inputMsg = Console.ReadLine();

                    if (inputMsg == "Q" || inputMsg == "q")
                    {
                        mainClient.SocketDisconnect();
                        break;
                    }

                    //info payload
                    Header infoHeader = new Header(MessageInfo.msgInfoLength, PayloadTag.msgInfo);
                    MessageInfo msgInfo = new MessageInfo(mainClient.GetId(), SendType.broadCast);
                    var sentBytes = await mainClient.Send(infoHeader, msgInfo);
                    if (sentBytes <= 0)
                    {
                        break;
                    }

                    Header msgHeader = new Header(inputMsg.Length, PayloadTag.msg);
                    Message msg = new Message(inputMsg);

                    try
                    {
                        await mainClient.Send(msgHeader, msg);
                    }
                    catch (Exception e)
                    {
                        break;
                    }
                }
            }
        }
    }
}
