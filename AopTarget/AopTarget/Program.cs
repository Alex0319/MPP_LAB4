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
            TargetAopClass target = new TargetAopClass(17);
            target.First();
            target.First();
            target.First();
            target.First();
            target.Second(13, new object());
            target.Second(-13, new List<int>());
            target.Second(11, new TimeSpan());
            target.Second(16, new char());
            target.Second(110909, new TargetAopClass(7));
            target.Second(1, new int());
            TargetAopClass.Third();
        }
    }
}
