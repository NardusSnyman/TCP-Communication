using System;
using System.Text;

namespace Client
{
    class Program
    {
        static void Main(string[] args)
        {
            ClientServer.Client client = new ClientServer.Client(new ClientServer.ConnectionArguments("", 998));
            client.debug = new Action<string, int>((o,a) =>
            {
                Console.WriteLine(o);
            });
            client.Communicate(new ClientServer.ClientMessage("get", ""), ((x)=>{ Console.WriteLine(x.length); }));
            Console.ReadLine();
        }
    }
}
