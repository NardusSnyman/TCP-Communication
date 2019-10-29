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
        public Server(ConnectionArguments args1)
        {
            args = args1;
        }
        //-----------------
        public ConnectionArguments args;//connection arguments
        
        public TcpListener listener;//server main listener
        public List<Command> commands = new List<Command>();//server commands
        Task task;//server task
        public Action<string> debug;
        NetworkData overread = new NetworkData();//data storage for overread data

        public void Start()
        {
            ///////
            if (debug == null)
                debug = new Action<string>((x) => { });
            listener = new TcpListener(IPAddress.Any, args.port);
            listener.Start();

            task = new Task(new Action(() =>
            {
                debug("started");
                while (1 == 1)
                {
                debug("waiting");
                    TcpClient client = listener.AcceptTcpClient();//accept new client
                    debug("Client Connected");
                    Command comm = new Command();
                    long length = 0;
                    debug("connected client");
                    SendRecieveUtil.RecieveBytes(client, ref overread, args, new List<RetrievalNode>(){
                    new RetrievalNode(){direct = (x) =>//length
                        {
                            length = Convert.ToInt64(x.GetDecodedString());
                            debug(length.ToString());
                        }, motive="length"
                    },
                    new RetrievalNode(){direct = (x) =>//operation
                    {
                        debug(x.GetDecodedString());
                            string operation = x.GetDecodedString();
                        bool found = false;
                            foreach (var command in commands)
                            {
                                if (command.operation == operation)
                                {
                                    comm = command;
                                found = true;
                                }
                            }
                            if(!found)
                            debug("no command found by the name of " + operation);
                        }, motive="operation"
                    },new RetrievalNode(){direct = (x) =>//message
                        {
                            var output = comm.action(x);
                            SendRecieveUtil.SendBytes(client, output, args);
                        }, motive="message"}
                    });
                }

            }));
            task.ConfigureAwait(false);
            task.Start();

        }//start server

        

        public void Stop()//stop server
        {
            task.Dispose();
        }
    }
}
