using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using static ClientServer.EncodingClasses;

namespace ClientServer
{
    public interface ThreadDefiner
    {
       public void Define(Action<NetworData> act){
           act(x);
       }
    }
}
