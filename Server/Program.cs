using System;
using System.IO;
using System.Text;
using ClientServer;

namespace Server
{
    class Program
    {

        static void Main(string[] args)
        {
            byte[] data = File.ReadAllBytes(Directory.GetCurrentDirectory() + @"\text.txt");
            ClientServer.Server server = new ClientServer.Server(998);
            server.debug = new Action<string, int>((string e, int sum)=>
            {
                Console.WriteLine(e);
            });
            var cmd1 = new Command();
            cmd1.operation = "repeat";
            cmd1.action = new Func<ClientMessage, ServerMessage>((o) =>
            {
                return new ServerMessage("xx:" + o.message, true);
            });
            server.commands = new System.Collections.Generic.List<Command>() { cmd1 };
            server.Start();
            Console.ReadLine();
        }
    }
}
