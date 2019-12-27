using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Timers;
using static ClientServer.EncodingClasses;

namespace ClientServer
{
    public class SendRecieveUtil
    {
        public static string separator { get { string x = ""; while (x.Length < expanded_length) x = 9 + x; return x; } }
        public static void SendBytes(ConnectClient c, NetworkData data1, ConnectionArguments conn, Action<string, int> debug)
        {
            Console.WriteLine("1");
            NetworkStream stream = c.GetStream();
        start:
            Stream data = data1.finished_stream;
            Console.WriteLine("2");
            if (data == null)
            {
                data1.InitStream(out int length1);
                debug($"[{c.port}]: length={length1}", 3);
                goto start;
            }
            data.Position = 0;
            byte[] buffer = new byte[conn.buffer_size];
            Console.WriteLine("3");
            long count = 0;
            int length;

            string encoded_string = Encoding.UTF8.GetString(((MemoryStream)data).ToArray());
            string data_out = NetworkData.fromEncodedString(encoded_string).GetDecodedString();
            debug($"[{c.port}]: dataOut={data_out}", 5);
            Stopwatch sw = new Stopwatch();
                while ((length = data.Read(buffer, 0, conn.buffer_size)) > 0)
                {
                try
                {
                    var data_1 = NetworkData.fromEncodedString(Encoding.UTF8.GetString(buffer));
                    debug($"[{c.port}]: Writing packet ({count})", 2);
                    debug($"[{c.port}]: packetdata={data_1.GetEncodedString()}", 5);
                    debug($"[{c.port}]: packetdata={data_1.GetDecodedString()}", 5);
                    stream.Write(buffer, 0, length);
                    count += buffer.Length / conn.buffer_size;
                    sw.Reset();
                }
                catch (System.IO.IOException e)
                {
                    c.Reconnect();
                    if (!sw.IsRunning)
                        sw.Start();
                    if (sw.ElapsedMilliseconds > TimeSpan.FromSeconds(0.5).TotalMilliseconds)
                        break;
                    debug($"[{c.port}]: error=" + e.Message + e.InnerException, 2);
                    
                }
            }
            sw.Stop();

        }


        public static void RecieveBytes(ConnectClient c, ref NetworkData overread, ConnectionArguments conn, int total, Action<double> prog, Action<string, int> debug, List<RetrievalNode> nodes)
        {
            debug($"[{c.port}]: begin read ({nodes.Count})", 2);
            c.Reconnect();
            var stream = c.GetStream();
                foreach (var node in nodes)
                {

                    debug($"[{c.port}]: executing node ({node.motive})", 2);
                    var dat = getData(c, ref overread, conn.buffer_size, separator, total, prog, null, debug);//size
                    debug($"[{c.port}]: dataIn={dat.GetDecodedString()}", 5);
                    node.direct?.Invoke(dat);

                }
        }
        public static NetworkData getData(ConnectClient c, ref NetworkData overread, int buffer_size, string separator, int total, Action<double> prog, Action<NetworkData> bytes, Action<string, int> debug)
        {
            Stopwatch sw = new Stopwatch();
            int onlyonce = 0;
                int index = 0;
                StringBuilder sb = new StringBuilder();
                byte[] buffer = new byte[buffer_size];
                if (overread == null || overread.GetDecodedString() == "null")
                    overread = NetworkData.fromEncodedString("");
            while (c.Connected) {

                string dat = overread.GetEncodedString();
                if ((index = dat.IndexOf(separator)) != -1)//contains ender in overread
                {
                    debug($"[{c.port}]: overread contains end character", 2);
                    var return_ = NetworkData.fromEncodedString(dat.Substring(0, index));//return data

                    overread = NetworkData.fromEncodedString(dat.Substring(index + separator.Length));//out overread
                    bytes?.Invoke(return_);
                    debug($"[{c.port}]: finalizing", 2);
                    sw.Reset();
                    return return_;//return data
                }
                else {//overread does not contain ender
                    if (onlyonce == 0)
                        sb.Append(overread.GetEncodedString());
                    onlyonce = 1;
                    int length = 0;
                    try
                    {
                        debug($"[{c.port}]: recieved packet ({length})", 2);
                        if(c.GetStream().Length > 0 && c.Connected)
                        length = c.GetStream().Read(buffer, 0, buffer_size);//read string represented bytes
                        debug("1", 2);
                    }
                    catch(Exception e)
                    {
                        debug($"[{c.port}]: server closed", 2);
                        break;
                    }
                    if (length == 0)
                    {
                        debug($"[{c.port}]: breaking", 2);
                        if (!sw.IsRunning)
                            sw.Start();
                        if (sw.ElapsedMilliseconds > TimeSpan.FromSeconds(0.25).TotalMilliseconds)
                        {
                            c.Close();
                            break;
                        }
                    }
                    else
                    {
                        debug($"[{c.port}]: -packet is valid", 2);
                        sw.Reset();
                        string data = Encoding.UTF8.GetString(buffer, 0, length);
                        debug($"[{c.port}]: packetdata={data}", 5);
                        debug($"[{c.port}]: packetdata={NetworkData.fromEncodedString(data).GetDecodedString()}", 5);
                        if ((index = data.IndexOf(separator)) != -1)//contains ender in list
                        {
                            debug($"[{c.port}]: -packet contains ender", 2);
                            sb.Append(data);
                            int index2 = sb.Length - (data.Length - index);


                            var return_ = NetworkData.fromEncodedString(sb.ToString().Substring(0, index2));

                            overread = NetworkData.fromEncodedString(data.Substring(index + separator.Length));
                            bytes?.Invoke(return_);
                            prog?.Invoke(sb.Length / total);
                            debug($"[{c.port}]: finalizing", 2);
                            debug(sb.ToString(), 5);
                            sw.Reset();
                            return return_;

                        }
                        else
                        {

                            bytes?.Invoke(NetworkData.fromEncodedString(data));
                            debug($"[{c.port}]: -general packet type", 2);

                            if (bytes == null)
                            {
                                sb.Append(data);
                                debug("1=" + data, 5);
                                prog?.Invoke(sb.Length / total);
                            }
                        }
                    }
                }
            }
            sw.Reset();
            return NetworkData.Empty();
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
