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

        public static readonly char ender = toUnicodeChar("F0000", out bool s, null);//unused ender character
        public static readonly char separator = toUnicodeChar("FFFFD", out bool s, null);//unused ender character
        public static void SendBytes(TcpClient c, BaseEncode data, ConnectionArguments conn, Action<string> debug)
        {
            NetworkStream stream = c.GetStream();
            debug?.Invoke("stream fetched");
            NetworkEncoding utf = new BaseEncode(data.GetString() + ender.ToString()).GetNetworkEncoding(debug);
            
            StringReader sr = new StringReader(utf.GetRawString());
            debug?.Invoke("variables initialized");
            char[] buffer = new String('0', conn.buffer_size).ToCharArray();

            int count = 0;
            int length;
            while((length = sr.ReadBlock(buffer, 0, conn.buffer_size))> 0)
            {
                debug?.Invoke("read block");
                NetworkEncoding encode = new NetworkEncoding(String.Join("", new List<char>(buffer).GetRange(0, length).ToArray()));
                stream.Write(encode.GetBytes(), 0, encode.GetBytes().Length);
                debug?.Invoke("packet=" + count++);
            }

            
        }

        
        public static BaseEncode RecieveBytes(TcpClient c, ref BaseEncode overread, ConnectionArguments conn, Action<string> debug = null, Action<char[], bool> data = null)
        {
            BaseEncode encode = new BaseEncode("");
            var stream = c.GetStream();
            debug?.Invoke("stream fetched");
            int index2 = 0;
            if (overread != null)
            {
                if ((index2 = overread.bytes.IndexOf(ender)) != -1)
                {
                    debug?.Invoke("overread contains ender");
                    //give part to encode
                    //give part to overread
                    encode.bytes.AddRange(overread.bytes.GetRange(0, index2));
                    if (index2 + 1 != overread.bytes.Count)
                        overread = new BaseEncode(overread.bytes.GetRange(index2 + 1, overread.bytes.Count - index2 - 1));
                    return encode;

                }
            }
            debug?.Invoke("overread does not contain ender");
            int count = 0;
            //add overread to new stream
            if (overread != null)
                    encode.bytes.AddRange(overread.bytes);
                byte[] buffer = new byte[conn.buffer_size];
                while (true)
                {
                
                int length = stream.Read(buffer, 0, buffer.Length);//read string represented bytes
                    if (length > 0)
                    {//found source
                    string data1 = Encoding.UTF8.GetString(buffer);

                        var piece = new NetworkEncoding(data1).GetBaseEncode(debug);//convert string representation to unicode characters
                    debug?.Invoke("unicode characters recieved");
                    int index = 0;
                        if ((index = piece.bytes.IndexOf(ender)) != -1)//contains ender
                        {
                        debug?.Invoke("ender exists");
                        //give part to encode
                        //give part to overread
                        var data3 = piece.bytes.GetRange(0, index);
                        encode.bytes.AddRange(data3);
                        data?.Invoke(data3.ToArray(), true);
                        try
                        {
                            overread = new BaseEncode(piece.bytes.GetRange(index + 1, length - index - 1));
                        }
                        catch (ArgumentException)
                        {
                            debug?.Invoke("overread does not exist");
                        }
                            break;
                        }
                        else//does not contain
                        {
                        debug?.Invoke("reading. no ender");
                        data?.Invoke(piece.bytes.ToArray(), false);
                        encode.bytes.AddRange(piece.bytes);//add data to collector
                        }
                    debug?.Invoke("packet=" + count++);
                }
                }
            
            return encode;

        }


        public static string toUnicodeString(char unicode, Action<string> debug)
        {
            string str = String.Format("{0:x4} ", (int)unicode).Replace(" ", "");
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
                return 'x';
            }
        }
    }

}
