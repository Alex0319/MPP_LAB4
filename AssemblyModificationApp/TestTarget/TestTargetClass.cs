using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Logger;

namespace TestTarget
{
    [Log("E:\\lAB\\5 семестр\\СПП\\MPP_LAB4\\AssemblyModificationApp\\TestTarget\\bin\\Debug\\FileLog.txt")]
    public class TestTargetClass
    {
        private int classField1;
        private string classField2;

        public TestTargetClass()
        {
            classField1 = 0;
            classField2 = null;
        }

        public TestTargetClass(int field1,string field2)
        {
            classField1 = field1;
            classField2 = field2;
        }

        public Fields GetFields()
        {
            return new Fields(classField1,classField2);
        }

        public void SetFields(ref int field1, string field2)
        {
            classField1 = field1;
            classField2 = field2;
        }

        public static char GetFieldSymbol(TestTargetClass target, int index)
        {
            try
            {
                if (index < target.classField2.Length)
                    return target.classField2[index];
                return '\n';
            }
            finally
            {
            }
        }
    }

    public struct Fields
    {
        public Fields(int field1, string field2)
        {
            this.field1 = field1;
            this.field2 = field2;
        }

        int field1;
        string field2;
    }
}
