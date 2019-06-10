using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace ClientServer
{
    public class ConnectionArguments
    {
        public string ip;
        public int port;
        public char separator;
        public byte ender;
        public int buffer_size;
        public ConnectionArguments()
        {

        }
        public ConnectionArguments(string ip, int port, char separator, byte ender, int buffer_size)
        {
            this.ip = ip;
            this.port = port;
            this.ender = ender;
            this.separator = separator;
            this.buffer_size = buffer_size;
        }
    }
}
