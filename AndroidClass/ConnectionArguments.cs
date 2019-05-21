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
        public ConnectionArguments()
        {

        }
        public ConnectionArguments(string ip, int port, char separator, byte ender)
        {
            this.ip = ip;
            this.port = port;
            this.ender = ender;
            this.separator = separator;
        }
    }
}
