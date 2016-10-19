using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace AopLogger
{
    public class LoggerTargetFile: ILoggerTarget
    {
        private FileStream fileStream;

        public LoggerTargetFile(string filename)
        {
            fileStream = new FileStream(filename, FileMode.Append, FileAccess.Write);
        }

        public bool Flush(LogInfo logInfo)
        {
            Write(Encoding.Default.GetBytes(logInfo.ConvertToString().ToArray()));
            fileStream.Flush();
            return true;
        }

        public async Task<bool> FlushAsync(LogInfo logInfo)
        {
            Write(Encoding.Default.GetBytes(logInfo.ConvertToString().ToArray()));
            await fileStream.FlushAsync();
            return true;
        }

        public void Write(byte[] log)
        {
            fileStream.Write(log, 0, log.Length);
        }

        public void Close()
        {
            fileStream.Close();
            fileStream.Dispose(); 
        }
    }
}
