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
            Server server = new Server(ConnectionArguments.fromLocal(998, 256));
            server.debug = new ExtendedDebug()
            {
                mainProcessDebug = (x)=>{ Console.WriteLine(x); }, closeUpDebug = (x) => { Console.WriteLine(x); }
            };
            server.commands = new List<Command>() { new Command() {
                operation = "repeat", action= (Tuple<NetworkData, string> info)=>{

                    Console.WriteLine(info.Item2);
                    return NetworkData.fromDecodedString(info.Item1.GetDecodedString() + "REPEATED");
                }
            } };
            Console.WriteLine(server.args.ip + ":" + server.args.port);
            server.Start();
            while (1 == 1)
            {

            }
        }
    }
}
