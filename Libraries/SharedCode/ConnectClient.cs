using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace ClientServer
{
    public class ConnectClient : TcpClient
    {
        public int port;
        public string ip;
        public void Reconnect()
        {
            try
            {
                this.Connect(ip, port);
            }
            catch (Exception e)
            {

            }
        }
    }
}
