using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AopLogger
{
    public class Logger: ILogger
    {
        private int bufferLimit;
        private ILoggerTarget[] target;
        private object locker=new object();
        private List<LogInfo> logInfoList;
        private volatile int bufferId=0;
        private volatile int currentBufferId=0;

        private class ThreadInfo
        {
            public List<LogInfo> logs;
            public int threadId;
        }

        public Logger(int bufferLimit, ILoggerTarget[] target)
        {
            this.bufferLimit = bufferLimit;
            this.target = target;
            logInfoList = new List<LogInfo>();
        }

        public void Log(LogInfo logInfo)
        {
            if (logInfoList.Count == bufferLimit)
            {
                ThreadPool.QueueUserWorkItem(FlushLogsInAllTargets, new ThreadInfo { logs = logInfoList, threadId = bufferId++});
                logInfoList=new List<LogInfo>();
            }
            logInfoList.Add(logInfo);       
        }

        private void FlushLogsInAllTargets(object objThreadInfo)
        {
            var threadInfo=(ThreadInfo)objThreadInfo;
            var logsList = threadInfo.logs;
            Monitor.Enter(locker);
            try
            {
                while(threadInfo.threadId!=currentBufferId)   
                    Monitor.Wait(locker);
                foreach (ILoggerTarget currentTarget in target)
                    foreach (LogInfo log in logsList)
                        currentTarget.Flush(log);
                currentBufferId++;
                Monitor.PulseAll(locker);
            }
            finally 
            {
                Monitor.Exit(locker);
            }
        }

        public void FlushRemainLogs()
        {
            ThreadPool.QueueUserWorkItem(FlushLogsInAllTargets, new ThreadInfo { logs = logInfoList, threadId = bufferId++ });
            Monitor.Enter(locker);
            try
            {
                while (bufferId != currentBufferId)
                    Monitor.Wait(locker);
            }
            finally
            {
                Monitor.Exit(locker);
            }
            foreach (ILoggerTarget currentTarget in target)
                currentTarget.Close();
        }
    }
}
