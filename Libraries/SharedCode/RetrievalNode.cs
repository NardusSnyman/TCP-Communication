using System;
using System.Collections.Generic;
using System.Text;
using static ClientServer.EncodingClasses;

namespace ClientServer
{
    public class RetrievalNode
    {
        public Action<NetworkData> direct;
        public Action<NetworkData> packetSend;
        public string motive;
    }
}
