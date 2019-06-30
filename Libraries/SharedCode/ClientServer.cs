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
        public Action<string> debug;

        public void Start()
        {
            ///////

            listener = new TcpListener(IPAddress.Any, args.port);
                listener.Start();
            debug?.Invoke("listener started");

            task = new Task(new Action(() =>
            {

                while (1 == 1)
                {
                    Start:
                    
                    ///-----
                    TcpClient client = listener.AcceptTcpClient();//accept new client
                    debug?.Invoke("client connected");
                    debug?.Invoke("attempting read...");
                    var bytes = SendRecieveUtil.RecieveBytes(client, ref overread, args, debug);
                    
                    debug?.Invoke("read data. parsing");
                    if (bytes == null)//encoding error
                    {
                        debug?.Invoke("read is null");
                        client.Dispose();
                        goto Start;
                    }
                    debug?.Invoke("checking commands list...");
                    var parts = bytes.GetRawString().Split(new string[] { SendRecieveUtil.separator }, StringSplitOptions.None);
                    debug?.Invoke("operation...");
                    string operation = new NetworkEncoding(parts[0]).GetBaseEncode().GetString();
                    debug?.Invoke("data...");
                    BaseEncode data =new NetworkEncoding(parts[1]).GetBaseEncode();
                    debug?.Invoke("command=" + operation);
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
                        debug?.Invoke("command not found");
                    debug?.Invoke("sending data");
                    SendRecieveUtil.SendBytes(client, output.GetNetworkEncoding(), args, debug);
                    debug?.Invoke("client disconnected");
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
       
        public ConnectionArguments args;//connection arguments
        NetworkEncoding overread;//data storage for overread data
        public Action<string> debug;
       
        public Client(ConnectionArguments args1)
        {
            args = args1;
        }
        //-----------------
        int attempts = 4;
        int temp = 0;
        public string Communicate(string operation, string message)
         {
            debug?.Invoke(operation + ":" + message);
            TcpClient client = new TcpClient();
             start:
             try
             {
                 client.Connect(args.ip, args.port);
                debug?.Invoke("connected");
             }
             catch (Exception e)
             {
                debug?.Invoke("server offline");
                Thread.Sleep(1200);
                 temp++;
                 if(temp > attempts)
                 {
                     temp = 0;
                    return "";
                 }
                
                 goto start;
             }
            debug?.Invoke("formatting data");
            debug?.Invoke($"--{new BaseEncode(operation).GetNetworkEncoding().GetRawString()} + {SendRecieveUtil.separator} + {new BaseEncode(message).GetNetworkEncoding().GetRawString()}");
            var input = new NetworkEncoding(new BaseEncode(operation).GetNetworkEncoding().GetRawString() + SendRecieveUtil.separator + new BaseEncode(message).GetNetworkEncoding().GetRawString());
            debug?.Invoke("sending data");
            SendRecieveUtil.SendBytes(client, input, args, debug);

            debug?.Invoke("attempting read...");
             var data = SendRecieveUtil.RecieveBytes(client, ref overread, args, debug);
            debug?.Invoke("read data. parsing");
            return data.GetBaseEncode().GetString();
         }//main form of communication*/

       
    }
}
