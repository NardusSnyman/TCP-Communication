
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
            var arguments = ConnectionArguments.fromLocal(998, 3072);
            Console.WriteLine($"ip: {arguments.ip}");
            Console.WriteLine($"port: {arguments.port}");
            ClientServer.Client client = new ClientServer.Client(arguments);
             client.debug += (string mess) =>
             {
                 Console.WriteLine(mess);
             };
            Console.WriteLine($"Communiating...");
            string data = Console.ReadLine();

            
           
            var msg = client.Communicate("repeat", new BaseEncode(File.ReadAllBytes(Directory.GetCurrentDirectory() + @"\input.jpg")).GetString());
                File.WriteAllBytes(Directory.GetCurrentDirectory() + @"\output.jpg", new BaseEncode(msg).GetBytes());
                
            Console.WriteLine("exit");
            Console.ReadLine();
        }
    }
}
