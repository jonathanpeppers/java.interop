using MonoDroid.Generation;
using System.Linq;

namespace generatortests
{
	class TestClass : ClassGen
	{
		string baseType;

		public TestClass (string baseType, string javaName) : base (new TestBaseSupport (javaName))
		{
			this.baseType = baseType;

			SymbolTable.AddType (FullName, new SimpleSymbol (null, Name, Name, Name, Name));
		}

		public override bool IsAbstract => false;

		public override bool IsFinal => false;

		public override string BaseType => baseType;
	}

	class TestBaseSupport : GenBaseSupport
	{
		public TestBaseSupport (string javaName)
		{
			var split = javaName.Split ('.');
			Name = split.Last ();
			FullName = javaName;
			PackageName = javaName.Substring (0, javaName.Length - Name.Length - 1);
		}

		public override bool IsAcw => false;

		public override bool IsDeprecated => false;

		public override string DeprecatedComment => string.Empty;

		public override bool IsGeneratable => true;

		public override bool IsGeneric => false;

		public override bool IsObfuscated => false;

		public override string FullName { get; set; }
		public override string Name { get; set; }

		public override string Namespace => PackageName;

		public override string JavaSimpleName => Name;

		public override string PackageName { get; set; }

		public override string Visibility => "public";

		GenericParameterDefinitionList typeParameters = new GenericParameterDefinitionList ();

		public override GenericParameterDefinitionList TypeParameters => typeParameters;
	}

	class TestField : Field
	{
		TestClass type;

		public TestField (TestClass type, string name)
		{
			this.type = type;
			Name = name;
		}

		public override bool IsDeprecated => false;

		public override string DeprecatedComment => string.Empty;

		public override bool IsFinal => false;

		public override bool IsStatic => false;

		public override string JavaName => Name;

		public override bool IsEnumified => false;

		public override string TypeName => type.FullName;

		public override string Name { get; set; }

		public override string Value => null;

		public override string Visibility => "public";

		Parameter setterParameter = new Parameter ("value", "java.lang.String", "string", false);

		protected override Parameter SetterParameter => setterParameter;
	}
}
