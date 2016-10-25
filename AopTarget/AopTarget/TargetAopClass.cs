using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using Logger;

namespace AopTarget
{
    [Log("FileLog.txt")]
    public class TargetAopClass
    {
        public TargetAopClass(int parameter)
        {
        }

        public void First()
        {
            Second(6, new object());
        }

        public object Second(int parameter1, object parameter2)
        {
            return new Random();
        }

        public static int Third()
        {
            return 10;
        }
    }
}
