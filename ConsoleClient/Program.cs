using ClientServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace ConsoleClient
{
    class Program
    {
        static void Main(string[] args)
        {
            var disp = Dispatcher.CurrentDispatcher;
            Client client = new Client(new ConnectionArguments("192.168.1.8", 998, 256));
            Console.WriteLine(client.args.ip + ":" + client.args.port);
            client.universalDebug = new ExtendedDebug()
            {
                mainProcessDebug = (x) =>
                {
                        Console.WriteLine(x);
                },
                closeUpDebug = (x) =>
                {
                        Console.WriteLine(x);
                }
            };
            Console.WriteLine("hello");
            var t = new Task(()=>
            {
                Console.WriteLine("hello2");
                client.clientThread().Invoke();
                Console.WriteLine("hello3");

            });
            t.ConfigureAwait(false);
            t.Start();

            Console.WriteLine("start");
            Task.Delay(200);
            client.Communicate("repeat", "hello", (x)=>{ Console.WriteLine(x.GetDecodedString()); });
            Console.WriteLine("end");
            while (1==1)
            {

            }
        }
    }
}
