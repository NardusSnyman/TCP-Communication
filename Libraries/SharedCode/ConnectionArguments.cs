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
        public int buffer_size;
        public ConnectionArguments()
        {

        }
        public ConnectionArguments(string ip, int port, int buffer_size)
        {
            this.ip = ip;
            this.port = port;
            this.buffer_size = buffer_size;
        }
        public static ConnectionArguments fromLocal(int port, int buffer_size)
        {
            return new ConnectionArguments(Dns.GetHostName(), port, buffer_size);
        }
    }
}
