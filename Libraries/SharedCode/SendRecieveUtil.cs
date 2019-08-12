using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        public static string separator { get { string x = "8_"; while (x.Length < expanded_length) x = 9 + x; return x; } }
        public static void SendBytes(TcpClient c, NetworkData data1, ConnectionArguments conn, Tuple<ExtendedDebug, ExtendedDebug> deb)
        {
            ExtendedDebug debug = deb.Item1;
            ExtendedDebug universalDebug = deb.Item2;
            NetworkStream stream = c.GetStream();
            debug.mainProcessDebug?.Invoke("ATTEMPTING WRITE");
            universalDebug.mainProcessDebug?.Invoke("ATTEMPTING WRITE");
            universalDebug.mainProcessDebug?.Invoke("ATTEMPTING WRITE");
        start:
            Stream data = data1.finished_stream;
            if (data == null)
            {
                data1.InitStream();
                goto start;
            }
            data.Position = 0;

            
            debug.closeUpDebug?.Invoke("variables initialized");
            universalDebug.closeUpDebug?.Invoke("variables initialized");
            byte[] buffer = new byte[conn.buffer_size];

            long count = 0;
            int length;

            debug.closeUpDebug?.Invoke("starting write");
            universalDebug.closeUpDebug?.Invoke("starting write");

            while ((length = data.Read(buffer, 0, conn.buffer_size))> 0)
            {
                if (c.Connected)
                {
                    stream.Write(buffer, 0, length);

                    count += buffer.Length / conn.buffer_size;
                    debug.uploadProgressDebug?.Invoke(count * conn.buffer_size, data.Length);
                    universalDebug.uploadProgressDebug?.Invoke(count * conn.buffer_size, data.Length);
                    debug.packetInvoked?.Invoke(true, count);
                    universalDebug.packetInvoked?.Invoke(true, count);
                }
                else
                {
                    c.Connect(conn.ip, conn.port);
                    stream = c.GetStream();
                }
            }
            debug.mainProcessDebug?.Invoke("END WRITE");
            universalDebug.mainProcessDebug?.Invoke("END WRITE");
            

        }


        public static void RecieveBytes(TcpClient c, ref NetworkData overread, ConnectionArguments conn, List<RetrievalNode> nodes)
        {
            var stream = c.GetStream();
            foreach (var node in nodes)
            {
                var dat = getData(stream, ref overread, conn.buffer_size, separator);//size
                node.direct?.Invoke(dat);
            }
        }
        public static NetworkData getData(Stream stream, ref NetworkData overread, int buffer_size, string separator)
        {
                int index = 0;
                StringBuilder sb = new StringBuilder();
                byte[] buffer = new byte[buffer_size];
                if (overread == null)
                    overread = NetworkData.fromDecodedString("");
            while (true) {

                string dat = overread.GetEncodedString();
                if ((index = dat.IndexOf(separator)) != -1)//contains ender in overread
                {
                    var return_ = NetworkData.fromEncodedString(dat.Substring(0, index));//return data

                    overread = NetworkData.fromEncodedString(dat.Substring(index + separator.Length));//out overread

                    return return_;//return data
                }
                else {//overread does not contain ender
                    sb.Append(overread.GetEncodedString());
                    int length = stream.Read(buffer, 0, buffer_size);//read string represented bytes
                    string data = Encoding.UTF8.GetString(buffer, 0, length);

                   
                    if ((index = data.IndexOf(separator)) != -1)//contains ender in list
                    {
                        sb.Append(data);
                        int index2 = sb.Length - (data.Length - index);


                        var return_ = NetworkData.fromEncodedString(sb.ToString().Substring(0, index2));
                        
                        overread = NetworkData.fromEncodedString(data.Substring(index + separator.Length));
                        return return_;

                    }
                    else
                    {
                        sb.Append(data);
                    }
                }
            }
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
