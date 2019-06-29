using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Timers;
using static ClientServer.EncodingClasses;

namespace ClientServer
{
    public class SendRecieveUtil
    {

        public static string ender = "999999";
        public static string separator = "999998";
        public static void SendBytes(TcpClient c, NetworkEncoding utf, ConnectionArguments conn, Action<string> debug)
        {
            NetworkStream stream = c.GetStream();
            debug?.Invoke("ATTEMPTING WRITE");
            
            

            StringReader sr = new StringReader(utf.GetRawString() + ender);
            debug?.Invoke("variables initialized");
            char[] buffer = new String('0', conn.buffer_size).ToCharArray();

            int count = 0;
            int length;
            while((length = sr.ReadBlock(buffer, 0, conn.buffer_size))> 0)
            {
                NetworkEncoding encode = new NetworkEncoding(String.Join("", new List<char>(buffer).GetRange(0, length).ToArray()));

                var bytes = Encoding.UTF8.GetBytes(encode.GetRawString());
                stream.Write(bytes, 0, bytes.Length);
                
                debug?.Invoke("packet=" + (count++));
            }
            debug?.Invoke("end write");

        }

        
        public static NetworkEncoding RecieveBytes(TcpClient c, ref NetworkEncoding overread, ConnectionArguments conn, Action<string> debug = null, Action<string[], bool> data = null)
        {
            NetworkEncoding encode = new NetworkEncoding("");
            var stream = c.GetStream();
            debug?.Invoke("ATTEMPTING READ");
            int index2 = 0;
            if (overread != null)
            {
                if ((index2 = overread.GetRawString().IndexOf(ender)) != -1)
                {
                    debug?.Invoke("overread contains ender");
                    //give part to encode
                    //give part to overread
                    var parts = overread.GetRawString().Split(new string[] { SendRecieveUtil.ender }, StringSplitOptions.None);
                    encode = new NetworkEncoding(parts[0]);
                    try
                    {
                        overread = new NetworkEncoding(parts[1]);
                    }
                    catch (Exception)
                    {

                    }
                    return encode;

                }
            }

            Stopwatch sw = new Stopwatch();
            debug?.Invoke("overread does not contain ender");
            int count = 0;
            bool dataCollected = false;
            int threshHold = 10000;
            //add overread to new stream
            if (overread != null)
                    encode.bytes.AddRange(overread.bytes);
                byte[] buffer = new byte[conn.buffer_size];
                while (sw.Elapsed.TotalMilliseconds < threshHold)
                {
                
                if (stream.DataAvailable)
                {
                    dataCollected = true;
                    sw.Reset();
                    int length = stream.Read(buffer, 0, buffer.Length);//read string represented bytes
                    
                    string data1 = Encoding.UTF8.GetString(buffer);
                   
                    var piece = new NetworkEncoding(data1).bytes;//convert string representation to unicode characters

                    int index = 0;
                    if ((index = piece.IndexOf(ender)) != -1)//contains ender
                    {
                        debug?.Invoke("ender exists");
                        //give part to encode
                        //give part to overread

                        var data2 = new NetworkEncoding(piece.GetRange(0, index)).bytes;
                        encode.bytes.AddRange(data2);
                        data?.Invoke(data2.ToArray(), true);
                        try
                        {
                            overread = new NetworkEncoding(piece.GetRange(index + 1, piece.Count - index - 1));
                        }
                        catch (ArgumentException)
                        {
                            debug?.Invoke("overread does not exist");
                        }
                        break;
                    }
                    else//does not contain
                    {
                        var byt = new NetworkEncoding(piece);
                        data?.Invoke(byt.bytes.ToArray(), false);
                        encode.bytes.AddRange(byt.bytes);//add data to collector
                    }
                    debug?.Invoke("packet=" + count++);
                }
                else
                {
                    if (dataCollected == true)
                        threshHold = 200;
                    
                    if (!sw.IsRunning)
                        sw.Start();
                }
            }
            if(sw.Elapsed.TotalMilliseconds < threshHold)
                debug?.Invoke("timed out");
            return encode;

        }


        public static string toUnicodeString(char unicode, Action<string> debug)
        {
            string str = String.Format("{0:x4}", (int)unicode);
            return str;

        }
        public static char toUnicodeChar(string unicode, out bool successful, Action<string> debug)
        {
            try
            {
                
                int p = int.Parse(unicode, System.Globalization.NumberStyles.HexNumber);
                successful = true;
                return (char)p;
            }
            catch (Exception)
            {
                //empty
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("-" + unicode + "-");
                Console.ResetColor();
                debug?.Invoke("conversion unsuccessful. empty packet");
                successful = false;
                return 'O';
            }
        }
    }

}
