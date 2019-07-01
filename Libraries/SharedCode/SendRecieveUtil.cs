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
        public static string separator2 { get { string x = "7_"; while (x.Length < expanded_length) x = 9 + x; return x; } }
        public static void SendBytes(TcpClient c, NetworkEncoding utf, ConnectionArguments conn, Tuple<ExtendedDebug, ExtendedDebug> deb)
        {
            ExtendedDebug debug = deb.Item1;
            ExtendedDebug universalDebug = deb.Item2;
            NetworkStream stream = c.GetStream();
            debug.mainProcessDebug?.Invoke("ATTEMPTING WRITE");
            universalDebug.mainProcessDebug?.Invoke("ATTEMPTING WRITE");
            universalDebug.mainProcessDebug?.Invoke("ATTEMPTING WRITE");


            string data = separator2 + utf.GetRawString() + ender;
            long leng = data.Length + new BaseEncode(data.Length.ToString()).GetNetworkEncoding().GetRawString().Length;
            data = new BaseEncode(leng.ToString()).GetNetworkEncoding().GetRawString() + data;
            long totalLength = Encoding.UTF8.GetBytes(data).LongLength;
            StringReader sr = new StringReader(data);
            debug.closeUpDebug?.Invoke("variables initialized");
            universalDebug.closeUpDebug?.Invoke("variables initialized");
            char[] buffer = new String('0', conn.buffer_size).ToCharArray();

            long count = 0;
            int length;
            debug.closeUpDebug?.Invoke("starting write");
            universalDebug.closeUpDebug?.Invoke("starting write");
            while ((length = sr.ReadBlock(buffer, 0, conn.buffer_size))> 0)
            {
                NetworkEncoding encode = new NetworkEncoding(String.Join("", new List<char>(buffer).GetRange(0, length).ToArray()));

                var bytes = Encoding.UTF8.GetBytes(encode.GetRawString());
                stream.Write(bytes, 0, bytes.Length);

                count += bytes.Length / conn.buffer_size;
                debug.uploadProgressDebug?.Invoke(count * conn.buffer_size, totalLength);
                universalDebug.uploadProgressDebug?.Invoke(count * conn.buffer_size, totalLength);
                debug.packetInvoked?.Invoke(true, count);
                universalDebug.packetInvoked?.Invoke(true, count);
            }
            debug.mainProcessDebug?.Invoke("END WRITE");
            universalDebug.mainProcessDebug?.Invoke("END WRITE");
            

        }

        
        public static NetworkEncoding RecieveBytes(TcpClient c, ref NetworkEncoding overread, ConnectionArguments conn, Tuple<ExtendedDebug, ExtendedDebug> deb)
        {
            ExtendedDebug debug = deb.Item1;
            ExtendedDebug universalDebug = deb.Item2;
            NetworkEncoding encode = new NetworkEncoding("");
            long size = 0;
            NetworkEncoding sizeEncode = new NetworkEncoding("");
            var stream = c.GetStream();
            debug.mainProcessDebug?.Invoke("ATTEMPTING READ");
            universalDebug.mainProcessDebug?.Invoke("ATTEMPTING READ");
            int index2 = 0;
            if (overread != null)
            {
                if ((index2 = overread.GetRawString().IndexOf(ender)) != -1)
                {
                    debug.closeUpDebug?.Invoke("overread contains ender");
                    universalDebug.closeUpDebug?.Invoke("overread contains ender");
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
            debug.closeUpDebug?.Invoke("overread does not contain ender or null");
            universalDebug.closeUpDebug?.Invoke("overread does not contain ender or null");
            long count = 0;
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

                    string data1 = Encoding.UTF8.GetString(buffer, 0, length);//recieved data
                   
                    var piece = new NetworkEncoding(data1);//convert string representation to unicode characters

                    if (size == 0)//size not found yet
                    {
                        int index = 0;
                        if ((index = piece.bytes.IndexOf(separator2)) != -1)//contains separator2 so size ends
                        {
                            debug.closeUpDebug?.Invoke("separator2 exists in main stream packet");
                            universalDebug.closeUpDebug?.Invoke("separator2 exists in main stream packet");
                            //give part to encode
                            //give part to overread
                            var data2 = new NetworkEncoding(new List<string>(piece.bytes.Take(index))).bytes;
                            sizeEncode.bytes.AddRange(data2);//first part sent to size
                            
                            //rest of the data
                            var rest = new List<string>(piece.bytes.Skip(index + 1));//rest of the data collected
                            
                            int index3 = 0;
                            if ((index3 = rest.IndexOf(ender)) != -1)//contains ender in list
                            {
                                debug.closeUpDebug?.Invoke("Ender exists in main stream packet");
                                universalDebug.closeUpDebug?.Invoke("Ender exists in main stream packet");
                                //give part to encode
                                //give part to overread
                                var data3 = new NetworkEncoding(new List<string>(rest.Take(index3))).bytes;
                                
                                encode.bytes.AddRange(data3);
                               

                                try
                                {

                                    overread = new NetworkEncoding(new List<string>(rest.Skip(index3 + 1)));
                                }
                                catch (ArgumentException)
                                {
                                    debug.closeUpDebug?.Invoke("overread initialized with no remaining data");
                                    universalDebug.closeUpDebug?.Invoke("overread initialized with no remaining data");
                                }
                                debug.closeUpDebug?.Invoke("end read-size recieved");
                                universalDebug.closeUpDebug?.Invoke("end read-size recieved");
                                debug.packetInvoked?.Invoke(false, count++);
                                universalDebug.packetInvoked?.Invoke(false, count);
                                
                                return encode;
                            }//ender exists
                            else//does not contain
                            {
                                encode.bytes.AddRange(rest);//data added to encode
                                size = Convert.ToInt64(sizeEncode.GetBaseEncode().GetString());
                            }



                        }
                        else//does not contain
                        {

                            sizeEncode.bytes.AddRange(piece.bytes);//add data to collector
                        }
                        debug.packetInvoked?.Invoke(false, count += length / conn.buffer_size);
                        universalDebug.packetInvoked?.Invoke(false, count / conn.buffer_size);
                    }
                    else
                    {
                        int index = 0;
                        if ((index = piece.bytes.IndexOf(ender)) != -1)//contains ender in list
                        {
                            debug.closeUpDebug?.Invoke("Ender exists in main stream packet");
                            universalDebug.closeUpDebug?.Invoke("Ender exists in main stream packet");
                            //give part to encode
                            //give part to overread
                            var data2 = new NetworkEncoding(new List<string>(piece.bytes.Take(index))).bytes;
                            encode.bytes.AddRange(data2);


                            try
                            {

                                overread = new NetworkEncoding(new List<string>(piece.bytes.Skip(index + 1)));
                            }
                            catch (ArgumentException)
                            {
                                debug.closeUpDebug?.Invoke("overread initialized with no remaining data");
                                universalDebug.closeUpDebug?.Invoke("overread initialized with no remaining data");
                            }
                            debug.closeUpDebug?.Invoke("end read");
                            universalDebug.closeUpDebug?.Invoke("end read");
                            debug.packetInvoked?.Invoke(false, count++);
                            universalDebug.packetInvoked?.Invoke(false, count);
                            return encode;
                        }
                        else//does not contain
                        {

                            encode.bytes.AddRange(piece.bytes);//add data to collector
                        }
                    }
                    count += length / conn.buffer_size;
                    debug.packetInvoked?.Invoke(false, count);
                    universalDebug.packetInvoked?.Invoke(false, count);
                    debug.downloadProgressDebug?.Invoke(count * conn.buffer_size, size);
                    universalDebug.downloadProgressDebug?.Invoke(count * conn.buffer_size, size);

                }
                else
                {
                    debug.closeUpDebug?.Invoke($"data not available  previouslyRecieved={dataCollected}");
                    universalDebug.closeUpDebug?.Invoke($"data not available  previouslyRecieved={dataCollected}");
                    if (dataCollected == true)
                        threshHold = (int)conn.timeout_on_recent.TotalMilliseconds;

                    if (!sw.IsRunning)
                        sw.Start();
                }

            }
            if (sw.Elapsed.TotalMilliseconds > threshHold)
            {
                debug.closeUpDebug?.Invoke("client timed out");
                universalDebug.closeUpDebug?.Invoke("client timed out");
            }
            return encode;

        }


        public static string toUnicodeString(char unicode, Action<string> debug)
        {
            string str = String.Format("{0:x4}", (int)unicode);
            return str;

        }
        public static char toUnicodeChar(string unicode, out bool successful)
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
                successful = false;
                return '-';
            }
        }
    }

}
