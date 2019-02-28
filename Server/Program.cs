using System;
using System.Text;
using ClientServer;

namespace Server
{
    class Program
    {

        static void Main(string[] args)
        {
            
            ClientServer.Server server = new ClientServer.Server(998);
            server.debug = new Action<string>((string e)=>
            {
                Console.WriteLine(e);
            });
            var cmd = new Command();
            cmd.operation = "run";
            cmd.action = new Func<ClientMessage, ServerMessage>((o) =>
            {
                Console.WriteLine(Encoding.ASCII.GetString(o.messagearray));
                return new ServerMessage("ok", true, new byte[0]);
            });
            server.commands = new System.Collections.Generic.List<Command>() { cmd };
            server.Start();
            Console.ReadLine();
        }
    }
}
