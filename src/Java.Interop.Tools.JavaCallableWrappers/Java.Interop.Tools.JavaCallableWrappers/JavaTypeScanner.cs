using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Utils;
using System.Reflection.PortableExecutable;

using Java.Interop.Tools.TypeNameMappings;

namespace Java.Interop.Tools.JavaCallableWrappers
{
	public class JavaTypeScanner
	{
		public  Action<TraceLevel, string>      Logger                      { get; private set; }
		public  bool                            ErrorOnCustomJavaObject     { get; set; }

		public JavaTypeScanner (Action<TraceLevel, string> logger)
		{
			if (logger == null)
				throw new ArgumentNullException (nameof (logger));
			Logger      = logger;
		}

		public List<TypeDefinitionAndAssembly> GetJavaTypes (IEnumerable<string> assemblies)
		{
			var javaTypes = new List<TypeDefinitionAndAssembly> ();

			foreach (var assembly in assemblies) {
				using (var stream = File.OpenRead (assembly))
				using (var peReader = new PEReader (stream)) {
					var reader = peReader.GetMetadataReader ();
					var assemblyDef = reader.GetAssemblyDefinition ();
					var assemblyName = assemblyDef.GetAssemblyName ();
					foreach (var type in reader.TypeDefinitions) {
						AddJavaTypes (reader, assemblyName, javaTypes, reader.GetTypeDefinition (type));
					}
				}
			}

			return javaTypes;
		}

		void AddJavaTypes (MetadataReader reader, AssemblyName assembly, List<TypeDefinitionAndAssembly> javaTypes, TypeDefinition type)
		{
			if (type.IsSubclassOf (reader, "Java.Lang", "Object", "Throwable")) {

				// For subclasses of e.g. Android.App.Activity.
				javaTypes.Add (new TypeDefinitionAndAssembly(assembly, type));
			} else if (type.IsClass () && !type.IsSubclassOf (reader, "System", "Exception") && type.ImplementsInterface (reader, "Android.Runtime", "IJavaObject")) {
				var level   = ErrorOnCustomJavaObject ? TraceLevel.Error : TraceLevel.Warning;
				var prefix  = ErrorOnCustomJavaObject ? "error" : "warning";
				Logger (
						level,
						$"{prefix} XA4212: Type `{type.FullName (reader)}` implements `Android.Runtime.IJavaObject` but does not inherit `Java.Lang.Object` or `Java.Lang.Throwable`. This is not supported.");
				return;
			}

			var nestedTypes = type.GetNestedTypes ();
			foreach (var nested in nestedTypes)
				AddJavaTypes (reader, assembly, javaTypes, reader.GetTypeDefinition (nested));
		}

		public static bool ShouldSkipJavaCallableWrapperGeneration (TypeDefinition type)
		{
			if (JavaNativeTypeManager.IsNonStaticInnerClass (type))
				return true;

			
			foreach (var customAttribute in type.GetCustomAttributes ()) {

				//TODO: r?

				if (JavaCallableWrapperGenerator.ToRegisterAttribute (r).DoNotGenerateAcw) {
					return true;
				}
			}

			return false;
		}
		// Returns all types for which we need to generate Java delegate types.
		public static List<TypeDefinitionAndAssembly> GetJavaTypes (IEnumerable<string> assemblies, Action<string, object []> log)
		{
			Action<TraceLevel, string> l = (level, value) => log ("{0}", new string [] { value });
			return new JavaTypeScanner (l).GetJavaTypes (assemblies);
		}
	}
}
