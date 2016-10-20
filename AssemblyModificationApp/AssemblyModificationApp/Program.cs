using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Reflection;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Logger;

namespace AssemblyModificationApp
{
    class Program
    {
        static void Main(string[] args)
        {
            string path = "E:\\lAB\\5 семестр\\СПП\\MPP_LAB4\\AopTarget\\AopTarget\\bin\\Debug\\AopTarget.exe";
            if (/*args.Length > 0 && */File.Exists(path))
                InjectToAssembly(path);
        }

        static void InjectToAssembly(string path)
        {
            var assembly = AssemblyDefinition.ReadAssembly(path);
            var getCurrentMethodRef = assembly.MainModule.Import(typeof(MethodBase).GetMethod("GetCurrentMethod"));
            var getDeclaringTypeRef = assembly.MainModule.Import(typeof(MethodBase).GetMethod("get_DeclaringType"));
            var getCustomAttributeRef = assembly.MainModule.Import(typeof(Attribute).GetMethod("GetCustomAttribute", new Type[] { typeof(MethodInfo), typeof(Type) } ));
            var getTypeFromHandleRef = assembly.MainModule.Import(typeof(Type).GetMethod("GetTypeFromHandle"));
            var methodBaseRef = assembly.MainModule.Import(typeof(MethodBase));
            var typeRef = assembly.MainModule.Import(typeof(Type));
            var logAttributeRef = assembly.MainModule.Import(typeof(LogAttribute));
            var logAttributeOnCallMethodRef = assembly.MainModule.Import(typeof(LogAttribute).GetMethod("OnCallMethod"));
            var dictionaryType = Type.GetType("System.Collections.Generic.Dictionary`2[System.String,System.Object]");
            var dictionaryStringObjectRef = assembly.MainModule.Import(dictionaryType);
            var dictionaryConstructorRef = assembly.MainModule.Import(dictionaryType.GetConstructor(Type.EmptyTypes));
            var dictionaryMethodAddRef = assembly.MainModule.Import(dictionaryType.GetMethod("Add"));

            foreach (var typeDef in assembly.MainModule.Types)
                if (typeDef.CustomAttributes.Where(attribute => attribute.AttributeType.Resolve().Name == "LogAttribute").FirstOrDefault() != null)
                    foreach (var method in typeDef.Methods)
                    {
                        var ilProc = method.Body.GetILProcessor();
                        method.Body.InitLocals = true;
                        var attributeVar = new VariableDefinition(logAttributeRef);
                        var currentMethodVar = new VariableDefinition(methodBaseRef);
                        var parametersVar = new VariableDefinition(dictionaryStringObjectRef);
                        var classVar = new VariableDefinition(typeRef);

                        ilProc.Body.Variables.Add(attributeVar);
                        ilProc.Body.Variables.Add(currentMethodVar);
                        ilProc.Body.Variables.Add(parametersVar);
                        ilProc.Body.Variables.Add(classVar);
                        Instruction firstInstruction = ilProc.Body.Instructions[0];
                        ilProc.InsertBefore(firstInstruction, Instruction.Create(OpCodes.Nop));
                        ilProc.InsertBefore(firstInstruction,Instruction.Create(OpCodes.Call,getCurrentMethodRef));
                        ilProc.InsertBefore(firstInstruction,Instruction.Create(OpCodes.Stloc,currentMethodVar));
                        ilProc.InsertBefore(firstInstruction, Instruction.Create(OpCodes.Ldloc, currentMethodVar));
                        ilProc.InsertBefore(firstInstruction, Instruction.Create(OpCodes.Callvirt, getDeclaringTypeRef));
                        ilProc.InsertBefore(firstInstruction, Instruction.Create(OpCodes.Stloc, classVar));
                        ilProc.InsertBefore(firstInstruction, Instruction.Create(OpCodes.Ldloc, classVar));
                        ilProc.InsertBefore(firstInstruction, Instruction.Create(OpCodes.Ldtoken, logAttributeRef));
                        ilProc.InsertBefore(firstInstruction, Instruction.Create(OpCodes.Call, getTypeFromHandleRef));
                        ilProc.InsertBefore(firstInstruction, Instruction.Create(OpCodes.Call, getCustomAttributeRef));
                        ilProc.InsertBefore(firstInstruction, Instruction.Create(OpCodes.Castclass, logAttributeRef));
                        ilProc.InsertBefore(firstInstruction, Instruction.Create(OpCodes.Stloc, attributeVar));
                        ilProc.InsertBefore(firstInstruction, Instruction.Create(OpCodes.Newobj,dictionaryConstructorRef));
                        ilProc.InsertBefore(firstInstruction, Instruction.Create(OpCodes.Stloc, parametersVar));

                        foreach (var argument in method.Parameters)
                        {
                            ilProc.InsertBefore(firstInstruction, Instruction.Create(OpCodes.Ldloc, parametersVar));
                            ilProc.InsertBefore(firstInstruction, Instruction.Create(OpCodes.Ldstr, argument.Name));
                            ilProc.InsertBefore(firstInstruction, Instruction.Create(OpCodes.Ldarg, argument));
                            ilProc.InsertBefore(firstInstruction, Instruction.Create(OpCodes.Call, dictionaryMethodAddRef));
                        }
                        ilProc.InsertBefore(firstInstruction, Instruction.Create(OpCodes.Ldloc, attributeVar));
                        ilProc.InsertBefore(firstInstruction, Instruction.Create(OpCodes.Ldloc, currentMethodVar));
                        ilProc.InsertBefore(firstInstruction, Instruction.Create(OpCodes.Ldloc, parametersVar));
                        ilProc.InsertBefore(firstInstruction, Instruction.Create(OpCodes.Callvirt, logAttributeOnCallMethodRef));                
                    }
            assembly.Write(path);
        }
    }
}
