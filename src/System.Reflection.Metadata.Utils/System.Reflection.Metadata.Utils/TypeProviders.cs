using System;

namespace System.Reflection.Metadata.Utils
{
	public class SimpleTypeProvider : ISimpleTypeProvider<Type>
	{
		public Type GetPrimitiveType (PrimitiveTypeCode typeCode)
		{
			switch (typeCode) {
				case PrimitiveTypeCode.Boolean:
					return typeof (bool);
				case PrimitiveTypeCode.Byte:
					return typeof (byte);
				case PrimitiveTypeCode.Char:
					return typeof (char);
				case PrimitiveTypeCode.Double:
					return typeof (double);
				case PrimitiveTypeCode.Int16:
					return typeof (short);
				case PrimitiveTypeCode.Int32:
					return typeof (int);
				case PrimitiveTypeCode.Int64:
					return typeof (long);
				case PrimitiveTypeCode.IntPtr:
					return typeof (IntPtr);
				case PrimitiveTypeCode.Object:
					return typeof (object);
				case PrimitiveTypeCode.SByte:
					return typeof (sbyte);
				case PrimitiveTypeCode.Single:
					return typeof (float);
				case PrimitiveTypeCode.String:
					return typeof (string);
				case PrimitiveTypeCode.TypedReference:
					return typeof (TypedReference);
				case PrimitiveTypeCode.UInt16:
					return typeof (ushort);
				case PrimitiveTypeCode.UInt32:
					return typeof (uint);
				case PrimitiveTypeCode.UInt64:
					return typeof (ulong);
				case PrimitiveTypeCode.UIntPtr:
					return typeof (UIntPtr);
				case PrimitiveTypeCode.Void:
					return typeof (void);
				default:
					throw new ArgumentOutOfRangeException (nameof (typeCode));
			}
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

	public class SimpleArrayProvider : SimpleTypeProvider, ISZArrayTypeProvider<Type>
	{
		public Type GetSZArrayType (Type elementType)
		{
			return elementType.MakeArrayType ();
		}
	}
}
