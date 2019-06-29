using System;
using System.Collections.Generic;
using System.Text;
using static ClientServer.EncodingClasses;

namespace ClientServer
{
    public class Command
    {
        public string operation;
        public Func<BaseEncode, BaseEncode> action;
        public Command()
        {

        }
        public Command(string operation1, Func<BaseEncode, BaseEncode> func1)
        {
            operation = operation1;
            action = func1;
        }
    }
}
