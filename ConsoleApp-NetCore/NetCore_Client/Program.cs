
using ClientServer;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using static ClientServer.EncodingClasses;

namespace Client
{
    class Program
    {
        static void Main(string[] args)
        {
            var arguments = ConnectionArguments.fromLocal(998, 2048, TimeSpan.FromSeconds(5), TimeSpan.FromMilliseconds(500));
            Console.WriteLine($"ip: {arguments.ip}");
            Console.WriteLine($"port: {arguments.port}");
            ClientServer.Client client = new ClientServer.Client(arguments);
             client.debug += (string mess) =>
             {
                 Console.WriteLine(mess);
             };
            Console.WriteLine($"Communiating...");
            while (1 == 1)
            {
                string data = Console.ReadLine();



                var msg = client.Communicate("repeat", data);
                Console.WriteLine(msg);
            }
            Console.WriteLine("exit");
            Console.ReadLine();
        }
    }
}
