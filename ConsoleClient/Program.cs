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
                client.clientThread(th).Invoke();
            });
            t.ConfigureAwait(false);
            t.Start();



            var txt = @"C:\Users\nicho\Pictures\Camera Roll\Capture2.PNG";
            var txt2 = @"C:\Users\nicho\Pictures\Camera Roll\Capture3.PNG";
            client.Communicate("repeat", NetworkData.fromDecodedBytes(File.ReadAllBytes(txt)), (x) =>
            {
                File.WriteAllBytes(txt2, x.GetDecodedBytes());
                Console.WriteLine("complete");
            });


            while (1 == 1)
            {
                
            }
        }
    }
}
