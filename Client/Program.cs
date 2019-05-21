using System;
using System.Text;

namespace Client
{
    class Program
    {
        static void Main(string[] args)
        {
            ClientServer.Client client = new ClientServer.Client(new ClientServer.ConnectionArguments("192.168.0.188", 998, '@', Convert.ToByte(';')));
            client.debug = new Action<string, int>((o,a) =>
            {
                Console.WriteLine(o);
            });
            var msg = client.Communicate(new ClientServer.ClientMessage("repeat", "hello"));
            Console.WriteLine(msg.message);
            Console.ReadLine();
        }
    }
}
