﻿using System;
using System.IO;
using System.Collections.Generic;

using Mono.Cecil;
using Mono.Cecil.Cil;

using Java.Interop.Tools.Cecil;

namespace Xamarin.Android.Tools.JniMarshalMethodGenerator
{
	public class TypeMover
	{
		AssemblyDefinition Source { get; }
		AssemblyDefinition Destination { get; }
		string DestinationPath { get; }
		Dictionary<string, System.Reflection.Emit.TypeBuilder> Types { get; }
		DirectoryAssemblyResolver Resolver { get; }

		MethodReference consoleWriteLine;
		TypeDefinitionCache cache;

		public TypeMover (AssemblyDefinition source, AssemblyDefinition destination, string destinationPath, Dictionary<string, System.Reflection.Emit.TypeBuilder> types, DirectoryAssemblyResolver resolver, TypeDefinitionCache cache)
		{
			Source = source;
			Destination = destination;
			DestinationPath = destinationPath;
			Types = types;
			Resolver = resolver;
			this.cache = cache;

			if (App.Debug) {
				consoleWriteLine = GetSingleParameterMethod (Destination.MainModule, "mscorlib", "System.Console", "WriteLine", "System.String");
				if (consoleWriteLine == null) {
					App.Warning (Message.WarningUnableToFindSCWriteLine);
					App.Debug = false;
				}
			}
		}

		public void Move ()
		{
			int movedTypesCount = 0;

			typeMap.Clear ();
			resolvedTypeMap.Clear ();

			foreach (var type in Types.Values) {
				Move (type);
				movedTypesCount++;
			}

			if (movedTypesCount <= 0) {
				if (App.Verbose)
					App.Information ("No type was moved => nothing to write, no new assembly created.");
				return;
			}

			Destination.Write (DestinationPath, new WriterParameters () { WriteSymbols = Destination.MainModule.HasSymbols });

			if (App.Verbose)
				App.ColorWriteLine ($"Wrote updated {Destination.MainModule.FileName} assembly", ConsoleColor.Cyan);
		}

		public static readonly string NestedName = "__<$>_jni_marshal_methods";

		Dictionary<string, MethodReference> newHelperMethods;

		bool TypeIsEmptyOrHasOnlyDefaultConstructor (TypeDefinition type)
		{
			return !type.HasMethods || (type.Methods.Count == 1 && type.Methods [0].IsConstructor);
		}

		void Move (Type type)
		{
			var typeSrc = Source.MainModule.GetType (type.GetCecilName ());
			var typeDst = Destination.MainModule.GetType (type.GetCecilName ());

			List<TypeDefinition> toRemove = null;
			foreach (var nestedType in typeDst.NestedTypes) {
				if (nestedType.Name == NestedName) {
					if (toRemove == null)
						toRemove = new List<TypeDefinition> ();

					toRemove.Add (nestedType);
				}
			}

			if (toRemove != null) {
				foreach (var t in toRemove) {
					App.ColorWriteLine ($"Removing original '{t.GetAssemblyQualifiedName (cache)}' type. (forced)", ConsoleColor.Cyan);
					typeDst.NestedTypes.Remove (t);
				}
			}

			var jniType = new TypeDefinition ("", NestedName, TypeAttributes.NestedPrivate | TypeAttributes.Sealed);

			if (TypeIsEmptyOrHasOnlyDefaultConstructor (typeSrc))
				return;

			if (App.Verbose) {
				Console.Write ($"Moving type ");
				App.ColorWrite ($"{typeSrc.FullName},{typeSrc.Module.FileName}", ConsoleColor.Yellow);
				Console.Write (" to ");
				App.ColorWriteLine ($"{Destination.MainModule.FileName}", ConsoleColor.Yellow);
			}

			jniType.BaseType = GetUpdatedType (typeSrc.BaseType, Destination.MainModule);
			typeDst.NestedTypes.Add (jniType);


			newHelperMethods = new Dictionary<string, MethodReference> ();

			foreach (var m in typeSrc.Methods) {
				if (m.Name == "__RegisterNativeMembers")
					continue;
				var newMethod = Duplicate (m, Destination.MainModule, typeDst);
				AddMethod (jniType, newMethod);

				newHelperMethods [newMethod.Name] = newMethod;
			}

			foreach (var m in typeSrc.Methods) {
				if (m.Name != "__RegisterNativeMembers")
					continue;

				AddMethod (jniType, Duplicate (m, Destination.MainModule, typeDst));
			}
		}

		void AddMethod (TypeDefinition type, MethodDefinition method)
		{
			type.Methods.Add (method);

			if (App.Verbose) {
				Console.Write ("Moved method ");
				App.ColorWriteLine ($"{method}", ConsoleColor.Green);
			}
		}

		static Dictionary<TypeReference, TypeReference> typeMap = new Dictionary<TypeReference, TypeReference> ();
		static Dictionary<TypeReference, TypeDefinition> resolvedTypeMap = new Dictionary<TypeReference, TypeDefinition> ();

		static TypeDefinition Resolve (TypeReference type)
		{
			if (resolvedTypeMap.ContainsKey (type))
				return resolvedTypeMap [type];

			var resolved = type.Resolve ();

			resolvedTypeMap [type] = resolved;

			return resolved;
		}

		static TypeReference GetUpdatedArray (TypeReference type, TypeReference newReference)
		{
			if (!type.IsArray)
				return type;

			var t = type;
			var stack = new Stack<ArrayType> ();

			do {
				var ta = t as ArrayType;

				stack.Push (ta);
				t = ta.ElementType;
			} while (t.IsArray);

			while (stack.Count > 0) {
				var ta = stack.Pop ();
				var na = new ArrayType (newReference, ta.Rank);

				if (ta.Rank > 1) {
					for (int i = 0; i < ta.Rank; i++) {
						na.Dimensions [i] = ta.Dimensions [i];
					}
				}

				newReference = na;
			}

			return newReference;
		}

		static TypeReference GetUpdatedType (TypeReference type, ModuleDefinition module)
		{
			if (typeMap.ContainsKey (type))
				return typeMap [type];

			if (type is GenericInstanceType giType)
				return GetUpdatedGenericType (giType, module);

			var td = Resolve (type);

			var tr = td.Module.FileName != module.FileName
				? module.ImportReference (type)
				: module.GetType (td.FullName);

			if (type.IsArray && td.Module.FileName == module.FileName)
				tr = GetUpdatedArray (type, tr);

			typeMap [type] = tr;

			return tr;
		}

		static TypeReference GetUpdatedGenericType (GenericInstanceType type, ModuleDefinition module)
		{
			if (typeMap.ContainsKey (type))
				return typeMap [type];

			var td = Resolve (type);
			var newType = new GenericInstanceType (GetUpdatedType (td, module));

			if (type.HasGenericArguments)
				foreach (var ga in type.GenericArguments)
					newType.GenericArguments.Add (GetUpdatedType (ga, module));

			if (type.HasGenericParameters)
				foreach (var gp in type.GenericParameters)
					newType.GenericParameters.Add (gp);

			var tr = td.Module.FileName != module.FileName
				? module.ImportReference (newType)
				: module.GetType (newType.FullName);

			if (type.IsArray)
				tr = GetUpdatedArray (type.ElementType, tr);

			typeMap [type] = tr;

			return tr;
		}


		static Dictionary<MethodReference, MethodReference> methodMap = new Dictionary<MethodReference, MethodReference> ();
		static Dictionary<MethodReference, MethodDefinition> resolvedMethodMap = new Dictionary<MethodReference, MethodDefinition> ();

		static MethodDefinition ResolveMethod (MethodReference method)
		{
			if (resolvedMethodMap.ContainsKey (method))
				return resolvedMethodMap [method];

			var resolved = method.Resolve ();

			resolvedMethodMap [method] = resolved;

			return resolved;
		}

		static MethodReference GetUpdatedMethod (MethodReference method, ModuleDefinition module)
		{
			if (methodMap.ContainsKey (method))
				return methodMap [method];

			MethodDefinition md = ResolveMethod (method.IsGenericInstance ? (method as GenericInstanceMethod).ElementMethod : method);
			MethodReference mr;

			if (method.IsGenericInstance) {
				var newGenericMethod = new GenericInstanceMethod (md);

				var genericInstance = method as GenericInstanceMethod;
				if (genericInstance.HasGenericArguments)
					foreach (var ga in genericInstance.GenericArguments)
						newGenericMethod.GenericArguments.Add (GetUpdatedType (ga, module));

				mr = module.ImportReference (newGenericMethod);
			} else
				mr = module.ImportReference (md.Module != null && md.Module.FileName == module.FileName ? md : method);

			foreach (var p in mr.Parameters)
				p.ParameterType = GetUpdatedType (p.ParameterType, module);

			if (method.DeclaringType != null && !method.DeclaringType.HasGenericParameters)
				methodMap [method] = mr;

			return mr;
		}

		static FieldReference GetUpdatedField (FieldReference fr, ModuleDefinition module)
		{
			FieldReference newField = new FieldReference (fr.Name, GetUpdatedType (fr.FieldType, module));
			newField.DeclaringType = GetUpdatedType (fr.DeclaringType, module);

			return newField;
		}

		Instruction GetUpdatedInstruction (Instruction il, ModuleDefinition module)
		{
			if (il.Operand == null)
				return Instruction.Create (il.OpCode);

			if (il.Operand is MethodReference mr)
				return Instruction.Create (il.OpCode, GetUpdatedMethod (mr, module));

			if (il.Operand is GenericInstanceType giType)
				return Instruction.Create (il.OpCode, GetUpdatedGenericType (giType, module));

			if (il.Operand is TypeReference tr)
				return Instruction.Create (il.OpCode, GetUpdatedType (tr, module));

			if (il.Operand is FieldReference fr)
				return Instruction.Create (il.OpCode, GetUpdatedField (fr, module));

			return il;
		}

		static ExceptionHandler GetUpdatedExceptionHandler (ExceptionHandler eh, Dictionary<Instruction, Instruction> instructionMap, ModuleDefinition module)
		{
			var handler = new ExceptionHandler (eh.HandlerType);

			if (handler.CatchType != null)
				handler.CatchType = GetUpdatedType (eh.CatchType, module);

			if (eh.TryStart != null)
				handler.TryStart = instructionMap [eh.TryStart];

			if (eh.TryEnd != null)
				handler.TryEnd = instructionMap [eh.TryEnd];

			if (eh.FilterStart != null)
				handler.FilterStart = instructionMap [eh.FilterStart];

			if (eh.HandlerStart != null)
				handler.HandlerStart = instructionMap [eh.HandlerStart];

			if (eh.HandlerEnd != null)
				handler.HandlerEnd = instructionMap [eh.HandlerEnd];

			return handler;
		}

		MethodReference GetActionConstructor (TypeReference type, ModuleDefinition module)
		{
			var td = Resolve (type);
			if (!td.HasMethods)
				return null;

			foreach (var m in td.Methods) {
				if (m.IsConstructor && m.HasParameters && m.Parameters.Count == 2 && m.Parameters [0].ParameterType.FullName == "System.Object" && m.Parameters [1].ParameterType.FullName == "System.IntPtr") {
					var mr = GetUpdatedMethod (m, module);
					if (type is GenericInstanceType)
						mr.DeclaringType = type;
					return mr;
				}
			}
			return null;
		}

		bool AnalyzeAndImprove (Mono.Collections.Generic.Collection<Instruction> instructions, Mono.Collections.Generic.Collection<Instruction> newInstructions, int idx, string typeName, out int skipCount, ModuleDefinition module)
		{
			var idxStart = idx;
			var il = instructions [idx++];

			skipCount = 0;

			if (il.OpCode == OpCodes.Ldstr && il.Operand is string opStr && opStr != null && opStr == typeName) {
				il = instructions [idx++];
				if (il.OpCode != OpCodes.Call || !(il.Operand is MethodReference opMR) || opMR.Name != "GetType" || opMR.DeclaringType.FullName != "System.Type")
					return false;

				il = instructions [idx++];
				if (il.OpCode != OpCodes.Stloc_0)
					return false;

				skipCount = 2;

				if (App.Debug) {
					newInstructions.Add (Instruction.Create (OpCodes.Ldstr, $"Registering JNI marshal methods in {opStr}"));
					newInstructions.Add (Instruction.Create (OpCodes.Call, module.ImportReference (consoleWriteLine)));
				}

				return true;
			}

			if (il.OpCode == OpCodes.Dup && instructions.Count > idxStart + 10) {
				il = instructions [idx++];
				if (!il.OpCode.ToString ().StartsWith ("ldc.i4", StringComparison.InvariantCulture))
					return false;

				il = instructions [idx++];
				if (il.OpCode != OpCodes.Ldstr)
					return false;

				il = instructions [idx++];
				if (il.OpCode != OpCodes.Ldstr)
					return false;

				MethodReference constructor;
				TypeReference delegateType;
				var customDelegate = false;
				il = instructions [idx++];
				if (il.OpCode == OpCodes.Ldstr && il.Operand is string delegateTypeName) {
					il = instructions [idx++];
					if (!il.OpCode.ToString ().StartsWith ("ldc.i4", StringComparison.InvariantCulture))
						return false;

					il = instructions [idx++];
					if (il.OpCode != OpCodes.Call || !(il.Operand is MethodReference opMR2) || opMR2.Name != "GetType")
						return false;

					delegateType = module.GetType (delegateTypeName);
					if (delegateType == null) {
						var t = Type.GetType (delegateTypeName);
						if (t == null)
							return false;

						delegateType = GetType (t.Assembly.GetName ().ToString (), delegateTypeName);
						if (delegateType == null)
							return false;
					}

					skipCount = 11;
					customDelegate = true;
				} else if (il.OpCode == OpCodes.Ldtoken && il.Operand is TypeReference) {
					delegateType = GetUpdatedType (il.Operand as TypeReference, module);

					il = instructions [idx++];
					if (il.OpCode != OpCodes.Call || !(il.Operand is MethodReference opMR2) || opMR2.Name != "GetTypeFromHandle")
						return false;

					skipCount = 10;
				} else
					return false;

				constructor = GetActionConstructor (delegateType, module);
				if (constructor == null)
					return false;

				il = instructions [idx++];
				if (il.OpCode != OpCodes.Ldloc_0)
					return false;

				il = instructions [idx++];
				if (il.OpCode != OpCodes.Ldstr)
					return false;

				var methodName = il.Operand as string;
				if (string.IsNullOrEmpty (methodName))
					return false;

				il = instructions [idx++];
				if (il.OpCode != OpCodes.Call || !(il.Operand is MethodReference opMR3) || opMR3.Name != "CreateDelegate")
					return false;

				il = instructions [idx++];
				if (il.OpCode != OpCodes.Newobj)
					return false;

				il = instructions [idx++];
				if (il.OpCode != OpCodes.Stelem_Any)
					return false;

				idx = idxStart;
				for (int i = 0; i < 4; i++)
					newInstructions.Add (GetUpdatedInstruction (instructions [idx++], module));

				newInstructions.Add (Instruction.Create (OpCodes.Ldnull));
				newInstructions.Add (Instruction.Create (OpCodes.Ldftn, newHelperMethods?[methodName]));
				newInstructions.Add (Instruction.Create (OpCodes.Newobj, constructor));

				idx += customDelegate ? 6 : 5;
				for (int i = 0; i < 2; i++)
					newInstructions.Add (GetUpdatedInstruction (instructions [idx++], module));

				return true;
			}

			return false;
		}

		MethodDefinition Duplicate (MethodDefinition src, ModuleDefinition module, TypeDefinition type)
		{
			var md = new MethodDefinition (src.Name, src.Attributes, GetUpdatedType (src.ReturnType, module));

			if (src.HasCustomAttributes)
				foreach (var ca in src.CustomAttributes)
					md.CustomAttributes.Add (new CustomAttribute (GetUpdatedMethod (ca.Constructor, module), ca.GetBlob ()));

			foreach (var p in src.Parameters)
				md.Parameters.Add (new ParameterDefinition (p.Name, p.Attributes, GetUpdatedType (p.ParameterType, module)));

			md.Body.InitLocals = src.Body.InitLocals;

			var instructionMap = new Dictionary<Instruction, Instruction> ();
			var instructions = src.Body.Instructions;
			var newInstructions = md.Body.Instructions;
			var count = instructions.Count;
			var typeName = type.FullName.Replace ('/', '+');
			var isRegisterMethod = src.Name == "__RegisterNativeMembers";
			int skipCount;
			int improvements = 0;
			bool failed = false;

			for (int i = 0; i < count; i++) {
				var il = instructions [i];
				bool result = false;

				if (isRegisterMethod && (result = AnalyzeAndImprove (instructions, newInstructions, i, typeName, out skipCount, module))) {
					i += skipCount;
					improvements++;
				} else {
					failed |= !result;
					Instruction newInstruction = GetUpdatedInstruction (il, module);
					newInstructions.Add (newInstruction);
					instructionMap [il] = newInstruction;
				}
			}

			if (src.Body.HasVariables)
				foreach (var v in src.Body.Variables)
					if (!isRegisterMethod || failed || v.VariableType.FullName != "System.Type")
						md.Body.Variables.Add (new VariableDefinition (GetUpdatedType (v.VariableType, module)));


			if (isRegisterMethod && improvements < 2)
				App.Information ($"Method {md} was not improved. There should have been at least 2 performance improvements in this registration method.");

			if (src.Body.HasExceptionHandlers)
				foreach (var eh in src.Body.ExceptionHandlers)
					md.Body.ExceptionHandlers.Add (GetUpdatedExceptionHandler (eh, instructionMap, module));

			md.Body.MaxStackSize = src.Body.MaxStackSize;

			return md;
		}

		TypeDefinition GetType (string assemblyName, string typeName)
		{
			var assembly = Resolver.Resolve (assemblyName);
			if (assembly == null)
				return null;

			return assembly.MainModule.GetType (typeName);
		}

		MethodReference GetSingleParameterMethod (ModuleDefinition module, string assemblyName, string typeName, string methodName, string parameterTypeName)
		{
			var typeTD = GetType (assemblyName, typeName);
			foreach (var md in typeTD.Methods)
				if (md.Name == methodName && md.HasParameters && md.Parameters.Count == 1 && md.Parameters [0].ParameterType.FullName == parameterTypeName)
					return GetUpdatedMethod (md, module);

			return null;
		}
	}
}
