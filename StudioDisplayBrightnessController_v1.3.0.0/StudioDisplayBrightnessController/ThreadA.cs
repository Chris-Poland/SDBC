using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace StudioDisplayBrightnessController
{
    class ThreadA
    {
        private volatile bool threadActive;
        public bool isThreadActive()
        {
            return this.threadActive;
        }
        public void setThreadActiveToFalse()
        {
            this.threadActive = false;
        }



        private long threadWatchdog;
        private Object threadWatchdogLock = new Object();
        public long getThreadWatchdog()
        {
            lock (threadWatchdogLock)
            {
                return this.threadWatchdog;
            }
        }
        public void updateThreadWatchdog()
        {
            lock (threadWatchdogLock)
            {
                this.threadWatchdog = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            }
        }





        public ThreadA()
        {
            threadActive = true;
            threadWatchdog = DateTimeOffset.Now.ToUnixTimeMilliseconds();
        }



    }
}
