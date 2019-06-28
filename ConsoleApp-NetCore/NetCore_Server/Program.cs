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
            ClientServer.Server server = new ClientServer.Server(ConnectionArguments.fromLocal(998, 1024));
            server.commands = new System.Collections.Generic.List<Command>()
            {
                new Command("repeat", (string msg) =>
                {
                    return msg + " (ECHO ECHo ECho Echo echo)";
                })
            };
            server.debug += (string mess) =>
            {
                Console.WriteLine(mess);
            };
            server.Start();
            Console.WriteLine("console started");
            Console.ReadLine();
        }
    }
}
