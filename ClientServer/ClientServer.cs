using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using static ClientServer.Utilities;

namespace ClientServer
{
    public class Server
    {
        public int port = 0;
        public Server()
        {

        }
        public Server(int port1, Action<string> debug = null)
        {
            port = port1;
        }
        public Action<string, int> debug;
        public TcpListener listener;
        public List<Command> commands;
        private Task task;
        public void Start()
        {
            if (debug == null)
            {
                debug = new Action<string, int>((string e, int a)=>{
                });
            }
            task = new Task(new Action(() =>
            {
            listener = new TcpListener(IPAddress.Any, port);
                listener.Start();
                while (1 == 1)
                {
                    debug.Invoke("waiting for client", 1);
                    TcpClient client = listener.AcceptTcpClient();
                    debug.Invoke("connected", 1);
                    NetworkStream stream = client.GetStream();
                    //recieve size
                    
                    MemoryStream input = new MemoryStream();
                    ReadStream(stream, ref input);
                    debug.Invoke("recieved data", 1);
                    //processing
                    ClientMessage cm = new ClientMessage(input.ToArray());
                    ServerMessage returnmsg = null;
                    for (int x = 0; x < commands.Count && returnmsg == null; x++)
                    {
                        Command command = commands[x];
                        if (command.operation == cm.operation)
                        {
                            returnmsg = command.action.Invoke(cm);
                        }
                    }
                    if (returnmsg == null)
                    {
                        returnmsg = new ServerMessage("", false);
                        debug.Invoke("NO OPERATION FOUND BY THE NAME OF " + cm.operation, 3);
                    }
                    //sending
                    MemoryStream output = ServerMessage.toStream(returnmsg);

                    stream.Write(output.ToArray(), 0, output.ToArray().Length);
                    client.Close();
                    debug.Invoke("sent data", 1);
                    debug.Invoke("client closed", 1);
                }
                
            }));
            task.Start();
            
        }

        

        public void Stop()
        {
            task.Dispose();
        }
    }
    public class Client
    {
        public ConnectionArguments args;
        public Action<string, int> debug;
        public ServerMessage Communicate(ClientMessage cm)
        {
            
            if (debug == null)
            {
                debug = new Action<string, int>((p,q) =>
                {

                });
            }
            
            TcpClient client = new TcpClient();
            client.Connect(args.ip, args.port);
            debug.Invoke("connected", 1);
            NetworkStream stream = client.GetStream();
            MemoryStream input = ClientMessage.toStream(cm);
            stream.Write(input.ToArray(), 0, input.ToArray().Length);
            debug.Invoke("sent data", 1);

            MemoryStream output = new MemoryStream();
            //recieve size
            ReadStream(stream, ref output);
            debug.Invoke("recieved data", 1);
            debug.Invoke("closing connection", 1);
            return new ServerMessage(output.ToArray());
        }
        public Client(ConnectionArguments args1)
        {
            args = args1;
        }
    }
}
