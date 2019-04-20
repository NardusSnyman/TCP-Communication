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
        private byte[] overread;
        public byte ender = networkEncoding.GetBytes("@")[0];
        public void Start()
        {
            listener = new TcpListener(IPAddress.Any, port);
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
                    var bytes = RecieveBytes(client, ref overread, ender, null);
                    debug.Invoke("message recieved-" + bytes.data.Length, 1);
                    ClientMessage cm = new ClientMessage(bytes);
                    ServerMessage returnmsg = null;
                    debug.Invoke("commands commencing", 1);
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
                    debug.Invoke("get bytes", 1);
                    var output = returnmsg.Bytes();
                    debug.Invoke("sending bytes", 1);
                    SendBytes(client, output, ender, null);
                    client.Close();
                    debug.Invoke("sent data-" + output.data.Length, 1);
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
        public byte ender = networkEncoding.GetBytes("@")[0];
        byte[] overread;
        public static string separator1 = ".:.";
        public ServerMessage Communicate(ClientMessage cm, Action<long> sendProg = null, Action<long> recieveProg = null)
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
            
            debug.Invoke("bytes-" + cm.Bytes().data.Length, 1);
            var input = cm.Bytes();
            debug.Invoke("sending bytes", 1);
            SendBytes(client, input, ender, sendProg);

            debug.Invoke("recieving bytes", 1);
            var output = RecieveBytes(client, ref overread, ender, recieveProg);

            debug.Invoke("recieved data-" + output.data.Length, 1);
            debug.Invoke("closing connection", 1);
            
            return new ServerMessage(output);
        }


        public Client(ConnectionArguments args1)
        {
            args = args1;
        }
    }
}
