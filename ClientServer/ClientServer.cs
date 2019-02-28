using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

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
        public Action<string> debug;
        public TcpListener listener;
        public List<Command> commands;
        private Task task;
        public void Start()
        {
            if (debug == null)
            {
                debug = new Action<string>((string e)=>{
                });
            }
            task = new Task(new Action(() =>
            {
            listener = new TcpListener(IPAddress.Any, port);
                listener.Start();
                while (1 == 1)
                {
                    debug.Invoke("waiting for client");
                    TcpClient client = listener.AcceptTcpClient();
                    debug.Invoke("client recieved");
                    NetworkStream stream = client.GetStream();

                    byte[] buffer = new byte[client.ReceiveBufferSize];
                    int bytesRead = stream.Read(buffer, 0, client.ReceiveBufferSize);
                    MemoryStream input = new MemoryStream(buffer, 0, bytesRead);
                    debug.Invoke("input=" + Encoding.ASCII.GetString(input.ToArray()));
                    ClientMessage cm = new ClientMessage(input);
                    debug.Invoke("recieved bulk");
                    ServerMessage returnmsg = null;
                    for (int x = 0; x < commands.Count && returnmsg == null; x++)
                    {
                        Command command = commands[x];
                        if (command.operation == cm.operation)
                        {
                            returnmsg = command.action.Invoke(cm);
                        }
                    }
                    debug.Invoke($"completed operation [{cm.operation}]");
                    debug.Invoke("output=" + returnmsg.message + "-" + Encoding.ASCII.GetString(returnmsg.messagearray));
                    MemoryStream output = ServerMessage.toStream(returnmsg);
                    stream.Write(output.ToArray(), 0, output.ToArray().Length);
                    client.Close();
                    debug.Invoke("client closed");
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
        public Action<string> debug;
        public ServerMessage Communicate(ClientMessage cm)
        {
            if(debug == null)
            {
                debug = new Action<string>((p) =>
                {

                });
            }

            TcpClient client = new TcpClient();
            client.Connect(args.ip, args.port);
            debug.Invoke("connected");
            NetworkStream stream = client.GetStream();
            MemoryStream input = ClientMessage.toStream(cm);
            debug.Invoke("data=" + Encoding.ASCII.GetString(input.ToArray()));
            stream.Write(input.ToArray(), 0, input.ToArray().Length);
            debug.Invoke("sent data");

            byte[] buffer = new byte[client.ReceiveBufferSize];
            int bytesRead = stream.Read(buffer, 0, client.ReceiveBufferSize);
            debug.Invoke("recieved buffer");
            MemoryStream output = new MemoryStream(buffer, 0, bytesRead);
            debug.Invoke("recieved data");
            return new ServerMessage(output);
        }
        public Client(ConnectionArguments args1)
        {
            args = args1;
        }
    }
}
