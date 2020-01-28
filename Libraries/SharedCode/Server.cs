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
using System.Diagnostics;
using System.Linq;

namespace ClientServer
{
    public class Server
    {
        //INITIALIZATION
        public Server()
        {

        }
        public Server(ConnectionArguments args1, ThreadingUtil util = null)
        {
            args = args1;
            if (this.threadingUtil != null)
                this.threadingUtil = util;
            else
                this.threadingUtil = new ThreadingUtilDef();
        }
        //-----------------
        public ConnectionArguments args;//connection arguments

        public List<TcpListener> listeners;//server main listener
        public List<Command> commands = new List<Command>();//server commands
        public Action<string, int> debug;//message and level  1=surface, 2=base events, 3=debug data
        private ThreadingUtil threadingUtil;
        NetworkData overread = new NetworkData();//data storage for overread data

        private int stop = 0;
        public void Restart()
        {
            stop = 1;
            Start();
        }

        public void Start()
        {
            ///////
            foreach (int port in args.ports)
            {
                threadingUtil.BackgroundTask(()=>
                {
                    if (debug == null)
                        debug = new Action<string, int>((x, y) => { });
                    TcpListener listener = new TcpListener(IPAddress.Any, port);
                    listener.Start();


                    debug("[SYS]: started server with port " + port, 1);

                    while (stop == 0)
                    {
                        debug($"[{port}]: waiting", 1);
                        while (!listener.Pending())
                        {

                        }
                            ConnectToClient(listener, port);
 


                    }
                    stop = 0;
                    listener.Stop();
                });
            }
        }//start server

        private void ConnectToClient(TcpListener listener, int port)
        {
            try
            {
                ConnectClient cli = (ConnectClient)listener.AcceptTcpClient();//accept new client

                debug($"[{port}]: Client Connected", 0);
                int count = 0;

                
                    Command comm = new Command();
                    long length = 0;
                    debug($"[{port}]: wait for read", 1);


                    SendRecieveUtil.RecieveBytes(cli, ref overread, args, 0, null, debug, 
                        new List<RetrievalNode>(){
                                new RetrievalNode(){direct = (x) =>//length
                            {
                            try{
                            length = Convert.ToInt64(x.GetDecodedString());
                            }catch(Exception e)
                            {

                            }
                                if(length == 0)
                                {
                                    count++;
                                }else
                                    count = 0;
                                
                                debug($"[{port}]: length={length}", 3);
                        }, motive="length"
                    },
                    new RetrievalNode(){direct = (x) =>//operation
                    {
                        if(x!=null){
                        debug(x.GetDecodedString(), 3);
                            string operation = x.GetDecodedString();
                        bool found = false;
                            foreach (var command in commands)
                            {
                                if (command.operation == operation)
                                {
                                    comm = command;
                                    found = true;
                                     debug($"[{port}]: operation={operation}", 3);
                                }
                            }
                            if(!found)
                            debug("no command found by the name of " + operation, 2);
                        }
                        }, motive="operation"
                            },
                    new RetrievalNode(){direct = (x) =>//message
                        {
                            if(x!=null){
                                debug($"[{port}]: data processed and sent", 1);
                                debug($"[{port}]: message recieved", 2);
                            var output = comm.action(x);
                            SendRecieveUtil.SendBytes(cli, output, args, debug);
                            }
                        }, motive="message"}
                        });
                
                debug($"[{port}]: Client closed connection", 0);

            }
            catch(Exception e)
            {
                debug($"[{port}]: {e.Message}", 4);
            }

        }

        public void Stop()//stop server
        {
            stop = 1;
        }
    }
}
