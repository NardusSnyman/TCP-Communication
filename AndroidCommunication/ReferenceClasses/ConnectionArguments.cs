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
        public ConnectionArguments()
        {

        }
        public ConnectionArguments(string ip_, int port_)
        {
            ip = ip_;
            port = port_;
        }
    }
}
