
using System;

namespace ClientServer
{
    public interface ThreadingUtil
    {
        public void BackgroundTask(Action Act);
        public void MainThreadTask(Action Act);

    }
}
