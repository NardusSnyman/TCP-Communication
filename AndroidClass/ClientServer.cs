using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Runtime;
using System.Threading.Tasks;
using static ClientServer.SendRecieve;

namespace ClientServer
{
    public class Server
    {
        public ConnectionArguments args;
        public Server()
        {

        }
        public Server(ConnectionArguments args1, Action<string> debug = null)
        {
            args = args1;
        }
        public Action<string, int> debug;
        public TcpListener listener;
        public List<Command> commands;
        private Task task;
        MemoryStream overread;
        public void Start()
        {
            listener = new TcpListener(IPAddress.Any, args.port);
                listener.Start();
            
            if (debug == null)
            {
                debug = new Action<string, int>((string e, int a)=>{
                });
            }
            debug.Invoke("listener started", 1);
            task = new Task(new Action(() =>
            {

                while (1 == 1)
                {
                    TcpClient client = listener.AcceptTcpClient();
                    debug.Invoke("connected", 1);
                    var bytes = RecieveBytes(client, (z)=> { debug.Invoke("Packet:"+z, 1); }, args.ender, ref overread, args.buffer_size);

                    debug.Invoke("message recieved-" + bytes.data.Length, 1);
                    ClientMessage cm = new ClientMessage(bytes, args.separator);
                    ServerMessage returnmsg = null;
                    debug.Invoke("commands commencing-" + cm.operation, 1);
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

                    var output = returnmsg.Bytes(args.separator);
                    debug.Invoke("sending bytes-" + output.data.Length, 1);
                    SendBytes(client, output, (z) => { debug.Invoke("Packet:" + z, 1); }, args.ender, args.buffer_size);
                    client.Close();
                    debug.Invoke("client closed", 1);
                }
                
            }));
            task.Start();
            debug.Invoke("task started", 1);
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
        MemoryStream overread;
        public ServerMessage Communicate(ClientMessage cm)
        {
            if (debug == null)
            {
                debug = new Action<string, int>((p, q) =>
                {

                });
            }
            TcpClient client = new TcpClient();
                client.Connect(args.ip, args.port);
                while (!client.Connected)
                {

                }

            debug.Invoke("connected", 1);
            
            var input = cm.Bytes(args.separator);
            debug.Invoke("sending bytes-" + input.String(), 1);
            SendBytes(client, input, (z) => { debug.Invoke("Packet:" + z, 1); }, args.ender, args.buffer_size);

            
            var bytes = RecieveBytes(client, (z) => { debug.Invoke("Packet:" + z, 1); }, args.ender, ref overread, args.buffer_size);
            debug.Invoke("recieving bytes-" + bytes.data.Length, 1);
            debug.Invoke("closing connection", 1);
                client.Close();



            return new ServerMessage(bytes, args.separator);
        }


        public Client(ConnectionArguments args1)
        {
            args = args1;
        }
    }
}
