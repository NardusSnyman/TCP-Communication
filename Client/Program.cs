using System;
using System.Text;

namespace Client
{
    class Program
    {
        static void Main(string[] args)
        {
            ClientServer.Client client = new ClientServer.Client(new ClientServer.ConnectionArguments("ip", 998));
            client.debug = new Action<string, int>((o,a) =>
            {
                Console.WriteLine(o);
            });
            var msg = client.Communicate(new ClientServer.ClientMessage("run", "works"));
            Console.ReadLine();
        }
    }
}
