using System;
using System.Text;

namespace Client
{
    class Program
    {
        static void Main(string[] args)
        {
            ClientServer.Client client = new ClientServer.Client(new ClientServer.ConnectionArguments("192.168.1.25", 998));
            client.debug = new Action<string>((o) =>
            {
                Console.WriteLine(o);
            });
            var msg = client.Communicate(new ClientServer.ClientMessage("run", "works", Encoding.ASCII.GetBytes("works2")));
            Console.WriteLine(msg.message);
            Console.ReadLine();
        }
    }
}
