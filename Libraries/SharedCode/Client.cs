using System;
using System.Collections.Generic;
using System.Net.Sockets;
using static ClientServer.EncodingClasses;
using System.Threading;
using System.Linq;
using System.Threading.Tasks;

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

        List<ConnectClient> clients = new List<ConnectClient>();
        public void clientThread(out Action start)
        {
            for (int x = 0; x < args.ports.Count; x++)
            {
                debug($"[SYS]: initialized port {args.ports[x]}", 1);
                clients.Add(new ConnectClient() { port=args.ports[x], ip=args.ip });
            }

            start = new Action(() =>
            {
                new Task(() =>
                {
                    while (true)
                    {
                        if (command_queue.Count > 0)
                        {
                            debug($"[SYS]: command recieved", 3);
                            for (int i = 0; i < clients.Count; i++)
                            {
                                if (command_queue.Count == 0)
                                    break;
                                
                                ConnectClient c = clients[i];
                                debug($"[SYS]: port {c.port} available", 3);
                                if (c.Connected)
                                {
                                    debug($"[{c.port}]: busy", 2);
                                    continue;
                                }
                                
                                  
                                    debug($"[{c.port}]: checking ({command_queue.Count})", 0);
                                    ClientMessage cm = command_queue.Dequeue();//get args
                                    debug($"[{c.port}]: data pulled from queue", 0);

                                var t = new Task(() =>
                                {
                                    try
                                    {
                                        c.Connect(args.ip, c.port);//connect to server
                                        debug($"[SYS]: port {c.port} connected", 2);
                                    }
                                    catch (SocketException e)
                                    {
                                        debug($"[SYS]: server is not available", 4);
                                    }

                                    string msg = "";
                                    if (cm.message != null)
                                    {
                                        msg = cm.message.GetDecodedString();//initialize message
                                    }
                                    NetworkData dat = NetworkData.fromEncodedString(String.Join(SendRecieveUtil.separator,
                                        new List<string>() { cm.operation, msg }.Select(x => NetworkData.fromDecodedString(x).GetEncodedString())));
                                    debug($"[{c.port}]: Sending bytes", 1);
                                    SendRecieveUtil.SendBytes(c, dat, args, debug);//send bytes

                                    int length = 0;
                                    SendRecieveUtil.RecieveBytes(c, ref overread, args, 0, null, debug, new List<RetrievalNode>() { new RetrievalNode()
                            {

                                direct = (x) =>//length
                                {
                                    if (x != null && x.GetDecodedString() != "null")
                                        length = int.Parse(x.GetDecodedString());
                                    else
                                        
                                    debug($"[{c.port}]: length={length}", 3);
                                }, motive = "length"
                            },
                                });//recieve bytes and get length
                                    SendRecieveUtil.RecieveBytes(c, ref overread, args, length, cm.progress, debug, new List<RetrievalNode>() { new RetrievalNode()
                            {

                                direct = (x) => { cm.completed(x); c.Close(); debug($"[{c.port}]: Client closed", 0); debug($"[{c.port}]: data={x.GetDecodedString()}", 3); }, motive = "message"
                            } });//get final data and close client


                                });
                                t.Start();
                            }
                        }

                    }
                }).Start();


            });

        }
    }
}
