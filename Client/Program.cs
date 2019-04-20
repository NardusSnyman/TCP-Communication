using System;
using System.Text;

namespace Client
{
    class Program
    {
        static void Main(string[] args)
        {
            ClientServer.Client client = new ClientServer.Client(new ClientServer.ConnectionArguments("", 998));
            /*client.debug = new Action<string, int>((o,a) =>
            /{
                Console.WriteLine(o);
            });*/
            var msg1 = client.Communicate(new ClientServer.ClientMessage("size", ""));
            var size = Convert.ToInt32(msg1.message);
            Console.WriteLine("XXXX:" + size);
            var cli_msg = new ClientServer.ClientMessage("get", "");
            var msg = client.Communicate(cli_msg, null, new Action<long>((x) => {
                Console.WriteLine("YYYY:" + x);
                Console.WriteLine("-" + (x * 100 / size));
            }));
            Console.ReadLine();
        }
    }
}
