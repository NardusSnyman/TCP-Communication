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
            ClientServer.Server server = new ClientServer.Server(new ConnectionArguments("", 998, '@', Convert.ToByte(';'), 1024))
            {
                debug = new Action<string, int>((string e, int sum) =>
                {
                    Console.WriteLine(e);
                })
            };
            var cmd1 = new Command();
            cmd1.operation = "repeat";
            cmd1.action = new Func<ClientMessage, ServerMessage>((o) =>
            {
                Console.WriteLine("XX=" + o.message);
                return new ServerMessage("xx:" + o.message, true);
            });
            server.commands = new System.Collections.Generic.List<Command>() { cmd1 };
            server.Start();
            Console.ReadLine();
        }
    }
}
