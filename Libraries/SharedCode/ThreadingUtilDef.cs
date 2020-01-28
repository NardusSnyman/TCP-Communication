using ClientServer;
using System;
using System.Collections.Generic;
using System.Text;

namespace ClientServer
{
    class ThreadingUtilDef : ThreadingUtil
    {
        public void BackgroundTask(Action Act)
        {
            Act();
        }

        public void MainThreadTask(Action Act)
        {
            Act();
        }
    }
}
