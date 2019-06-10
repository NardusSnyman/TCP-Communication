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

namespace ClientServer
{
    public class Server
    {
        //INITIALIZATION
        public Server()
        {

        }
        public Server(ConnectionArguments args1, Action<TcpClient> clientConnected = null, Action<string> debug = null)
        {
            args = args1;
            this.clientConnected = clientConnected;
        }
        //-----------------
        public ConnectionArguments args;//connection arguments
        
        public Action<string, int> debug;//debugging invocation
        public TcpListener listener;//server main listener
        public List<Command> commands = new List<Command>();//server commands
        Action<TcpClient> clientConnected;
        private Task task;//server task
        MemoryStream overread;//data storage for overread data
        public void Start()
        {
            listener = new TcpListener(IPAddress.Any, args.port);
                listener.Start();
            
            if (debug == null)
            {
                debug = new Action<string, int>((string e, int a)=>{//int a-states  1=Main Debugger 2=error debugger 3=extra detail debugger
                });
            }
            debug.Invoke("listener started", 1);
            task = new Task(new Action(() =>
            {

                while (1 == 1)
                {
                    TcpClient client = listener.AcceptTcpClient();//accept new client
                    if (clientConnected != null)
                        clientConnected(client);
                    debug.Invoke("connected", 1);
                    //recieve packets until "arg.ender" final char is read, write packet count, and return bytes not part of operation
                    var bytes = SendRecieveUtil.RecieveBytes(client, (z, b)=> { debug.Invoke("Packet:"+z, 3); }, args.ender, ref overread, args.buffer_size);

                    debug.Invoke("message recieved-" + bytes.data.Length, 1);

                    ClientMessage cm = new ClientMessage(bytes, args.separator);//create a new client message with byte data
                    ServerMessage returnmsg = null;
                    debug.Invoke("commands commencing-" + cm.operation, 1);
                    for (int x = 0; x < commands.Count && returnmsg == null; x++)//find command with operation name
                    {
                        Command command = commands[x];
                        if (command.operation == cm.operation)
                        {
                            debug.Invoke($"Operation found", 3);
                            returnmsg = command.action.Invoke(cm);//return server message
                        }
                        else debug.Invoke($"{command.operation} != {cm.operation}", 3);
                    }
                    if (returnmsg == null)//command does not exist
                    {
                        returnmsg = new ServerMessage("", false);
                        debug.Invoke("NO OPERATION FOUND BY THE NAME OF " + cm.operation, 2);
                    }

                    var output = returnmsg.Bytes(args.separator);//get bytes of server message
                    debug.Invoke("sending bytes-" + output.data.Length, 1);
                    //send bytes, debug packet count, and add an ender character
                    SendRecieveUtil.SendBytes(client, output, (z) => { debug.Invoke("Packet:" + z, 3); }, args.ender, args.buffer_size);
                    client.Close();
                    debug.Invoke("client closed", 1);
                }
                
            }));
            task.Start();
            debug.Invoke("task started", 1);
        }//start server

        

        public void Stop()//stop server
        {
            task.Dispose();
        }
    }
    public class Client
    {
        public ConnectionArguments args;//connection arguments
        public Action<string, int> debug;//debugging invocation
        MemoryStream overread;//data storage for overread data
        //INITIALIZATION
        public Client(ConnectionArguments args1)
        {
            args = args1;
        }
        //-----------------
        public ServerMessage Communicate(ClientMessage cm)
        {
            if (debug == null)
            {
                debug = new Action<string, int>((p, q) =>//int a-states  1=Main Debugger 2=error debugger 3=extra detail debugger
                {

                });
            }
            TcpClient client = new TcpClient();
            start:
            try
            {
                client.Connect(args.ip, args.port);
            }
            catch (Exception e)
            {
                Thread.Sleep(1200);
                debug.Invoke("attempting connection... || " + e.Message, 2);
                goto start;
            }
            debug.Invoke("connected", 1);
            //get bytes of client message
            var input = cm.Bytes(args.separator);
            debug.Invoke("sending bytes-" + input.String(), 1);
            //send bytes, debug packet count, and add an ender character
            SendRecieveUtil.SendBytes(client, input, (z) => { debug.Invoke("Packet:" + z, 3); }, args.ender, args.buffer_size);

            //recieve packets until "arg.ender" final char is read, write packet count, and return bytes not part of operation
            var bytes = SendRecieveUtil.RecieveBytes(client, (z, b) => { debug.Invoke("Packet:" + z, 3); }, args.ender, ref overread, args.buffer_size);
            debug.Invoke("recieving bytes-" + bytes.data.Length, 1);
            debug.Invoke("closing connection", 1);
                client.Close();



            return new ServerMessage(bytes, args.separator);
        }//main form of communication

        public void PregressiveDataStream(ClientMessage cm, Action<BaseEncode> read)
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
            //get bytes of client message
            var input = cm.Bytes(args.separator);
            debug.Invoke("sending bytes-" + input.String(), 1);
            //send bytes, debug packet count, and add an ender character
            SendRecieveUtil.SendBytes(client, input, (z) => { debug.Invoke("Packet:" + z, 3); }, args.ender, args.buffer_size);

            //recieve packets until "arg.ender" final char is read, write packet count, and return bytes not part of operation
            var bytes = SendRecieveUtil.RecieveBytes(client, (z, b) => { debug.Invoke("Packet:" + z, 3); read(b); }, args.ender, ref overread, args.buffer_size);
            debug.Invoke("recieving bytes-" + bytes.data.Length, 1);
            debug.Invoke("closing connection", 1);
            client.Close();


        }//main form of communication
    }
}
