using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using Java.Interop.Tools.TypeNameMappings;

namespace Java.Interop.Tools.JavaCallableWrappers
{
	public class TypeNameMapGeneratorMetadata : BaseTypeNameMapGenerator
	{
		public TypeNameMapGeneratorMetadata (IEnumerable<string> assemblies, Action<TraceLevel, string> logger)
			: base (logger)
		{
			if (assemblies == null)
				throw new ArgumentNullException ("assemblies");

			Types = new List<Type> ();
			foreach (var assembly in assemblies) {
				var pe = new PEReader (File.OpenRead (assembly));
				var reader = pe.GetMetadataReader ();
				foreach (var h in reader.TypeDefinitions) {
					var type = new Type {
						Definition = reader.GetTypeDefinition (h),
						PE = pe,
						Reader = reader,
					};
					Types.Add (type);
				}
			}
		}

		readonly List<Type> Types;

		class Type
		{
			public TypeDefinition Definition { get; set; }

			public PEReader PE { get; set; }

			public MetadataReader Reader { get; set; }
		}

		public override void WriteJavaToManaged (Stream output)
		{
			if (output == null)
				throw new ArgumentNullException ("output");

			var typeMap = GetTypeMapping (
					t => t.Definition.IsInterface () || t.Definition.HasGenericParameters (),
					t => JavaNativeTypeManager.ToJniName (t.Definition),
					t => t.Definition.GetPartialAssemblyQualifiedName (t.Reader));

			WriteBinaryMapping (output, typeMap);
		}

		public override void WriteManagedToJava (Stream output)
		{
			if (output == null)
				throw new ArgumentNullException ("output");

			var typeMap = GetTypeMapping (
					t => false,
					t => t.Definition.GetPartialAssemblyQualifiedName (t.Reader),
					t => JavaNativeTypeManager.ToJniName (t.Definition));

			WriteBinaryMapping (output, typeMap);
		}

		protected override void Dispose (bool disposing)
		{
			throw new NotImplementedException ();
		}

		Dictionary<string, string> GetTypeMapping (Func<Type, bool> skipType, Func<Type, string> key, Func<Type, string> value)
		{
			var typeMap = new Dictionary<string, Type> ();
			var aliases = new Dictionary<string, List<string>> ();
			foreach (var type in Types) {
				if (skipType (type))
					continue;

				var k = key (type);

				Type e;
				if (!typeMap.TryGetValue (k, out e)) {
					typeMap.Add (k, type);
				} else if (type.Definition.IsAbstract () || type.Definition.IsInterface () || e.Definition.IsAbstract () || e.Definition.IsInterface ()) {
					// Two separate types w/ the same key; a JavaToManaged issue.
					// Prefer the base (abstract?) class over the invoker.
					var b = e;
					if (type.Definition.IsAssignableFrom (e.Definition, type.Reader))
						b = type;
					typeMap [k] = b;
				} else {
					List<string> a;
					if (!aliases.TryGetValue (k, out a)) {
						aliases.Add (k, a = new List<string> ());
						a.Add (value (e));
					}
					a.Add (value (type));
				}
			}
			foreach (var e in aliases.OrderBy (e => e.Key)) {
				Log (TraceLevel.Warning, $"Mapping for type '{e.Key}' is ambiguous between {e.Value.Count} types.");
				Log (TraceLevel.Warning, $"     Using: {e.Value.First ()}");
				foreach (var o in e.Value.Skip (1)) {
					Log (TraceLevel.Info, $"  Ignoring: {o}");
				}
			}
			return typeMap.ToDictionary (e => e.Key, e => value (e.Value));
		}
	}
}
