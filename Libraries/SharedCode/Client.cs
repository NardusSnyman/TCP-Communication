using System;
using System.Collections.Generic;
using System.Net.Sockets;
using static ClientServer.EncodingClasses;
using System.Threading;
using System.Linq;

namespace ClientServer
{
    public class Client
    {

        public ConnectionArguments args;//connection arguments
        NetworkData overread;//data storage for overread data
        public Queue<ClientMessage> command_queue = new Queue<ClientMessage>();
        public Action<string, int> debug;//message and level  1=surface, 2=base events, 3=debug data
        public Client(ConnectionArguments args1)
        {
            args = args1;
            if (debug == null) debug = new Action<string, int>((x, y) => { });
        }
        //-----------------
        public void Communicate(string operation, NetworkData message, Action<NetworkData> onCompleted, Action failed = null)
        {
            ClientMessage cm = new ClientMessage();
            cm.failed = failed;
            cm.completed = onCompleted;
            cm.operation = operation;
            cm.message = message;
            command_queue.Enqueue(cm);
            debug("[SYS]: added operation to queue", 1);
        }

        List<TcpClient> clients = new List<TcpClient>();
        public void clientThread(out Action start)
        {
            for (int x = 0; x < args.ports.Count; x++)
            {
                debug($"[SYS]: initialized port {args.ports[x]}", 1);
                clients.Add(new TcpClient());
            }

            start = new Action(() =>
            {

                //try connecting


                while (true)
                {


                    if (command_queue.Count > 0)
                    {

                        for (int i = 0; i < clients.Count; i++)
                        {
                            if (command_queue.Count == 0)
                                break;
                                int port = args.ports[i];
                            TcpClient client = clients[i];
                            if (client.Connected)
                            {
                                debug($"[{port}]: busy", 2);
                                continue;
                            }
                            client.Connect(args.ip, args.ports[i]);//connect to server
                            ClientMessage cm = command_queue.Dequeue();//get args
                            debug($"[{port}]: communication pulled", 1);

                            string msg = "";
                            if (cm.message != null)
                            {
                                msg = cm.message.GetDecodedString();//initialize message
                            }
                            NetworkData dat = NetworkData.fromEncodedString(String.Join(SendRecieveUtil.separator,
                                new List<string>() { cm.operation, msg }.Select(x => NetworkData.fromDecodedString(x).GetEncodedString())));
                            dat.InitStream();//initialize encoded data

                            SendRecieveUtil.SendBytes(client, dat, args, port);//send bytes
                            debug($"[{port}]: Sending bytes", 1);
                            int length = 0;
                            SendRecieveUtil.RecieveBytes(client, ref overread, args, 0, null, new List<RetrievalNode>(){new RetrievalNode()
                            {

                            direct = (x) =>//length
                            {
                                if(x != null)
                                length = int.Parse(x.GetDecodedString());
                                debug($"[{port}]: length={length}", 3);
                            }
                            },
                            });//recieve bytes and get length

                            SendRecieveUtil.RecieveBytes(client, ref overread, args, length, cm.progress, new List<RetrievalNode>(){new RetrievalNode()
                            {

                            direct=(x)=>{ cm.completed(x); client.Close(); debug($"[{port}]: Client closed", 1); debug($"[{port}]: data={x.GetDecodedString()}", 3); }, motive="message"
                            } });//get final data and close client


                        }
                    }

                }
                
            });

        }
    }
}
