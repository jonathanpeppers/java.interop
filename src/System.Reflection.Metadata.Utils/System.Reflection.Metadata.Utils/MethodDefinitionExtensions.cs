using System;

namespace System.Reflection.Metadata.Utils
{
	public static class MethodDefinitionExtensions
	{
		public static bool IsStatic (this MethodDefinition method)
		{
			return (method.Attributes & MethodAttributes.Static) != 0;
		}

		public static bool IsCtor (this MethodDefinition method)
		{
			//TODO: implement
			throw new NotImplementedException ();
		}
	}
}
