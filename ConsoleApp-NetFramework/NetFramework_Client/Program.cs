using ClientServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    class Program
    {
        static void Main(string[] args)
        {
            ClientServer.Client client = new ClientServer.Client(new ConnectionArguments("192.168.0.188", 998, '@', Convert.ToByte(';'), 1024))
            {
                debug = new Action<string, int>((o, a) =>
                {
                    Console.WriteLine(o);
                })
            };
            var msg = client.Communicate(new ClientMessage("repeat", "hello"));
            Console.WriteLine(msg.message);
            Console.ReadLine();
        }
    }
}
