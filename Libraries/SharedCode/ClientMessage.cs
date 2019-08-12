using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using static ClientServer.EncodingClasses;

namespace ClientServer
{
    public class ClientMessage
    {
        public string operation;
        public string message;
        public Action<NetworkData> completed;
        public ExtendedDebug debug;
        public Action failed;
    }
}
