using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using static ClientServer.EncodingClasses;

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
}
