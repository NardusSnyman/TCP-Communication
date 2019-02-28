using System;
using System.Collections.Generic;
using System.Text;

namespace ClientServer
{
    public class Command
    {
        public string operation;
        public Func<ClientMessage, ServerMessage> action;
        public Command()
        {

        }
        public Command(string operation1, Func<ClientMessage, ServerMessage> func1)
        {
            operation = operation1;
            action = func1;
        }
    }
}
