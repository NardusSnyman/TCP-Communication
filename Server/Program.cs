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
            server.debug = new Action<string, int>((string e, int sum)=>
            {
                Console.WriteLine(e);
            });
            var cmd = new Command();
            cmd.operation = "run";
            cmd.action = new Func<ClientMessage, ServerMessage>((o) =>
            {
                return new ServerMessage("ok", true);
            });
            server.commands = new System.Collections.Generic.List<Command>() { cmd };
            server.Start();
            Console.ReadLine();
        }
    }
}
