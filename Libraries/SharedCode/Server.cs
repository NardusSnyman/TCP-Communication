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

                    NetworkData output = null;
                    Command comm = new Command();
                    long length = 0;
                    SendRecieveUtil.RecieveBytes(client, ref overread, args, new List<RetrievalNode>(){
                    new RetrievalNode(){direct = (x) =>//length
                        {
                            length = Convert.ToInt64(x.GetDecodedString());
                            
                        }, motive="length"
                    },
                    new RetrievalNode(){direct = (x) =>//operation
                    {
                            string operation = x.GetDecodedString();
                            foreach (var command in commands)
                            {
                                if (command.operation == operation)
                                {
                                    comm = command;
                                }
                            }
                        }, motive="operation"
                    },
                    new RetrievalNode(){direct = (x) =>//message
                        {
                            output = comm.action?.Invoke(x);
                        }, motive="message"} });
                    
                    SendRecieveUtil.SendBytes(client, output, args);
                    
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
