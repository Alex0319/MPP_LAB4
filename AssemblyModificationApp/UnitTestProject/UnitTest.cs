using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.IO;
using System.Reflection;
using System.Diagnostics;
using AssemblyModificationApp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTestProject
{
    [TestClass]
    public class UnitTest
    {
        [TestMethod]
        public void  TestMethod_CheckLogsFromTestTarget()
        {
            var expectedResults = new List<string>() {"{TestTargetClass}. METHOD: {.ctor}. PARAMETERS: {no params}",
                                                      "{TestTargetClass}. METHOD: {.ctor}. PARAMETERS: {field1 = 15, field2 = stringParam}",
                                                      "{TestTargetClass}. METHOD: {SetFields}. PARAMETERS: {field1 = 10, field2 = newStringParam0}",
                                                      "{TestTargetClass}. METHOD: {GetFields}. PARAMETERS: {no params} and RETURNS {TestTarget.Fields: {field1 = 10, field2 = newStringParam0}}",
                                                      "{TestTargetClass}. METHOD: {GetFieldSymbol}. PARAMETERS: {target = TestTarget.TestTargetClass, index = 0} and RETURNS {n}",
                                                      "{TestTargetClass}. METHOD: {SetFields}. PARAMETERS: {field1 = 30, field2 = newStringParam1}",
                                                      "{TestTargetClass}. METHOD: {GetFields}. PARAMETERS: {no params} and RETURNS {TestTarget.Fields: {field1 = 30, field2 = newStringParam1}}",
                                                      "{TestTargetClass}. METHOD: {GetFieldSymbol}. PARAMETERS: {target = TestTarget.TestTargetClass, index = 20} and RETURNS {\n}"};
            string path = "E:\\lAB\\5 семестр\\СПП\\MPP_LAB4\\AssemblyModificationApp\\TestTarget\\bin\\Debug\\TestTarget.exe";            
            
            AssemblyModifier assemblyModifier = new AssemblyModifier(path);
            assemblyModifier.InjectToAssembly();

            Process testTargetProcess=new Process();
            testTargetProcess.StartInfo = new ProcessStartInfo(path);          
            testTargetProcess.Start();
            testTargetProcess.WaitForExit();      

            List<string> actualLogs = GetFileLogs(Path.Combine(Path.GetDirectoryName(path),"FileLog.txt"));
            actualLogs.RemoveRange(0, actualLogs.Count - expectedResults.Count);
            for (int i = 0; i < actualLogs.Count;i++ )
                actualLogs[i] = actualLogs[i].Replace("\r\n","");
            CollectionAssert.AreEqual(expectedResults, actualLogs);            
        }

        private List<string> GetFileLogs(string path)
        {
            string allLogs;
            if (File.Exists(path))
            {
                using (var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read))
                   using (var streamReader = new StreamReader(fileStream))
                        allLogs = streamReader.ReadToEnd();
                return allLogs.Split(new string[] { "CLASS: " }, StringSplitOptions.RemoveEmptyEntries).ToList();
            }
            return null;
        }
    }
}
