using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace TwainWeb.Standalone.App
{
    public static class AsyncMethods
    {
        public delegate void method(object obj);
        public static void asyncWithWaitTime<T>(T obj, string nameThread, method meth, int waitTime, AutoResetEvent waitHandle)
        {
            var thr = new Thread(new ParameterizedThreadStart(meth));
            thr.Name = nameThread;
            thr.Start(obj);
            waitHandle.WaitOne(waitTime);
            waitHandle.Reset();
            if (!thr.IsAlive)
            {
                thr.Abort();
                thr.Join();
            }            
        }
    }
}
