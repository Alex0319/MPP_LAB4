using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.IO;
using System.Threading;

namespace Logger
{
    public class LogAttribute : Attribute
    {
        public virtual void OnEnter(MethodBase method, Dictionary<string, object> parameters)
        {
            WriteToFile(String.Format("CLASS: {{{0}}}. METHOD: {{{1}}}. PARAMETERS: {{{2}}}", method.DeclaringType.Name, method.Name, GetParameterValues(parameters)));
        }

        public virtual void OnExit(object returnValue)
        {
            if (returnValue != null)
                WriteToFile(String.Format(" and RETURNS {{{0}}}{1}", GetStringWithReturnValue(returnValue), Environment.NewLine));
            else
                WriteToFile(String.Format("{0}", Environment.NewLine));
        }

        protected void WriteToFile(string logString)
        {
            using (var fileStream = new FileStream("FileLog.txt", FileMode.Append))
            using (var streamWriter = new StreamWriter(fileStream))
            {
                streamWriter.Flush();
                streamWriter.Write(logString);
            }
        }

        private string GetParameterValues(Dictionary<string, object> parameters)
        {
            const int countOfDeletedSymbols = 2;
            StringBuilder stringBuilder = new StringBuilder();
            foreach (string parameter in parameters.Keys)
                stringBuilder.AppendFormat("{0} = {1}, ", parameter, parameters[parameter]);
            if (stringBuilder.Length > 0)
                stringBuilder.Remove(stringBuilder.Length - countOfDeletedSymbols, countOfDeletedSymbols);
            else
                stringBuilder.Append("no params");
            return stringBuilder.ToString();
        }

        private string GetStringWithReturnValue(object returnValue)
        {
            Type returnValueType = returnValue.GetType();
            if (returnValueType.IsValueType || returnValueType.IsClass)
                return String.Format("{0}: {{{1}}}", returnValueType.ToString(), GetParameterValues(GetDictionaryFromReturnFieldsAndProperties(returnValue, returnValueType)));
            if(returnValueType.IsEnum)
                return String.Format("{0}: {{{1}}}", returnValueType.ToString(), GetParameterValues(GetDictionaryFromEnum(returnValue, returnValueType)));
            return returnValue.ToString();
        }

        private Dictionary<string, object> GetDictionaryFromEnum(object returnValue, Type returnValueType)
        {
            var parameters = new Dictionary<string, object>();
            string[] returnEnumNames = returnValueType.GetEnumNames();
            Array returnEnumValues = returnValueType.GetEnumValues();
            for (int i = 0; i < returnEnumNames.Length; i++)
                parameters.Add(returnEnumNames[i], returnEnumValues.GetValue(i));
            return parameters;
        }

        private Dictionary<string, object> GetDictionaryFromReturnFieldsAndProperties(object returnValue,Type returnValueType)
        {
            var parameters = new Dictionary<string, object>();
            FieldInfo[] returnValueFields = returnValueType.GetFields(BindingFlags.Public
                                                                      | BindingFlags.Instance
                                                                      | BindingFlags.NonPublic
                                                                      | BindingFlags.Static);
            PropertyInfo[] returnValueProperties = returnValueType.GetProperties(BindingFlags.Public
                                                                      | BindingFlags.Instance
                                                                      | BindingFlags.NonPublic
                                                                      | BindingFlags.Static);
            int fieldsArrayLength = returnValueFields.Length, propertiesArrayLength = returnValueProperties.Length;
            for (int i = 0; i < fieldsArrayLength + propertiesArrayLength; i++)
                if (i < returnValueFields.Length)
                    parameters.Add(returnValueFields[i].Name, returnValueFields[i].GetValue(returnValue));
                else
                    parameters.Add(returnValueProperties[i - fieldsArrayLength].Name, returnValueProperties[i - fieldsArrayLength].GetValue(returnValue));
           return parameters;
        }
    }
}