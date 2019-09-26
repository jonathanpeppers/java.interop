using System;
using System.Reflection;
using System.Reflection.Metadata;

namespace Java.Interop.Tools.JavaCallableWrappers
{
	public static class SystemReflectionMetadataExtensions
	{
		public static bool IsAbstract (this TypeDefinition type) =>
			(type.Attributes & TypeAttributes.Abstract) == TypeAttributes.Abstract;

		public static bool IsInterface (this TypeDefinition type) =>
			(type.Attributes & TypeAttributes.Interface) == TypeAttributes.Interface;

		public static bool HasGenericParameters (this TypeDefinition type) =>
			type.GetGenericParameters ().Count > 0;

		public static string FullName (this TypeDefinition type, MetadataReader reader)
		{
			var name = reader.GetString (type.Name);
			var ns = reader.GetString (type.Namespace);
			return $"{ns}.{name}";
		}

		public static string GetPartialAssemblyQualifiedName (this TypeDefinition type, MetadataReader reader)
		{
			var fullName = type.FullName (reader);
			var assembly = "TODO"; //TODO: what do here?
			return $"{fullName}, {assembly}";
		}

		public static bool IsAssignableFrom (this TypeDefinition type, TypeDefinition c, MetadataReader reader)
		{
			var fullName = type.FullName (reader);
			if (fullName == c.FullName (reader)) {
				return true;
			}

			//TODO: this isn't right, need to look at base types
			return false;
		}
	}
}
