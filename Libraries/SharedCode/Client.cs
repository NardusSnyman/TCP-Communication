using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Runtime;
using System.Threading.Tasks;
using static ClientServer.EncodingClasses;
using System.Threading;
using System.Linq;

namespace ClientServer
{
    public class Client
    {

        Action client_act;
        public ConnectionArguments args;//connection arguments
        NetworkData overread;//data storage for overread data
        public Queue<ClientMessage> command_queue = new Queue<ClientMessage>();
        public Client(ConnectionArguments args1)
        {
            args = args1;
        }
        //-----------------
        int temp = 0;
        public void Communicate(string operation, NetworkData message, Action<NetworkData> onCompleted, Action failed = null)
         {
            ClientMessage cm = new ClientMessage();
            cm.failed = failed;
            cm.completed = onCompleted;
            cm.operation = operation;
            cm.message = message;
            command_queue.Enqueue(cm);
         }

        
        public Action clientThread()
        {
            Action act = new Action(()=>
            {
                while (true)
                {
                    if (command_queue.Count > 0)
                    {
                        

                        TcpClient client = new TcpClient();
                        
                        ClientMessage cm = command_queue.Dequeue();

                    start:


                        try
                        {
                            client.Connect(args.ip, args.port);
                        }
                        catch (Exception e)
                        {
                            Thread.Sleep(1200);
                            temp++;
                            if (temp > args.server_reconnect_attempts)
                            {
                                temp = 0;
                                cm.failed?.Invoke();
                                client.Close();
                                return;
                            }

                            goto start;
                        }
                        string msg = "";
                        if (cm.message != null)
                        {
                            msg = cm.message.GetDecodedString();
                        }
                        NetworkData dat = NetworkData.fromEncodedString(String.Join(SendRecieveUtil.separator, 
                            new List<string>() { cm.operation, msg  }.Select(x=>NetworkData.fromDecodedString(x).GetEncodedString())));
                        dat.InitStream();
                        SendRecieveUtil.SendBytes(client, dat, args);


                        SendRecieveUtil.RecieveBytes(client, ref overread, args, new List<RetrievalNode>(){new RetrievalNode()
                        {

                            direct = (x) =>//length
                            {
                                Console.WriteLine("Length=" + x.GetDecodedString());
                            }
                        },
                         });
                        
                        SendRecieveUtil.RecieveBytes(client, ref overread, args, new List<RetrievalNode>(){new RetrievalNode()
                        {

                            direct=cm.completed, motive="message"
                        } }) ;
                        

                        
                    }
                }
            });
            client_act = act;
            return act;
        }
    }
}
