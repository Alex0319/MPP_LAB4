using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Logger;

namespace AssemblyModificationApp
{
    public class AssemblyModifier
    {
        private string path;
        private Instruction firstInstruction;

        public AssemblyModifier(string path)
        {
            this.path = path;
        }

        public void InjectToAssembly()
        {
            var assembly = AssemblyDefinition.ReadAssembly(path);
            var getCurrentMethodRef = assembly.MainModule.Import(typeof(MethodBase).GetMethod("GetCurrentMethod"));
            var getDeclaringTypeRef = assembly.MainModule.Import(typeof(MethodBase).GetMethod("get_DeclaringType"));
            var getCustomAttributeRef = assembly.MainModule.Import(typeof(Attribute).GetMethod("GetCustomAttribute", new Type[] { typeof(MethodInfo), typeof(Type) }));
            var getTypeFromHandleRef = assembly.MainModule.Import(typeof(Type).GetMethod("GetTypeFromHandle"));
            var methodBaseRef = assembly.MainModule.Import(typeof(MethodBase));
            var typeRef = assembly.MainModule.Import(typeof(Type));
            var objectRef = assembly.MainModule.Import(typeof(object));
            var logAttributeRef = assembly.MainModule.Import(typeof(LogAttribute));
            var logAttributeOnEnterRef = assembly.MainModule.Import(typeof(LogAttribute).GetMethod("OnEnter"));
            var logAttributeOnExitRef = assembly.MainModule.Import(typeof(LogAttribute).GetMethod("OnExit"));
            var dictionaryType = Type.GetType("System.Collections.Generic.Dictionary`2[System.String,System.Object]");
            var dictionaryStringObjectRef = assembly.MainModule.Import(dictionaryType);
            var dictionaryConstructorRef = assembly.MainModule.Import(dictionaryType.GetConstructor(Type.EmptyTypes));
            var dictionaryMethodAddRef = assembly.MainModule.Import(dictionaryType.GetMethod("Add"));

            foreach (var typeDef in assembly.MainModule.Types)
                if (typeDef.CustomAttributes.Where(attribute => attribute.AttributeType.Resolve().Name == "LogAttribute").FirstOrDefault() != null)
                    foreach (var method in typeDef.Methods)
                    {
                        method.Body.InitLocals = true;
                        var ilProc = method.Body.GetILProcessor();
                        var attributeVar = new VariableDefinition(logAttributeRef);
                        var currentMethodVar = new VariableDefinition(methodBaseRef);
                        var parametersVar = new VariableDefinition(dictionaryStringObjectRef);
                        var classVar = new VariableDefinition(typeRef);

                        if (!CheckMethodModify(ilProc, attributeVar, currentMethodVar, parametersVar, classVar))
                        {
                            ilProc.Body.Variables.Add(attributeVar);
                            ilProc.Body.Variables.Add(currentMethodVar);
                            ilProc.Body.Variables.Add(parametersVar);
                            ilProc.Body.Variables.Add(classVar);

                            firstInstruction = ilProc.Body.Instructions[0];
                            ilProc.InsertBefore(firstInstruction, Instruction.Create(OpCodes.Nop));

                            GetCurrentMethod(ilProc, getCurrentMethodRef, currentMethodVar);
                            GetClassOfCurrentMethod(ilProc, getDeclaringTypeRef, currentMethodVar, classVar);
                            CreateLogAttributeObject(ilProc, classVar, logAttributeRef, getTypeFromHandleRef, getCustomAttributeRef, attributeVar);
                            CreateDictionary(ilProc, dictionaryConstructorRef, parametersVar);
                            AddParametersInDictionary(method, ilProc, parametersVar, dictionaryMethodAddRef);
                            CallLogAttributeOnEnterMethod(ilProc, attributeVar, currentMethodVar, parametersVar, logAttributeOnEnterRef);
                            GetReturnValue(method, ilProc, objectRef, logAttributeOnExitRef, attributeVar);
                        }
                    }
            assembly.Write(path);
        }

        private void GetCurrentMethod(ILProcessor ilProc, MethodReference getCurrentMethodRef, VariableDefinition currentMethodVar)
        {
            ilProc.InsertBefore(firstInstruction, Instruction.Create(OpCodes.Call, getCurrentMethodRef));
            ilProc.InsertBefore(firstInstruction, Instruction.Create(OpCodes.Stloc, currentMethodVar));
        }

        private void GetClassOfCurrentMethod(ILProcessor ilProc, MethodReference getDeclaringTypeRef, VariableDefinition currentMethodVar,VariableDefinition classVar)
        {
            ilProc.InsertBefore(firstInstruction, Instruction.Create(OpCodes.Ldloc, currentMethodVar));
            ilProc.InsertBefore(firstInstruction, Instruction.Create(OpCodes.Callvirt, getDeclaringTypeRef));
            ilProc.InsertBefore(firstInstruction, Instruction.Create(OpCodes.Stloc, classVar));
        }

        private void CreateLogAttributeObject(ILProcessor ilProc,VariableDefinition classVar, TypeReference logAttributeRef, MethodReference getTypeFromHandleRef, MethodReference getCustomAttributeRef, VariableDefinition attributeVar)
        {
            ilProc.InsertBefore(firstInstruction, Instruction.Create(OpCodes.Ldloc, classVar));
            ilProc.InsertBefore(firstInstruction, Instruction.Create(OpCodes.Ldtoken, logAttributeRef));
            ilProc.InsertBefore(firstInstruction, Instruction.Create(OpCodes.Call, getTypeFromHandleRef));
            ilProc.InsertBefore(firstInstruction, Instruction.Create(OpCodes.Call, getCustomAttributeRef));
            ilProc.InsertBefore(firstInstruction, Instruction.Create(OpCodes.Castclass, logAttributeRef));
            ilProc.InsertBefore(firstInstruction, Instruction.Create(OpCodes.Stloc, attributeVar)); 
        }

        private void CreateDictionary(ILProcessor ilProc,MethodReference dictionaryConstructorRef, VariableDefinition parametersVar)
        {
            ilProc.InsertBefore(firstInstruction, Instruction.Create(OpCodes.Newobj, dictionaryConstructorRef));
            ilProc.InsertBefore(firstInstruction, Instruction.Create(OpCodes.Stloc, parametersVar));
        }

        private void AddParametersInDictionary(MethodDefinition method, ILProcessor ilProc, VariableDefinition parametersVar, MethodReference dictionaryMethodAddRef)
        {
            foreach (var argument in method.Parameters)
            {
                ilProc.InsertBefore(firstInstruction, Instruction.Create(OpCodes.Ldloc, parametersVar));
                ilProc.InsertBefore(firstInstruction, Instruction.Create(OpCodes.Ldstr, argument.Name));
                ilProc.InsertBefore(firstInstruction, Instruction.Create(OpCodes.Ldarg, argument));
                CheckArgument(ilProc,argument);
                if (argument.ParameterType.GetElementType().IsPrimitive)
                    ilProc.InsertBefore(firstInstruction, Instruction.Create(OpCodes.Box, argument.ParameterType.GetElementType()));
                ilProc.InsertBefore(firstInstruction, Instruction.Create(OpCodes.Callvirt, dictionaryMethodAddRef));
            }           
        }

        private void CheckArgument(ILProcessor ilProc, ParameterDefinition argument)
        {
            if (argument.ParameterType.IsByReference)
                switch(argument.ParameterType.Name.Replace("&",""))
                {
                    case "Char": ilProc.InsertBefore(firstInstruction, Instruction.Create(OpCodes.Ldind_I1));
                        break;
                    case "Int16": ilProc.InsertBefore(firstInstruction, Instruction.Create(OpCodes.Ldind_I2));
                        break;
                    case "Int32": ilProc.InsertBefore(firstInstruction, Instruction.Create(OpCodes.Ldind_I4));
                        break;
                    case "Int64": ilProc.InsertBefore(firstInstruction, Instruction.Create(OpCodes.Ldind_I8));
                        break;
                    case "UInt16": ilProc.InsertBefore(firstInstruction, Instruction.Create(OpCodes.Ldind_U2));
                        break;
                    case "UInt32": ilProc.InsertBefore(firstInstruction, Instruction.Create(OpCodes.Ldind_U4));
                        break;
                    case "Single": ilProc.InsertBefore(firstInstruction, Instruction.Create(OpCodes.Ldind_R4));
                        break;
                    case "Double": ilProc.InsertBefore(firstInstruction, Instruction.Create(OpCodes.Ldind_R8));
                        break;
                    default: ilProc.InsertBefore(firstInstruction, Instruction.Create(OpCodes.Ldind_Ref));
                        break;
                } 
        }

        private void CallLogAttributeOnEnterMethod(ILProcessor ilProc, VariableDefinition attributeVar, VariableDefinition currentMethodVar, VariableDefinition parametersVar, MethodReference logAttributeOnEnterRef)
        {
            ilProc.InsertBefore(firstInstruction, Instruction.Create(OpCodes.Ldloc, attributeVar));
            ilProc.InsertBefore(firstInstruction, Instruction.Create(OpCodes.Ldloc, currentMethodVar));
            ilProc.InsertBefore(firstInstruction, Instruction.Create(OpCodes.Ldloc, parametersVar));
            ilProc.InsertBefore(firstInstruction, Instruction.Create(OpCodes.Callvirt, logAttributeOnEnterRef));
        }

        private void GetReturnValue(MethodDefinition method, ILProcessor ilProc, TypeReference objectRef, MethodReference logAttributeOnExitRef, VariableDefinition attributeVar)
        {
            var returnValueVar = new VariableDefinition(objectRef);
            ilProc.Body.Variables.Add(returnValueVar);
            Instruction lastInstruction = ilProc.Body.Instructions.Last();
            
            if (!method.ReturnType.Name.Equals(typeof(void).Name))
            {
                var returnLocalVar = method.Body.Variables.First(var => var.VariableType.Name.Equals(method.ReturnType.Name));
                ilProc.InsertBefore(lastInstruction, Instruction.Create(OpCodes.Ldloc, returnLocalVar));
                if (returnLocalVar.VariableType.IsPrimitive || returnLocalVar.VariableType.IsValueType)
                    ilProc.InsertBefore(lastInstruction, Instruction.Create(OpCodes.Box, returnLocalVar.VariableType));
                ilProc.InsertBefore(lastInstruction, Instruction.Create(OpCodes.Stloc, returnValueVar));
            }
            ilProc.InsertBefore(lastInstruction, Instruction.Create(OpCodes.Ldloc, attributeVar));
            ilProc.InsertBefore(lastInstruction, Instruction.Create(OpCodes.Ldloc, returnValueVar));
            ilProc.InsertBefore(lastInstruction, Instruction.Create(OpCodes.Callvirt, logAttributeOnExitRef));
        }

        private bool CheckMethodModify(ILProcessor ilProc, VariableDefinition attributeVar, VariableDefinition currentMethodVar, VariableDefinition parametersVar, VariableDefinition classVar)
        {
            if (ilProc.Body.Variables.Count >= 4)
                if (ilProc.Body.Variables.Any(var => var.VariableType.Name.Equals(attributeVar.VariableType.Name)) &&
                    ilProc.Body.Variables.Any(var => var.VariableType.Name.Equals(currentMethodVar.VariableType.Name)) &&
                    ilProc.Body.Variables.Any(var => var.VariableType.Name.Equals(parametersVar.VariableType.Name)) &&
                    ilProc.Body.Variables.Any(var => var.VariableType.Name.Equals(classVar.VariableType.Name)))
                    return true;
            return false;
        }
    }
}
