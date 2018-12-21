using System.Collections.Generic;

namespace System.Reflection.Metadata.Utils
{
	public static class TypeDefinitionExtensions
	{
		public static bool AreEqual (this MetadataReader reader, TypeDefinition type, string @namespace, string typeName)
		{
			return typeName == reader.GetString (type.Name) && @namespace == reader.GetString (type.Namespace);
		}

		public static bool AreEqual (this MetadataReader reader, TypeReference type, string @namespace, string typeName)
		{
			return typeName == reader.GetString (type.Name) && @namespace == reader.GetString (type.Namespace);
		}

		public static bool AreEqual (this MetadataReader reader, TypeDefinition type, string @namespace, params string [] typeNames)
		{
			if (@namespace != reader.GetString (type.Namespace))
				return false;

			var actual = reader.GetString (type.Name);
			foreach (var typeName in typeNames) {
				if (actual == typeName)
					return true;
			}
			return false;
		}

		public static bool AreEqual (this MetadataReader reader, TypeReference type, string @namespace, params string [] typeNames)
		{
			if (@namespace != reader.GetString (type.Namespace))
				return false;

			var actual = reader.GetString (type.Name);
			foreach (var typeName in typeNames) {
				if (actual == typeName)
					return true;
			}
			return false;
		}

		public static bool IsClass (this TypeDefinition type)
		{
			return (type.Attributes & TypeAttributes.ClassSemanticsMask) == TypeAttributes.Class;
		}

		public static bool IsEnum (this TypeDefinition type, MetadataReader reader)
		{
			if (type.BaseType.Kind == HandleKind.TypeReference) {
				var typeReference = reader.GetTypeReference ((TypeReferenceHandle)type.BaseType);
				return reader.AreEqual (typeReference, "System", "Enum");
			}
			return false;
		}

		public static bool IsInterface (this TypeDefinition type)
		{
			return (type.Attributes & TypeAttributes.ClassSemanticsMask) == TypeAttributes.Interface;
		}

		public static bool IsValueType (this TypeDefinition type, MetadataReader reader)
		{
			if (type.BaseType.Kind == HandleKind.TypeReference) {
				var typeReference = reader.GetTypeReference ((TypeReferenceHandle)type.BaseType);
				return reader.AreEqual (typeReference, "System", "ValueType", "Enum");
			}
			return false;
		}

		public static bool IsArray (this TypeDefinition type)
		{
			//TODO: implement
			throw new NotImplementedException ();
		}

		public static bool IsAbstract (this TypeDefinition type)
		{
			return (type.Attributes & TypeAttributes.Abstract) != 0;
		}

		public static bool IsGeneric (this TypeDefinition type)
		{
			var generics = type.GetGenericParameters ();
			return generics.Count > 0;
		}

		public static string FullName (this TypeDefinition type, MetadataReader reader)
		{
			var ns = reader.GetString (type.Namespace);
			var name = reader.GetString (type.Name);
			return $"{ns}.{name}";
		}

		public static IEnumerable<TypeDefinition> GetTypeAndBaseTypes (this TypeDefinition type, MetadataReader reader)
		{
			yield return type;

			foreach (var baseType in type.GetBaseTypes (reader)) {
				yield return baseType;
			}
		}

		public static IEnumerable<TypeDefinition> GetBaseTypes (this TypeDefinition type, MetadataReader reader)
		{
			EntityHandle handle;
			while (!(handle = type.BaseType).IsNil) {
				if (handle.Kind == HandleKind.TypeDefinition) {
					type = reader.GetTypeDefinition ((TypeDefinitionHandle)handle);
					yield return type;
				} else {
					break;
				}
			}
		}

		public static bool IsSubclassOf (this TypeDefinition type, MetadataReader reader, string @namespace, params string [] typeNames)
		{
			foreach (var baseType in type.GetTypeAndBaseTypes (reader)) {
				if (reader.AreEqual (baseType, @namespace, typeNames))
					return true;
			}
			return false;
		}

		public static bool ImplementsInterface (this TypeDefinition type, MetadataReader reader, string @namespace, string interfaceName)
		{
			foreach (var t in type.GetTypeAndBaseTypes (reader)) {
				foreach (var i in t.GetInterfaceImplementations ()) {
					var iface = reader.GetInterfaceImplementation (i);
					if (iface.Interface.Kind == HandleKind.TypeDefinition) {
						var interfaceType = reader.GetTypeDefinition ((TypeDefinitionHandle)iface.Interface);
						if (reader.AreEqual (interfaceType, @namespace, interfaceName))
							return true;
					} else if (iface.Interface.Kind == HandleKind.TypeReference) {
						var interfaceType = reader.GetTypeReference ((TypeReferenceHandle)iface.Interface);
						if (reader.AreEqual (interfaceType, @namespace, interfaceName))
							return true;
					}
				}
			}
			return false;
		}

		public static IEnumerable<MethodDefinition> GetInstanceConstructors (this TypeDefinition type, MetadataReader reader)
		{
			foreach (var h in type.GetMethods ()) {
				var method = reader.GetMethodDefinition (h);
				if (method.IsCtor () && !method.IsStatic ())
					yield return method;
			}
		}

		public static string GetPartialAssemblyQualifiedName (this TypeDefinition type, MetadataReader reader, AssemblyDefinition assembly)
		{
			return $"{reader.GetString (assembly.Name)}, {type.FullName (reader)}";
		}

		public static string GetAssemblyQualifiedName (this TypeDefinition type, MetadataReader reader, AssemblyDefinition assembly)
		{
			var assemblyName = assembly.GetAssemblyName ();
			return $"{assemblyName.FullName}, {type.FullName (reader)}";
		}
	}
}
