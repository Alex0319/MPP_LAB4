using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestTarget
{
    class Program
    {
        static void Main(string[] args)
        {
            
            List<TestTargetClass> targetsList = new List<TestTargetClass>() { new TestTargetClass(), new TestTargetClass(15, "stringParam") };
            for(int i=0;i<targetsList.Count;i++)
            {
                int temp = 10 + i * 20;
                targetsList[i].SetFields(ref temp,"newStringParam"+i);
                targetsList[i].GetFields();
                TestTargetClass.GetFieldSymbol(targetsList[i],i * 20);
            }
        }
    }
}
