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
        public static void SendBytes(TcpClient c, NetworkData data1, ConnectionArguments conn, int port)
        {
            NetworkStream stream = c.GetStream();
        start:
            Stream data = data1.finished_stream;
            if (data == null)
            {
                data1.InitStream();
                goto start;
            }
            data.Position = 0;
            byte[] buffer = new byte[conn.buffer_size];

            long count = 0;
            int length;

            while ((length = data.Read(buffer, 0, conn.buffer_size))> 0)
            {
                if (c.Connected)
                {
                    stream.Write(buffer, 0, length);
                    count += buffer.Length / conn.buffer_size;
                }
                else
                {
                    c.Connect(conn.ip, port);
                    stream = c.GetStream();
                }
            }
            

        }


        public static void RecieveBytes(TcpClient c, ref NetworkData overread, ConnectionArguments conn, int total, Action<double> prog, List<RetrievalNode> nodes)
        {
            var stream = c.GetStream();
            foreach (var node in nodes)
            {
                try
                {
                    var dat = getData(c, stream, ref overread, conn.buffer_size, separator, total, prog, null);//size
                    node.direct?.Invoke(dat);
                }catch(Exception e)
                {

                }
            }
        }
        public static NetworkData getData(TcpClient c, Stream stream, ref NetworkData overread, int buffer_size, string separator, int total, Action<double> prog, Action<NetworkData> bytes)
        {
            Stopwatch sw = new Stopwatch();
            int onlyonce = 0;
                int index = 0;
                StringBuilder sb = new StringBuilder();
                byte[] buffer = new byte[buffer_size];
                if (overread == null)
                    overread = NetworkData.fromDecodedString("");
            while (c.Connected) {

                string dat = overread.GetEncodedString();
                if ((index = dat.IndexOf(separator)) != -1)//contains ender in overread
                {
                  
                    var return_ = NetworkData.fromEncodedString(dat.Substring(0, index));//return data

                    overread = NetworkData.fromEncodedString(dat.Substring(index + separator.Length));//out overread
                    bytes?.Invoke(return_);
                    
                    return return_;//return data
                }
                else {//overread does not contain ender
                    if (onlyonce == 0)
                        sb.Append(overread.GetEncodedString());
                    onlyonce = 1;
                    int length = 0;
                    try
                    {
                        
                        length = stream.Read(buffer, 0, buffer_size);//read string represented bytes
                    }catch(Exception e)
                    {
                        break;
                    }
                    if (length == 0)
                    {
                        if (!sw.IsRunning)
                            sw.Start();
                        if (sw.ElapsedMilliseconds > TimeSpan.FromSeconds(0.5).TotalMilliseconds)
                        {
                            c.Close();
                            break;
                        }
                    }
                    else
                    {
                        sw.Reset();
                        string data = Encoding.UTF8.GetString(buffer, 0, length);

                        if ((index = data.IndexOf(separator)) != -1)//contains ender in list
                        {
                            sb.Append(data);

                            int index2 = sb.Length - (data.Length - index);


                            var return_ = NetworkData.fromEncodedString(sb.ToString().Substring(0, index2));

                            overread = NetworkData.fromEncodedString(data.Substring(index + separator.Length));
                            bytes?.Invoke(return_);
                            prog?.Invoke(sb.Length / total);
                            return return_;

                        }
                        else
                        {

                            bytes?.Invoke(NetworkData.fromEncodedString(data));
                            if (bytes == null)
                            {
                                sb.Append(data);
                                prog?.Invoke(sb.Length / total);
                            }
                        }
                    }
                }
            }
            return null;
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
