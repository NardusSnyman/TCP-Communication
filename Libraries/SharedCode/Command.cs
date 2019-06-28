using System;
using System.Collections.Generic;
using System.Text;

namespace ClientServer
{
    public class Command
    {
        public string operation;
        public Func<string, string> action;
        public Command()
        {

        }
        public Command(string operation1, Func<string, string> func1)
        {
            operation = operation1;
            action = func1;
        }
    }
}
