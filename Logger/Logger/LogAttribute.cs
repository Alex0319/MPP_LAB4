using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.IO;
using System.Threading;

namespace Logger
{
    public class LogAttribute: Attribute
    {
        public virtual void OnEnter(MethodBase method,Dictionary<string,object> parameters)
        {
            WriteToFile(String.Format("CLASS: {{{0}}}. METHOD: {{{1}}}. PARAMETERS: {{{2}}}", method.DeclaringType.Name, method.Name, GetParameterValues(parameters)));
        }

        public virtual void OnExit(object returnValue)
        {
            if(returnValue != null)
                WriteToFile(String.Format(" and RETURNS {{{0}}}{1}",returnValue,Environment.NewLine));
            else
                WriteToFile(String.Format("{0}",Environment.NewLine));            
        }

        protected void WriteToFile(string logString)
        {
            using (var fileStream = new FileStream("FileLog.txt", FileMode.Append))
                using (var streamWriter = new StreamWriter(fileStream))
                {
                    streamWriter.Write(logString);
                    streamWriter.Flush();
                }
        }

        private string GetParameterValues(Dictionary<string, object> parameters)
        {
            const int countOfDeletedSymbols = 2;
            StringBuilder stringBuilder=new StringBuilder();
            foreach (string parameter in parameters.Keys)
                stringBuilder.AppendFormat("{0} = {1}, ",parameter,parameters[parameter]);
            if (stringBuilder.Length > 0)
                stringBuilder.Remove(stringBuilder.Length - countOfDeletedSymbols, countOfDeletedSymbols);
            else
                stringBuilder.Append("no params");
            return stringBuilder.ToString();
        }
    }
}
