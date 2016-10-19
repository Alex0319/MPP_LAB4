using System;

namespace AopLogger
{
    public class LogInfo
    {
        private string className,methodName,parameters,returns;

        public LogInfo(string className, string methodName, string parameters,string returns)
        {
            this.className = className;
            this.methodName = methodName;
            this.parameters = parameters;
            this.returns = returns;
        }

        public string ConvertToString()
        {
            if(returns==null)
                return String.Format("CLASS: {{0}}. METHOD: {{1}}. PARAMETERS: {{2}}{3}",className, methodName, parameters,Environment.NewLine);
            return String.Format("CLASS: {{0}}. METHOD: {{1}}. PARAMETERS: {{2}} and RETURNS {{3}}{4}", className, methodName, parameters,returns,Environment.NewLine);
        }
    }
}
