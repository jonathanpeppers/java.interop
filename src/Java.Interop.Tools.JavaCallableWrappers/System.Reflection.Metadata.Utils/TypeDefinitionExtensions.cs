using System.Collections.Generic;

namespace System.Reflection.Metadata.Utils
{
	public static class TypeDefinitionExtensions
	{
		public static bool IsStatic (this MethodDefinition method)
		{
			return (method.Attributes & MethodAttributes.Static) != 0;
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

		public static bool IsSubclassOf (this TypeDefinition type, MetadataReader reader, string @namespace, string typeName)
		{
			foreach (var baseType in type.GetTypeAndBaseTypes (reader)) {
				var ns = reader.GetString (baseType.Namespace);
				if (ns == @namespace) {
					var name = reader.GetString (baseType.Name);
					if (name == typeName)
						return true;
				}
			}
			return false;
		}
	}
}
