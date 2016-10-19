using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AopTarget;

namespace AopTarget
{
    [Log]
    public class TargetAopClass
    {
        public TargetAopClass(int parameter)
        {
        }

        public void First()
        {

        }

        public int Second(int parameter1, object parameter2)
        {
            return 10;
        }

    }
}
