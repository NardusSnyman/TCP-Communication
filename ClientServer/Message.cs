using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using static ClientServer.Utilities;

namespace ClientServer
{
    public class ServerMessage
    {
        public string message;
        public BaseEncode messagearray;
        public bool successful;
        public int length { get { return Bytes().GetnetworkEncoding().data.Length; } }
        public ServerMessage()
        {
            successful = false;
            message = "null";
            messagearray = new BaseEncode("null");
        }

        public ServerMessage(BaseEncode bytes)
        {
            var indexes = GetIndexes(bytes, separator1);
            byte[] new1 = new byte[indexes[0]];
            byte[] new2 = new byte[bytes.data.Length - indexes[0]];
            Array.ConstrainedCopy(bytes.data, 0, new1, 0, indexes[0]);
            Array.ConstrainedCopy(bytes.data, indexes[0], new2, 0, new2.Length);
            var data1 = new BaseEncode(new1);//arguments
            var data2 = new BaseEncode(new2);//sub-array
            byte[] src = data2.data;
            byte[] dst = new byte[src.Length - separator1.Length];
            Array.Copy(src, separator1.Length, dst, 0, dst.Length);
            var data3 = new BaseEncode(dst);//array
            messagearray = data3;
            message = data1.String().Split(separator2)[0];
            successful = Convert.ToBoolean(data1.String().Split(separator2)[1]);
        }
        public BaseEncode Bytes()
        {
            string arguments = $"{message}{separator2}{successful}{separator1}";
            MemoryStream ms = new MemoryStream();
            ms.Write(new BaseEncode(arguments).data);
            ms.Write(messagearray.data);
            var u = new BaseEncode(ms.ToArray());
            return u;
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
        public int length { get { return Bytes().GetnetworkEncoding().data.Length; } }
        public ClientMessage()
        {
            operation = "null";
            message = "null";
            messagearray = new BaseEncode("null");
        }

        public ClientMessage(BaseEncode bytes)
        {
            var indexes = GetIndexes(bytes, separator1);
            byte[] new1 = new byte[indexes[0]];
            byte[] new2 = new byte[bytes.data.Length - indexes[0]];
            Array.ConstrainedCopy(bytes.data, 0, new1, 0, indexes[0]);
            Array.ConstrainedCopy(bytes.data, indexes[0], new2, 0, new2.Length);
            var data1 = new BaseEncode(new1);//arguments
            var data2 = new BaseEncode(new2);//sub-array
            byte[] src = data2.data;
            byte[] dst = new byte[src.Length - separator1.Length];
            Array.Copy(src, separator1.Length, dst, 0, dst.Length);
            var data3 = new BaseEncode(dst);//array
            messagearray = data3;
            operation = data1.String().Split(separator2)[0];
            message =data1.String().Split(separator2)[1];
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
        public BaseEncode Bytes()
        {
            string arguments = $"{operation}{separator2}{message}{separator1}";
            MemoryStream ms = new MemoryStream();
            ms.Write(new BaseEncode(arguments).data);
            ms.Write(messagearray.data);
            var u = new BaseEncode(ms.ToArray());
            return u;
        }

    }
    public class NetworkEncoding
    {
        public byte[] data;
        public NetworkEncoding(byte[] data1)
        {
            data = data1;
        }
        public NetworkEncoding(string data1)
        {
            data = networkEncoding.GetBytes(data1);
        }
        public BaseEncode GetBaseEncode()
        {
            return new BaseEncode(Encoding.Convert(networkEncoding, baseEncoding, data));
        }
        public string String()
        {
            return networkEncoding.GetString(data);
        }
    }
    public class BaseEncode
    {
        public byte[] data;
        public BaseEncode(byte[] data1)
        {
            data = data1;
        }
        public NetworkEncoding GetnetworkEncoding()
        {
            return new NetworkEncoding(Encoding.Convert(baseEncoding, networkEncoding, data));
        }
        public BaseEncode(string data1)
        {
            data = baseEncoding.GetBytes(data1);
        }
        public string String()
        {
            return baseEncoding.GetString(data);
        }
    }
}
