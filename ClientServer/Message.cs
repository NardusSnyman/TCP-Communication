using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

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
            messagearray = Encoding.ASCII.GetBytes("null");
        }

        public ServerMessage(MemoryStream input)
        {
            string data = Encoding.ASCII.GetString(input.ToArray());

            //get arguments
            string pre_arguments = data.Split("||")[0];
            message = pre_arguments.Split('|')[0];
            successful = Convert.ToBoolean(pre_arguments.Split('|')[1]);
            messagearray = Encoding.ASCII.GetBytes(data.Split("||")[1]);
        }

        public ServerMessage(string message1, bool successful1, byte[] messagearray1 = null)
        {
            if (message1 == string.Empty)
            {
                message = "null";
            }
            else message = message1;
            if (messagearray == null)
            {
                messagearray = Encoding.ASCII.GetBytes("null");
            }
            else messagearray = messagearray1;
            successful = successful1;
        }
        public static MemoryStream toStream(ServerMessage msg)
        {
            string arguments = $"{msg.message}|{msg.successful}||{Encoding.ASCII.GetString(msg.messagearray)}";
            return new MemoryStream(Encoding.ASCII.GetBytes(arguments));
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
            messagearray = Encoding.ASCII.GetBytes("null");
        }

        public ClientMessage(MemoryStream input)
        {
            string data = Encoding.ASCII.GetString(input.ToArray());
            
            //get arguments
            string pre_arguments = data.Split("||")[0];
            operation = pre_arguments.Split('|')[0];
            message = pre_arguments.Split('|')[1];
            messagearray = Encoding.ASCII.GetBytes(data.Split("||")[1]);
        }

        public ClientMessage(string operation1, string message1, byte[] messagearray1 = null)
        {
            operation = operation1;
            if(message1 == string.Empty)
            {
                message = "null";
            }else message = message1;
            if(messagearray == null)
            {
                messagearray = Encoding.ASCII.GetBytes("null");
            }else messagearray = messagearray1;
        }
        public static MemoryStream toStream(ClientMessage msg)
        {
            string arguments = $"{msg.operation}|{msg.message}||{Encoding.ASCII.GetString(msg.messagearray)}";
            return new MemoryStream(Encoding.ASCII.GetBytes(arguments));
        }
    }
}
