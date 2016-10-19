using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AopLogger
{
    class Program
    {
        static void Main(string[] args)
        {
            Logger log=new Logger(3,null);
            Console.ReadKey();
        }
    }
}
