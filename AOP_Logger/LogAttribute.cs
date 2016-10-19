using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using Mono.Cecil;

namespace AopLogger
{
    public class LogAttribute : Attribute
    {
        private static Logger logger;

        public LogAttribute()
        {
            if(logger==null)
                logger = new Logger(4, new ILoggerTarget[] { new LoggerTargetFile("") });
        }

        public virtual void OnCallMethod(MethodBase method,Dictionary<string,object> parameters)
        {
            
        }
    }
}
