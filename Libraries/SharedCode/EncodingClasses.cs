using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ClientServer
{
    
    public class EncodingClasses
    {
        private static char filler = '_';//must be availabe in utf8
        public static int expanded_length = 5;//must be > 5
        public class NetworkData//contains filler for constant 
        {
            public List<string> bytes;

            public Stream finished_stream;
            public NetworkData()
            {
                bytes = new List<string>();
            }
            public static NetworkData fromEncodedString(string data)
            {
                var byte1 = new List<string>();
                StringReader reader = new StringReader(data);
                char[] buffer = new String('0', expanded_length).ToCharArray();
                while(reader.ReadBlock(buffer, 0, expanded_length) > 0)
                {
                    byte1.Add(String.Join("", buffer));
                }
                return new NetworkData() { bytes = byte1 };
            }
            public static NetworkData fromDecodedString(string data)
            {
                if (data == null)
                    data = "";
                var dat = toNet(data.ToCharArray());

                return new NetworkData() { bytes = dat } ;
            }
            public static NetworkData fromDecodedBytes(byte[] data)
            {
                List<string> str = new List<string>();
                foreach(var byt in data)
                {
                    char c = (char)byt;
                    string str1 = SendRecieveUtil.toUnicodeString(c, new Action<string>((x) => { }));
                    while (str1.Length < expanded_length)
                        str1 += filler.ToString();
                    str.Add(str1);
                }

                
                return new NetworkData() { bytes =str };
            }
            public string GetEncodedString()
            {
                return String.Join("", bytes);
            }
            public string GetDecodedString()
            {
                return String.Join("", toBase(bytes.ToArray()));
            }
            public byte[] GetDecodedBytes()
            {
                List<byte> bytes_ = new List<byte>();
                foreach(char c in toBase(bytes.ToArray()))
                {
                    bytes_.Add((byte)c);
                }
                return bytes_.ToArray();
            }
            public void InitStream()
            {
                string data = SendRecieveUtil.separator + String.Join("", bytes) + SendRecieveUtil.separator;//initial
                long leng = data.Length + String.Join("", toNet(data.Length.ToString().ToCharArray())).Length;//length of initial and literal length together
                string length = String.Join("", toNet(Convert.ToInt32(leng).ToString().ToCharArray()));
                string data1 = length + data;//new predicted length ^ and data
                var dat = Encoding.UTF8.GetBytes(data1);//data
                finished_stream = new MemoryStream(dat);
            }
            private static List<string> toNet(char[] bytes1)
            {
                List<string> dat1 = new List<string>();
                for (int i = 0; i < bytes1.Length; i++)
                {
                    string str = SendRecieveUtil.toUnicodeString(bytes1[i], new Action<string>((x) => { }));
                    while (str.Length < expanded_length)
                        str += filler.ToString();

                    dat1.Add(str);
                }
                return dat1;
            }
            private static List<char> toBase(string[] bytes1)
            {
                List<char> data = new List<char>();
                for (int i = 0; i < bytes1.Length; i++)
                {
                    string code = bytes1[i];
                    code = code.Replace(filler.ToString(), "");
                    char c = SendRecieveUtil.toUnicodeChar(code, out bool succ);
                    if (succ)
                        data.Add(c);
                    else i = bytes1.Length;//rest of the data is null 


                }
                return data;
            }
        }
    }
}
