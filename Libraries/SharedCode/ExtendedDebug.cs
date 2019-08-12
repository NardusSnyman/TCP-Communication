using System;
using System.Collections.Generic;
using System.Text;

namespace ClientServer
{
    public class ExtendedDebug
    {
        public Action<string> errorDebug;
        public Action<string> mainProcessDebug;
        public Action<string> closeUpDebug;
        public Action<bool, long> packetInvoked;//upload:packet #
        public Action<long, long> uploadProgressDebug;//value:total
        public Action<long, long> downloadProgressDebug;//value:total
        public Action<TimeSpan, TimeSpan> activeTimeStallTime;//total time active, time stalled(not recieving information)

    }
    
}
