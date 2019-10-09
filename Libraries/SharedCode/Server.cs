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
        NetworkData overread = new NetworkData();//data storage for overread data

        public void Start()
        {
            ///////

            listener = new TcpListener(IPAddress.Any, args.port);
            listener.Start();

            task = new Task(new Action(() =>
            {

                while (1 == 1)
                {
                    TcpClient client = listener.AcceptTcpClient();//accept new client
                    Command comm = new Command();
                    long length = 0;
                    Console.WriteLine("connected client");
                    SendRecieveUtil.RecieveBytes(client, ref overread, args, new List<RetrievalNode>(){
                    new RetrievalNode(){direct = (x) =>//length
                        {
                            length = Convert.ToInt64(x.GetDecodedString());

                        }, motive="length"
                    },
                    new RetrievalNode(){direct = (x) =>//operation
                    {
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
                            Console.WriteLine("no command found by the name of " + operation);
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
