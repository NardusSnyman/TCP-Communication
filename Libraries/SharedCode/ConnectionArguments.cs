using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ClientServer
{
    public class ConnectionArguments
    {
        public string ip;
        public int buffer_size;
        public TimeSpan timeout_time = TimeSpan.FromSeconds(5);//timeout before the client or server quits reading because no data is read
        public TimeSpan timeout_on_recent = TimeSpan.FromMilliseconds(300);//timeout before quit if a message is read beforehand


        public int server_reconnect_attempts = 3;
        public List<int> ports;
        public ConnectionArguments()
        {

        }
        public ConnectionArguments(string ip, int port, int buffer_size)
        {
            this.ip = ip;
            this.ports = new List<int> { port };
            this.buffer_size = buffer_size;
            
            while((this.buffer_size / EncodingClasses.expanded_length).ToString().Contains("."))
            {
                this.buffer_size++;
            }
        }
        public ConnectionArguments(string ip, List<int> ports, int buffer_size)
        {
            this.ip = ip;
            this.ports = ports;
            this.buffer_size = buffer_size;

            while ((this.buffer_size / EncodingClasses.expanded_length).ToString().Contains("."))
            {
                this.buffer_size++;
            }
        }
        public static ConnectionArguments fromLocal(List<int> ports, int buffer_size)
        {
            ConnectionArguments args = new ConnectionArguments();
            args.ip = GetLocalIPAddress();
            args.ports = ports;
            args.buffer_size = buffer_size;
            return args;
        }
        public static ConnectionArguments fromLocal(int port, int buffer_size)
        {
            ConnectionArguments args = new ConnectionArguments();
            args.ip = GetLocalIPAddress();
            args.ports = new List<int> { port };
            args.buffer_size = buffer_size;
            return args;
        }
        private static string GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            throw new Exception("No network adapters with an IPv4 address in the system!");
        }
    }
}
