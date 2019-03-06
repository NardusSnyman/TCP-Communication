using System;
using System.Text;

namespace Client
{
    class Program
    {
        static void Main(string[] args)
        {
            ClientServer.Client client = new ClientServer.Client(new ClientServer.ConnectionArguments("192.168.1.43", 998));
            client.debug = new Action<string, int>((o,a) =>
            {
                Console.WriteLine(o);
            });
            var msg = client.Communicate(new ClientServer.ClientMessage("run", "works", Encoding.ASCII.GetBytes(".bytes.")));
            Console.ReadLine();
        }
    }
}
