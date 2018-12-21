using System;

namespace System.Reflection.Metadata.Utils
{
	public class SimpleTypeProvider : ISimpleTypeProvider<Type>
	{
		public Type GetPrimitiveType (PrimitiveTypeCode typeCode)
		{
			throw new NotImplementedException ();
		}

		public Type GetTypeFromDefinition (MetadataReader reader, TypeDefinitionHandle handle, byte rawTypeKind)
		{
			var type = reader.GetTypeDefinition (handle);
			return Type.GetType (type.FullName (reader), throwOnError: true);
		}

		public Type GetTypeFromReference (MetadataReader reader, TypeReferenceHandle handle, byte rawTypeKind)
		{
			var type = reader.GetTypeReference (handle);
			return Type.GetType (type.FullName (reader), throwOnError: true);
		}
	}
}
