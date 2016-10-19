using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AopTarget
{
    class Program
    {
        static void Main(string[] args)
        {
            TargetAopClass target = new TargetAopClass(6);
            target.First();
            target.Second(1, new object());
        }
    }
}
