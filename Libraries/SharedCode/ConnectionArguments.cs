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
        public TimeSpan timeout_time;//timeout before the client or server quits reading because no data is read
        public TimeSpan timeout_on_recent;//timeout before quit if a message is read beforehand
        public ConnectionArguments()
        {

        }
        public ConnectionArguments(string ip, int port, int buffer_size, TimeSpan timeout_time, TimeSpan timeout_on_recent)
        {
            this.ip = ip;
            this.port = port;
            this.buffer_size = buffer_size;
            this.timeout_on_recent = timeout_on_recent;
            this.timeout_time = timeout_time;
        }
        public static ConnectionArguments fromLocal(int port, int buffer_size, TimeSpan timeout_time, TimeSpan timeout_on_recent)
        {
            return new ConnectionArguments(Dns.GetHostName(), port, buffer_size, timeout_time, timeout_on_recent);
        }
    }
}
