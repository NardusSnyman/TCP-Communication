using System;
using static ClientServer.EncodingClasses;

namespace ClientServer
{
    public class Command
    {
        public string operation;
        public Func<NetworkData, NetworkData> action;
        public Command(string operation, Func<NetworkData, NetworkData> action)
        {
            this.operation = operation;
            this.action = action;
        }
        public Command()
        {

        }
    }
}
