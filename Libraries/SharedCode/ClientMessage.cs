using System;
using System.Collections.Generic;
using System.Text;

namespace ClientServer
{
    public class ClientMessage
    {
        public string operation;
        public string message;
        public Action<string> completed;
        public ExtendedDebug debug;
        public Action failed;
    }
}
