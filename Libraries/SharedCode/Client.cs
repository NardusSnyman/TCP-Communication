using System.Collections.Generic;
using System.Net.Sockets;
using static ClientServer.EncodingClasses;
using System;
using System.Linq;

namespace ClientServer
{
    public class Client
    {

        public ConnectionArguments args;//connection arguments
        NetworkData overread ;//data storage for overread data
        public Queue<ClientMessage> command_queue = new Queue<ClientMessage>();
        public Action<string, int> debug;//message and level  1=surface, 2=base events, 3=debug data
        public ThreadingUtil threadingUtil;
        public Client(ConnectionArguments args1, ThreadingUtil util = null)
        {
            args = args1;
            if (debug == null) debug = new Action<string, int>((x, y) => { });
            if (this.threadingUtil != null)
                this.threadingUtil = util;
            else
                this.threadingUtil = new ThreadingUtilDef();
        }
        //-----------------
        public void Communicate(string operation, NetworkData message, Action<NetworkData> onCompleted, Action failed = null)
        {
            ClientMessage cm = new ClientMessage();
            cm.failed = failed;
            cm.completed = onCompleted;
            cm.operation = operation;
            cm.return_ = true;
            cm.message = message;
            command_queue.Enqueue(cm);
            debug("[SYS]: added operation to queue", 1);
        }
        public void CommunicateNoReturn(string operation, NetworkData message)
        {
            ClientMessage cm = new ClientMessage();
            cm.operation = operation;
            cm.message = message;
            cm.return_ = false;
            command_queue.Enqueue(cm);
            debug("[SYS]: added operation to queue", 1);
        }
        public void CommunicatePackets(string operation, NetworkData message, Action<NetworkData> onPacketRecieved, Action failed = null)
        {
            ClientMessage cm = new ClientMessage();
            cm.failed = failed;
            cm.onPacketRecieved = onPacketRecieved;
            cm.operation = operation;
            cm.return_ = true;
            cm.message = message;
            command_queue.Enqueue(cm);
            debug("[SYS]: added operation to queue", 1);
        }

        List<ConnectClient> clients = new List<ConnectClient> { };
        public void clientThread(out Action start)
        {
            for (int x = 0; x < args.ports.Count; x++)
            {
                debug($"[SYS]: initialized port {args.ports[x]}", 1);
                clients.Add(new ConnectClient() { args=new ConnectionArguments() {buffer_size=args.buffer_size, ip=args.ip, ports=new List<int> { args.ports[x] }, recieve_timeout=args.recieve_timeout, send_timeout=args.send_timeout } });
            }

            start = new Action(() =>
            {
                threadingUtil.BackgroundTask(() =>
                {
                    while (true)
                    {
                        if (command_queue.Count > 0)
                        {
                            debug($"[SYS]: command recieved", 3);
                            for (int i = 0; i < clients.Count; i++)
                            {
                                if (command_queue.Count == 0)
                                {
                                    break;
                                }

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

                                threadingUtil.BackgroundTask(() =>
                                {
                                try
                                {
                                    c.Connect(args.ip, c.port);//connect to server
                                    debug($"[SYS]: port {c.port} connected", 2);
                                }
                                catch (SocketException e)
                                {
                                    debug($"[SYS:726]: {e}", 4);
                                }

                                string msg = "";
                                if (cm.message != null)
                                {
                                    msg = cm.message.GetDecodedString();//initialize message
                                }
                                    string ret = "0";
                                    if (cm.return_)
                                        ret = "1";
                                NetworkData dat = NetworkData.fromEncodedString(String.Join(EncodingClasses.separator,
                                    new List<string>() { ret, cm.operation, msg }.Select(x => NetworkData.fromDecodedString(x).GetEncodedString())));
                                debug($"[{c.port}]: Sending bytes", 1);
                                SendRecieveUtil.SendBytes(c, dat, args, debug);//send bytes

                                    if (cm.return_)
                                    {
                                        int length = 0;
                                        SendRecieveUtil.RecieveBytes(c, ref overread, args, 0, null, debug, new List<RetrievalNode>() {
                                        new RetrievalNode()
                                    {

                                direct = (x) =>//length
                                {
                                    if (x != null && x.GetDecodedString() != "null")
                                        length = int.Parse(x.GetDecodedString());
                                    else

                                    debug($"[{c.port}]: length={length}", 3);
                                }, motive = "length"
                                },
                                        new RetrievalNode()
                                    {

                                direct = (x) => {
                                    threadingUtil.MainThreadTask(()=>{
                                        cm.completed(x);
                                        });
                                    c.Close();
                                    debug($"[{c.port}]: Client closed", 0);
                                    debug($"[{c.port}]: data={x.GetDecodedString()}", 5);
                                }, packetSend=cm.onPacketRecieved, motive = "message"
                                }
                                });//recieve bytes and get length
                                    }
                                });
                            
                            }
                        }

                    }
                });


            });

        }
    }
}
