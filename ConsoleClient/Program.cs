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
            Client client = new Client(new ConnectionArguments("192.168.0.143", 998, 2048));
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
            
            var t = new Task(()=>
            {
                client.clientThread().Invoke();
            });
            t.ConfigureAwait(false);
            t.Start();



            Console.Write("Path: ");
            var txt = @"C:\Users\nicho\Pictures\Camera Roll\Capture.PNG";
            var dat = NetworkData.fromDecodedBytes(File.ReadAllBytes(txt)).GetDecodedBytes();
            File.WriteAllBytes(new FileInfo(txt).DirectoryName + @"\NewFile2" + new FileInfo(txt).Extension, dat);
            client.Communicate("repeat", NetworkData.fromDecodedBytes(File.ReadAllBytes(txt)), (x) => { File.WriteAllBytes(new FileInfo(txt).DirectoryName + @"\NewFile" + new FileInfo(txt).Extension, x.GetDecodedBytes()); });
            while (1 == 1)
            {
                
            }
        }
    }
}
