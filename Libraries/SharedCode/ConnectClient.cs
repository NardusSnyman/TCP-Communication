using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace ClientServer
{
    public class ConnectClient
    {
        public TcpClient client;
        public ConnectionArguments args;

        public bool Connected = false;
        public int port;

        public ConnectClient()
        {
            client = new TcpClient();
            client.SendTimeout = 1;
            client.ReceiveTimeout = 1;
        }

        public void Reconnect()
        {
            try
            {
                client.Connect(args.ip, args.ports[0]);
                Connected = true;
            }
            catch (Exception e)
            {

            }
        }
        public void Close()
        {
            client.Close();
            Connected = false;
        }
        public void Connect(string ip, int port)
        {
            if (!client.Connected)
            {
                try
                {
                    client.Connect(ip, port);
                    this.port = port;
                    Connected = true;
                }
                catch (Exception e)
                {

                }
            }
        }
        public NetworkStream GetStream()
        {
            Reconnect();
            return client.GetStream();
        }
    }
}
