using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ClientServer
{
    
    public class EncodingClasses
    {
        private static char filler = '_';//must be availabe in utf8
        private static int expanded_length = 8;//must be > 4
        public class NetworkEncoding//contains filler for constant 
        {

            public List<string> bytes = new List<string>();

            public NetworkEncoding(string data)
            {
                StringReader reader = new StringReader(data);
                char[] buffer = new String('0', expanded_length).ToCharArray();
                while(reader.ReadBlock(buffer, 0, expanded_length) > 0)
                {
                    bytes.Add(String.Join("", buffer));
                }
            }
            public NetworkEncoding(List<string> data)
            {
                bytes = data;
            }
            public BaseEncode GetBaseEncode(Action<string> debug = null)
            {
                List<char> data = new List<char>();
                for (int i = 0; i < bytes.Count; i++)
                {
                    string code = bytes[i];
                    code = code.Replace(filler.ToString(), "");
                    char c = SendRecieveUtil.toUnicodeChar(code, out bool succ, debug);
                    if (succ)
                        data.Add(c);
                    else i = bytes.Count;//rest of the data is null 
                        
                    
                }
                return new BaseEncode(data);
            }
            public string GetRawString()
            {
                return String.Join("", bytes);
            }
            public string GetRawStringWithSeparator(string sep)
            {
                return String.Join(sep, bytes);
            }
            public string GetOriginalString()
            {
                return String.Join("", bytes).Replace(filler.ToString(), "");
            }
            public string GetOriginalStringWithSeparator(string sep)
            {
                return String.Join(sep, bytes).Replace(filler.ToString(), "");
            }
            public byte[] GetBytes()
            {
                return Encoding.UTF8.GetBytes(String.Join("", bytes));
            }
        }
        public class BaseEncode
        {
            public List<char> bytes = new List<char>();
            public BaseEncode(List<char> characters)
            {
                bytes = characters;
            }
            public BaseEncode(string characters)
            {
                bytes = new List<char>(characters.ToCharArray());
            }
            public BaseEncode(FileInfo file, int buffer_size = 2048)
            {
                var stream = file.OpenRead();
                byte[] buffer = new byte[buffer_size];
                int length;
                while((length = stream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    bytes.AddRange(Encoding.Unicode.GetString(buffer).ToCharArray());
                }
                stream.Close();
            }
            public void SaveToFile(FileInfo file, int buffer_size = 2048)
            {
                var stream = file.OpenRead();

                for (int index = 0; index < bytes.Count;index+=buffer_size)
                {
                    int count = bytes.Count - index;
                    List<char> buffer;
                    if (count < buffer_size) {
                        buffer = bytes.GetRange(index, count);
                        
                    }
                    else {
                        buffer = bytes.GetRange(index, buffer_size);
                    }
                    var byte2 = Encoding.Unicode.GetBytes(String.Join("", buffer));
                    stream.Write(byte2, 0, byte2.Length) ;

                }
                stream.Close();
            }
            public NetworkEncoding GetNetworkEncoding(Action<string> debug = null)
            {
                List<string> data = new List<string>();
                for(int i = 0; i < bytes.Count; i++)
                {
                    string str = SendRecieveUtil.toUnicodeString(bytes[i], debug);
                    while(str.Length < expanded_length)
                        str += filler.ToString();
                    
                        data.Add(str);
                }
                return new NetworkEncoding(data);
            }

            public string GetString()
            {
                if (bytes.Count > 0)
                    return String.Join("", bytes);
                return "NULL";
            }
           
        }
    }
}
