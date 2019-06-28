using ClientServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ClientServer.EncodingClasses;

namespace Client
{
    class Program
    {
        static void Main(string[] args)
        {
            var arguments = ConnectionArguments.fromLocal(998, 1024);
            Console.WriteLine($"ip: {arguments.ip}");
            Console.WriteLine($"port: {arguments.port}");
            ClientServer.Client client = new ClientServer.Client(arguments);
            client.debug += (string mess) =>
            {
                Console.WriteLine(mess);
            };
            string uni = Console.ReadLine();
            var bas = new BaseEncode(uni);
            Console.WriteLine(bas.GetNetworkEncoding().GetRawStringWithSeparator(" "));
            Console.WriteLine(bas.GetNetworkEncoding().GetOriginalStringWithSeparator(" "));
            Console.WriteLine(bas.GetNetworkEncoding().GetBaseEncode().GetString());
            Console.ReadKey();
            Console.WriteLine($"Communiating...");
            var msg = client.Communicate("repeat", "hello");
            Console.WriteLine(msg);
            Console.ReadLine();
        }
    }
}
