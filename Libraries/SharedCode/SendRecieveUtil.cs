using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Timers;
using static ClientServer.EncodingClasses;

namespace ClientServer
{
    public class SendRecieveUtil
    {

        public static string ender { get { string x = "9_"; while (x.Length < expanded_length) x=9+x; return x; } }
        public static string separator { get { string x = "8_"; while (x.Length < expanded_length) x = 9 + x; return x; } }
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
                    Debug.WriteLine("-" + overread.GetRawString());
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
            int threshHold = (int)conn.timeout_time.TotalMilliseconds;
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
                    
                    string data1 = Encoding.UTF8.GetString(buffer, 0, length);
                    
                    var piece = new NetworkEncoding(data1);//convert string representation to unicode characters
                    
                    int index = 0;
                    if ((index = piece.bytes.IndexOf(ender)) != -1)//contains ender in list
                    {
                        debug?.Invoke("ender exists");
                        //give part to encode
                        //give part to overread
                        var data2 = new NetworkEncoding(new List<string>(piece.bytes.Take(index))).bytes;
                        encode.bytes.AddRange(data2);
                        
                        data?.Invoke(data2.ToArray(), true);
                        try
                        {
                            Debug.WriteLine("+" + String.Join("", piece.bytes.Skip(index +1)));
                            overread = new NetworkEncoding(new List<string>(piece.bytes.Skip(index +1)));
                        }
                        catch (ArgumentException)
                        {
                            debug?.Invoke("overread does not exist");
                        }
                        break;
                    }
                    else//does not contain
                    {
                        data?.Invoke(piece.bytes.ToArray(), false);
                        encode.bytes.AddRange(piece.bytes);//add data to collector
                    }
                    debug?.Invoke("packet=" + count++);
                }
                else
                {
                    if (dataCollected == true)
                        threshHold = (int)conn.timeout_on_recent.TotalMilliseconds;
                    
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
                debug?.Invoke("conversion unsuccessful. empty packet");
                successful = false;
                return '-';
            }
        }
    }

}
