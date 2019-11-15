using ClientServer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using static ClientServer.EncodingClasses;

namespace ConsoleClient
{
    class Program
    {
        static void Main(string[] args)
        {
            Client client = new Client(ConnectionArguments.fromLocal(998, 2048));
            Console.WriteLine(client.args.ip + ":" + client.args.port);
            Thread th = Thread.CurrentThread;
            var t = new Task(()=>
            {
                client.clientThread().Invoke();
            });
            t.ConfigureAwait(false);
            t.Start();


            client.Communicate("repeat", NetworkData.fromDecodedString("hello"), (x) =>
            {
                Console.WriteLine(x.GetDecodedString());
            });


            while (1 == 1)
            {
                
            }
        }
    }
}
