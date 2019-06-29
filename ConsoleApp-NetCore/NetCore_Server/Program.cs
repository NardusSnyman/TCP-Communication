using System;
using System.IO;
using System.Text;
using ClientServer;
using static ClientServer.EncodingClasses;

namespace Server
{
    class Program
    {

        static void Main(string[] args)
        {
            ClientServer.Server server = new ClientServer.Server(ConnectionArguments.fromLocal(998, 2048, TimeSpan.FromSeconds(5), TimeSpan.FromMilliseconds(500)));
            server.commands = new System.Collections.Generic.List<Command>()
            {
                new Command("repeat", (BaseEncode msg) =>
                {
                    char byt = (char)msg.GetBytes()[1];
                    Console.WriteLine(msg.GetString());
                    return msg;
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
