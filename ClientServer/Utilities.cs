using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;

namespace ClientServer
{
    public class Utilities
    {
        public static byte[] Utf8ToLatin(byte[] utf)
        {
            Encoding iso = Encoding.GetEncoding("ISO-8859-1");
            Encoding utf8 = Encoding.UTF8;
            return Encoding.Convert(utf8, iso, utf);
        }
        public static byte[] LatinToUtf8(byte[] latin)
        {
            Encoding iso = Encoding.GetEncoding("ISO-8859-1");
            Encoding utf8 = Encoding.UTF8;
            return Encoding.Convert(iso, utf8, latin);
        }
        public static byte[] GetBytes(string input)
        {
            return Encoding.UTF8.GetBytes(input);
        }
        public static string GetString(byte[] input)
        {
            return Encoding.UTF8.GetString(input);
        }
        public static void ReadStream(NetworkStream stream, ref MemoryStream input)
        {
            byte[] data = new byte[1024];
            try
            {
                for (int index = 0, count = 0; (count = stream.Read(data, index, data.Length)) != 0; index += count)
                {
                    input.Write(data, index, count);
                }
            }
            catch (ArgumentOutOfRangeException) { }
        }
    }
}
