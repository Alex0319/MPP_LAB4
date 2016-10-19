using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.IO;

namespace AopTarget
{
    public class LogAttribute : Attribute
    {
        public virtual void OnCallMethod(MethodBase method,Dictionary<string,object> parameters)
        {
            using(var fileStream=new FileStream("FileLog.txt",FileMode.Append,FileAccess.Write))
                using (var streamWriter = new StreamWriter(fileStream))
                {
                    streamWriter.WriteLine("CLASS: {{0}}. METHOD: {{1}}. PARAMETERS: {{2}}{3}",method.Module.Name,method.Name,GetParameterValues(parameters),GetMethodReturnValue(method));
                }
        }

        private string GetParameterValues(Dictionary<string, object> parameters)
        {
            StringBuilder stringBuilder=new StringBuilder();
            foreach (string parameter in parameters.Keys)
                stringBuilder.AppendFormat("{0} = {1},",parameter,parameters[parameter]);
            stringBuilder.Remove(stringBuilder.Length - 1,1);
            return stringBuilder.ToString();
        }

        private string GetMethodReturnValue(MethodBase method)
        {
            return null;
        }
    }
}
