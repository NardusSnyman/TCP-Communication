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
        public TimeSpan timeout_time = TimeSpan.FromSeconds(5);//timeout before the client or server quits reading because no data is read
        public TimeSpan timeout_on_recent = TimeSpan.FromMilliseconds(300);//timeout before quit if a message is read beforehand
        public int server_reconnect_attempts = 3;
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
        public static ConnectionArguments fromExternalIP(int port, int buffer_size, TimeSpan timeout_time, TimeSpan timeout_on_recent)
        {
            string externalip = new WebClient().DownloadString("http://icanhazip.com");
            return new ConnectionArguments(externalip, port, buffer_size) { timeout_time=timeout_time, timeout_on_recent=timeout_on_recent };
            
        }
    }
}
