using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ClientServer
{
    public class EncodingClasses
    {
        public class NetworkEncoding
        {
            public byte[] data;
            public NetworkEncoding(byte[] data1)
            {
                data = data1;
            }
            public NetworkEncoding(byte data1)
            {
                MemoryStream ms = new MemoryStream();
                ms.WriteByte(data1);
                data = ms.ToArray();
            }
            public NetworkEncoding(string data1)
            {
                data = Encoding.UTF8.GetBytes(data1);
            }
            public BaseEncode GetBaseEncode()
            {
                return new BaseEncode(Encoding.Convert(Encoding.UTF8, Encoding.ASCII, data));
            }
            public string String()
            {
                return Encoding.UTF8.GetString(data);
            }
        }
        public class BaseEncode
        {
            public byte[] data;
            public BaseEncode(byte[] data1)
            {
                data = data1;
            }
            public BaseEncode(byte data1)
            {
                MemoryStream ms = new MemoryStream();
                ms.WriteByte(data1);
                data = ms.ToArray();
            }
            public NetworkEncoding GetnetworkEncoding()
            {
                return new NetworkEncoding(Encoding.Convert(Encoding.ASCII, Encoding.UTF8, data));
            }
            public BaseEncode(string data1)
            {
                data = Encoding.ASCII.GetBytes(data1);
            }
            public string String()
            {
                return Encoding.ASCII.GetString(data);
            }
        }
    }
}
