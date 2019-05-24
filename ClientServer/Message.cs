using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using static ClientServer.SendRecieve;

namespace ClientServer
{
    public class ServerMessage
    {
        public string message;
        public BaseEncode messagearray;
        public bool successful;
        public ServerMessage()
        {
            successful = false;
            message = "null";
            messagearray = new BaseEncode("null");
        }

        public ServerMessage(BaseEncode bytes, char separator)
        {
            string data = bytes.String();
            var param = data.Split(separator);
            message = param[0];
            successful = Boolean.Parse(param[1]);
            messagearray = new BaseEncode(param[2]);
        }
        public BaseEncode Bytes(char separator)
        {
            string arguments = $"{message}{separator}{successful}{separator}{messagearray.String()}";
            
            return new BaseEncode(arguments);
        }
        public ServerMessage(string message1, bool successful1, BaseEncode messagearray1 = null)
        {
            if (message1 == string.Empty)
            {
                message = "null";
            }
            else message = message1;
            if (messagearray1 == null)
            {
                messagearray = new BaseEncode("null");
            }
            else messagearray = messagearray1;
            successful = successful1;
        }
    }

    public class ClientMessage
    {
        public string operation;
        public string message;
        public BaseEncode messagearray;
        public ClientMessage()
        {
            operation = "null";
            message = "null";
            messagearray = new BaseEncode("null");
        }

        public ClientMessage(BaseEncode bytes, char separator)
        {
            string data = bytes.String();
            var param = data.Split(separator);
            operation = param[0];
            message = param[1];
            messagearray = new BaseEncode(param[2]);
        }
        public ClientMessage(string operation1, string message1, BaseEncode messagearray1 = null)
        {
            operation = operation1;
            if (message1 == string.Empty)
            {
                message = "null";
            }
            else message = message1;
            if (messagearray1 == null)
            {
                messagearray = new BaseEncode("null");
            }
            else messagearray = messagearray1;
        }
        public BaseEncode Bytes(char separator)
        {
            string arguments = $"{operation}{separator}{message}{separator}{messagearray.String()}";
            
            return new BaseEncode(arguments);
        }

    }

    public class SendRecieve
    {
        public static void SendBytes(TcpClient c, BaseEncode BaseEncode, Action<int> packet, byte ender, int buffer_size)
        {
            
            //declare variables
            byte[] bytes = new byte[buffer_size];
            var utf = BaseEncode.GetnetworkEncoding();
            NetworkStream s = c.GetStream();

            MemoryStream ms = new MemoryStream();
            ms.Write(utf.data, 0, utf.data.Length);
            ms.WriteByte(ender);
            ms.Position = 0;

            int count = 0;
            int length;
            while((length=ms.Read(bytes, 0, buffer_size)) > 0)
            {
                count += length / buffer_size;
                packet(count);
                s.Write(bytes, 0, length);
            }
                
        }
        public static BaseEncode RecieveBytes(TcpClient c, Action<int> packet, byte ender, ref MemoryStream overread, int buffer_size)
        {
            int count = 0;
            byte[] buffer = new byte[buffer_size];
            if (overread == null)
                overread = new MemoryStream();
            overread.Position = 0;
            int index;
            if ((index = Array.IndexOf(overread.ToArray(), ender)) > -1)
            {
                count -= index / buffer_size;
                packet(count);
                Array.Copy(overread.ToArray(), buffer, index);
                if (overread.Length - (index + 1) != 0)//length is not the same as second part beginning
                {
                    byte[] buff = new byte[overread.Length - (index + 1)];

                    Array.ConstrainedCopy(overread.ToArray(), (index + 1), buff, 0, buff.Length);
                    overread = new MemoryStream(buff); //equal to remaining piece
                }
                else
                {
                    overread = new MemoryStream();//equal 0 characters
                }
                
                return new NetworkEncoding(buffer).GetBaseEncode();
            }
            else//does not contain ender
            {
                
                //read from stream
                NetworkStream s = c.GetStream();
                MemoryStream ms = new MemoryStream();
                
                if(overread.Length > 0)//length of stored data > 0
                {
                    ms.Write(overread.ToArray(), 0, overread.ToArray().Length);//write to main stream
                }
                while (true)
                {
                    try
                    {
                       
                        int length;
                        while((length = s.Read(buffer, 0, buffer_size)) > 0)
                        {
                            count += length / buffer_size;
                            packet(count);
                            if ((index = Array.IndexOf(buffer, ender)) > -1)
                            {
                                
                                ms.Write(buffer, 0, index);//end found and writing first part to stream
                               
                                byte[] buff = new byte[length - (index + 1)];//buffer length accomodated for second part
                                if (length - (index + 1) != 0)//length != 0
                                {
                                    Array.ConstrainedCopy(overread.ToArray(), (index + 1), buff, 0, buff.Length);//get second part
                                    overread = new MemoryStream(buff);//write to overread
                                }
                                else
                                {
                                    overread = new MemoryStream();
                                }
                                return new NetworkEncoding(ms.ToArray()).GetBaseEncode();//return first part and other
                            }
                            else
                            {
                                ms.Write(buffer, 0, length);//no end...keep writing
                            }
                        }
                    }
                    catch (Exception e)
                    {

                    }
                }

                
            }

        }
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
