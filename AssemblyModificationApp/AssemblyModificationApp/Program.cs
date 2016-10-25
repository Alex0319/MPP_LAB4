using System;
using System.IO;

namespace AssemblyModificationApp
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length > 0)
                if (File.Exists(args[0]))
                {
                    AssemblyModifier assemblyModifier = new AssemblyModifier(args[0]);
                    assemblyModifier.InjectToAssembly();
                    Console.WriteLine("Inject to assembly was successfully");
                }
                else
                    Console.WriteLine("No such file {0}", args[0]);
            else
                Console.WriteLine("Error: Input path to assembly as paramater");
        }
    }
}
