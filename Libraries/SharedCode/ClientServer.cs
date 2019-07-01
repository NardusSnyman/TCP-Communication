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
        NetworkEncoding overread;//data storage for overread data
        public ExtendedDebug debug = new ExtendedDebug();

        public void Start()
        {
            ///////

            listener = new TcpListener(IPAddress.Any, args.port);
                listener.Start();
            debug.mainProcessDebug?.Invoke("listener started");

            task = new Task(new Action(() =>
            {

                while (1 == 1)
                {
                    Start:
                    
                    ///-----
                    TcpClient client = listener.AcceptTcpClient();//accept new client
                    debug.mainProcessDebug?.Invoke("client connected");
                    debug.closeUpDebug?.Invoke("attempting read...");
                    var bytes = SendRecieveUtil.RecieveBytes(client, ref overread, args, new Tuple<ExtendedDebug, ExtendedDebug>(debug, new ExtendedDebug()));
                    
                    debug.closeUpDebug?.Invoke("read data. parsing");
                    if (bytes == null)//encoding error
                    {
                        debug.errorDebug?.Invoke("read is null");
                        client.Dispose();
                        goto Start;
                    }
                    debug.closeUpDebug?.Invoke("checking commands list...");
                    var parts = bytes.GetRawString().Split(new string[] { SendRecieveUtil.separator }, StringSplitOptions.None);
                   
                    string operation = new NetworkEncoding(parts[0]).GetBaseEncode().GetString();
                    
                    BaseEncode data =new NetworkEncoding(parts[1]).GetBaseEncode();
                    
                    bool found = false;
                    BaseEncode output = new BaseEncode("NULL");
                    for(int i = 0; i < commands.Count; i++)
                    {
                        Command cmd = commands[i];
                        if (cmd.operation == operation)
                        {
                            output = cmd.action.Invoke(data);
                            found = true;
                        }
                    }
                    if(!found)
                        debug.errorDebug?.Invoke("command not found");
                    SendRecieveUtil.SendBytes(client, output.GetNetworkEncoding(), args, new Tuple<ExtendedDebug, ExtendedDebug>(debug, new ExtendedDebug()));
                    debug.mainProcessDebug?.Invoke("client disconnected");
                    client.Close();
                    
                }
                
            }));
            task.Start();
           
        }//start server

        

        public void Stop()//stop server
        {
            task.Dispose();
        }
    }
    public class Client
    {

        Action client_act;
        public ConnectionArguments args;//connection arguments
        NetworkEncoding overread;//data storage for overread data
        public ExtendedDebug universalDebug = new ExtendedDebug();
        public Queue<ClientMessage> command_queue = new Queue<ClientMessage>();
        public Client(ConnectionArguments args1, ExtendedDebug debug = null)
        {
            args = args1;
            universalDebug = debug;
        }
        //-----------------
        int temp = 0;
        public void Communicate(string operation, string message, Action<string> onCompleted, Action failed = null, ExtendedDebug debug = null)
         {
            ClientMessage cm = new ClientMessage();
            if (debug == null)
                cm.debug = new ExtendedDebug();
            else
                cm.debug = debug;
            cm.failed = failed;
            cm.completed = onCompleted;
            cm.operation = operation;
            cm.message = message;
            command_queue.Enqueue(cm);
            Debug.WriteLine("QUEUED " + cm.operation);
             
         }

        public Action clientThread()
        {
            Action act = new Action(()=>
            {
                while (true)
                {
                    if(command_queue.Count > 0)
                    {
                        TcpClient client = new TcpClient();
                        ClientMessage cm = command_queue.Dequeue();
                        ExtendedDebug debug = cm.debug;
                        Debug.WriteLine("RUNNING " + cm.operation);
                    start:
                        try
                        {
                            client.Connect(args.ip, args.port);
                            debug.mainProcessDebug?.Invoke("connected");
                            universalDebug.mainProcessDebug?.Invoke("connected");
                        }
                        catch (Exception e)
                        {
                            debug.errorDebug?.Invoke("server offline");
                            universalDebug.errorDebug?.Invoke("server offline");
                            Thread.Sleep(1200);
                            temp++;
                            if (temp > args.server_reconnect_attempts)
                            {
                                debug.errorDebug?.Invoke($"attempt {temp + 1} of {args.server_reconnect_attempts}");
                                universalDebug.errorDebug?.Invoke($"attempt {temp + 1} of {args.server_reconnect_attempts}");
                                temp = 0;
                                cm?.failed();
                            }

                            goto start;
                        }
                        debug.closeUpDebug?.Invoke("formatting data");
                        universalDebug.closeUpDebug?.Invoke("formatting data");
                        var input = new NetworkEncoding(new BaseEncode(cm.operation).GetNetworkEncoding().GetRawString() + SendRecieveUtil.separator + new BaseEncode(cm.message).GetNetworkEncoding().GetRawString());
                        SendRecieveUtil.SendBytes(client, input, args, new Tuple<ExtendedDebug, ExtendedDebug>(debug, universalDebug));


                        var data = SendRecieveUtil.RecieveBytes(client, ref overread, args, new Tuple<ExtendedDebug, ExtendedDebug>(debug, universalDebug));
                        debug.closeUpDebug?.Invoke("read data. parsing");
                        universalDebug.closeUpDebug?.Invoke("read data. parsing");
                        debug.mainProcessDebug?.Invoke("client closed");
                        universalDebug.mainProcessDebug?.Invoke("client closed");
                        cm.completed(data.GetBaseEncode().GetString());
                        
                    }
                }
            });
            client_act = act;
            return act;
        }
        
       
    }
}
