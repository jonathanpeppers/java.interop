using System;

namespace System.Reflection.Metadata.Utils
{
	public class TypeDefinitionAndAssembly
	{
		TypeDefinition type;

		public TypeDefinitionAndAssembly (AssemblyName assemblyName, TypeDefinition type)
		{
			Assembly = assemblyName;
			this.type = type;
		}

		public AssemblyName Assembly { get; }
		public ref TypeDefinition Type => ref type;
	}
}
