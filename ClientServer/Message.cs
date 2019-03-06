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
        public byte[] messagearray;
        public bool successful;
        public ServerMessage()
        {
            successful = false;
            message = "null";
            messagearray = GetBytes("null");
        }

        public ServerMessage(byte[] bytes)
        {
            string data = GetString(bytes);

            //get arguments
            message = data.Split(':')[0];
            successful = Convert.ToBoolean(data.Split(':')[1]);
            messagearray = GetBytes(data.Split(':')[2]);
        }
        
        public ServerMessage(string message1, bool successful1, byte[] messagearray1 = null)
        {
            if (message1 == string.Empty)
            {
                message = "null";
            }
            else message = message1;
            if (messagearray1 == null)
            {
                messagearray = GetBytes("null");
            }
            else messagearray = messagearray1;
            successful = successful1;
        }
        public static MemoryStream toStream(ServerMessage msg)
        {
            string arguments = $"{msg.message}:{msg.successful}:{GetString(msg.messagearray)}";
            var ms = new MemoryStream(GetBytes(arguments));
            return ms;
        }
    }

        public class ClientMessage
        {
            public string operation;
            public string message;
            public byte[] messagearray;
            public ClientMessage()
            {
                operation = "null";
                message = "null";
                messagearray = GetBytes("null");
            }

            public ClientMessage(byte[] bytes)
            {
                string data = GetString(bytes);

                //get arguments
                operation = data.Split(':')[0];
                message = data.Split(':')[1];
                messagearray = GetBytes(data.Split(":")[2]);
            }
        public ClientMessage(string operation1, string message1, byte[] messagearray1 = null)
            {
                operation = operation1;
                if (message1 == string.Empty)
                {
                    message = "null";
                }
                else message = message1;
                if (messagearray1 == null)
                {
                    messagearray = GetBytes("null");
                }
                else messagearray = messagearray1;
            }
            public static MemoryStream toStream(ClientMessage msg)
            {
                string arguments = $"{msg.operation}:{msg.message}:{GetString(msg.messagearray)}";
                var ms = new MemoryStream(GetBytes(arguments));
                return ms;
            }

        }
}
