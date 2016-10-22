using System;
using System.IO;

namespace AssemblyModificationApp
{
    class Program
    {
        static void Main(string[] args)
        {
            string path = "E:\\lAB\\5 семестр\\СПП\\MPP_LAB4\\AopTarget\\AopTarget\\bin\\Debug\\AopTarget.exe";
            if (/*args.Length > 0 && */File.Exists(path))
            {
                AssemblyModifier assemblyModifier = new AssemblyModifier(path);
                assemblyModifier.InjectToAssembly();
            }
        }
    }
}
