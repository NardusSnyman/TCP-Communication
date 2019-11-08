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
        public Action<string> debug;
        public Client(ConnectionArguments args1)
        {
            args = args1;
            if (debug == null) debug = new Action<string>((x) => { });
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
            debug("communicate");
         }

        
        public Action clientThread(Thread thr)
        {
           
            TcpClient client = new TcpClient();
            Action act = new Action(()=>
            {
                while (true)
                {
                    if (command_queue.Count > 0)
                    {
                        

                        
                        
                        ClientMessage cm = command_queue.Dequeue();
                        debug("communicate pulled");
                    start:


                        try
                        {
                            debug("attempting connection");
                            client.Connect(args.ip, args.port);
                        }
                        catch (Exception e)
                        {
                            Thread.Sleep(1200);
                            temp++;
                            debug("failed connection");
                            if (temp > args.server_reconnect_attempts)
                            {
                                temp = 0;
                                cm.failed?.Invoke();
                                client.Close();
                                return;
                            }

                            goto start;
                        }
                        debug("connection complete");
                        string msg = "";
                        if (cm.message != null)
                        {
                            msg = cm.message.GetDecodedString();
                        }
                        NetworkData dat = NetworkData.fromEncodedString(String.Join(SendRecieveUtil.separator, 
                            new List<string>() { cm.operation, msg  }.Select(x=>NetworkData.fromDecodedString(x).GetEncodedString())));
                        dat.InitStream();
                        SendRecieveUtil.SendBytes(client, dat, args);
                        debug("Sending bytes");
                        int length = 0;
                        SendRecieveUtil.RecieveBytes(client, ref overread, args, thr, 0, null, new List<RetrievalNode>(){new RetrievalNode()
                        {

                            direct = (x) =>//length
                            {
                                length = int.Parse(x.GetDecodedString());
                            }
                        },
                         });
                        
                        SendRecieveUtil.RecieveBytes(client, ref overread, args, thr, length, cm.progress, new List<RetrievalNode>(){new RetrievalNode()
                        {

                            direct=cm.completed, motive="message"
                        } }) ;
                        

                        
                    }
                    System.Threading.Thread.Sleep(1);
                }
            });
            client_act = act;
            return act;
        }
    }
}
