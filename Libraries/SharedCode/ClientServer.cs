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


                    //-----
                    debug.mainProcessDebug?.Invoke("waiting for client");
                    debug.closeUpDebug?.Invoke("waiting for client");
                    TcpClient client = listener.AcceptTcpClient();//accept new client
                    debug.mainProcessDebug?.Invoke("client connected");
                    debug.closeUpDebug?.Invoke("attempting read...");

                    NetworkData output = null;
                    Command comm = new Command();
                    string ip = "";
                    long length = 0;
                    SendRecieveUtil.RecieveBytes(client, ref overread, args, new List<RetrievalNode>(){
                    new RetrievalNode()
                    {
                        direct = (x) =>//length
                        {
                            length = Convert.ToInt64(x.GetDecodedString());
                        }, motive="length"
                    },new RetrievalNode()
                    {
                        direct = (x) =>//ip
                        {
                            ip=x.GetDecodedString();
                        }, motive="ip"
                    },
                        new RetrievalNode()
                    {
                        direct = (x) =>//operation
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
                    }, new RetrievalNode(){
                         direct = (x) =>//message
                        {
                            output = comm.action?.Invoke(new Tuple<NetworkData, string>(x, ip));
                        }, motive="message"
                    } });
                    
                    if(output == null)
                        debug.errorDebug?.Invoke("command not found");
                    
                    SendRecieveUtil.SendBytes(client, output, args, new Tuple<ExtendedDebug, ExtendedDebug>(debug, new ExtendedDebug()));
                    debug.mainProcessDebug?.Invoke("client disconnected");
                    
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
        NetworkData overread;//data storage for overread data
        public ExtendedDebug universalDebug = new ExtendedDebug();
        public Queue<ClientMessage> command_queue = new Queue<ClientMessage>();
        public Client(ConnectionArguments args1, ExtendedDebug debug = null)
        {
            args = args1;
            universalDebug = debug;
        }
        //-----------------
        int temp = 0;
        public void Communicate(string operation, string message, Action<NetworkData> onCompleted, Action failed = null, ExtendedDebug debug = null)
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
            universalDebug.closeUpDebug?.Invoke("Enqueueing commmands");
         }

        public Action clientThread()
        {
            Action act = new Action(()=>
            {
                while (true)
                {
                    if (command_queue.Count > 0)
                    {
                        

                        TcpClient client = new TcpClient();
                        
                        ClientMessage cm = command_queue.Dequeue();
                        ExtendedDebug debug = cm.debug;
                    start:
                       
                    
                        try
                        {
                            client.Connect(args.ip, args.port);
                        }
                        catch (Exception e)
                        {
                            Thread.Sleep(1200);
                            temp++;
                            if (temp > args.server_reconnect_attempts)
                            {
                                temp = 0;
                                cm.failed?.Invoke();
                                client.Close();
                                return;
                            }

                            goto start;
                        }
                        string externalip = new WebClient().DownloadString("http://icanhazip.com");
                        NetworkData dat = NetworkData.fromEncodedString(String.Join(SendRecieveUtil.separator, new List<string>() { externalip, cm.operation, cm.message }.Select(x=>NetworkData.fromDecodedString(x).GetEncodedString())));
                        dat.InitStream();
                        SendRecieveUtil.SendBytes(client, dat, args, new Tuple<ExtendedDebug, ExtendedDebug>(debug, universalDebug));

                        NetworkData data = null;
                        SendRecieveUtil.RecieveBytes(client, ref overread, args, new List<RetrievalNode>(){new RetrievalNode()
                        {

                            direct = (x) =>//length
                            {
                                Console.WriteLine(x.GetDecodedString());
                            }
                        },
                        new RetrievalNode()
                        {

                            direct = (x) =>
                            {
                                data = x;
                            }
                        } });
                        

                        cm.completed?.Invoke(data);

                        client.Close();
                    }
                }
            });
            client_act = act;
            return act;
        }
        
       
    }
}
