using ClientServer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ClientServer.EncodingClasses;
using static ClientServer.EncodingClasses.NetworkData;

namespace ConsoleServer
{
    class Program
    {
        static void Main(string[] args)
        {
            Server server = new Server(ConnectionArguments.fromLocal(998, 2048));
            server.commands = new List<Command>() {
                new Command() {
                operation = "repeat", action= (NetworkData info)=>{
                    Console.WriteLine("repeat");
                    
                    return info;
                } }
            };
            
            Console.WriteLine(server.args.ip + ":" + server.args.port);
            Console.WriteLine("server online");
            server.Start();
            while (1 == 1)
            {

            }
        }
    }
}
