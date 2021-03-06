﻿using System;
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
        public bool return_;
        public NetworkData message;
        public Action<NetworkData> completed;
        public Action<NetworkData> onPacketRecieved;
        public Action failed;
        public Action<double> progress;
    }
}
