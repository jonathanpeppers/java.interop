using MonoDroid.Generation;
using NUnit.Framework;
using System.IO;
using System.Linq;
using System.Text;

namespace generatortests
{
	[TestFixture]
	public class XamarinAndroidCodeGeneratorTests
	{
		class TestClass : ClassGen
		{
			string baseType;

			public TestClass (string baseType, string javaName) : base (new TestBaseSupport (javaName))
			{
				this.baseType = baseType;

				SymbolTable.AddType (Name, new SimpleSymbol (null, Name, Name, Name, Name));
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

			public override string TypeName => type.Name;

			public override string Name { get; set; }

			public override string Value => null;

			public override string Visibility => "public";

			Parameter setterParameter = new Parameter ("value", "java.lang.String", "string", false);

			protected override Parameter SetterParameter => setterParameter;
		}

		XamarinAndroidCodeGenerator generator;
		StringBuilder builder;
		StringWriter writer;
		CodeGenerationOptions options;

		[SetUp]
		public void SetUp ()
		{
			generator = new XamarinAndroidCodeGenerator ();
			builder = new StringBuilder ();
			writer = new StringWriter (builder);
			options = new CodeGenerationOptions ();
		}

		[Test]
		public void WriteClassHandle()
		{
			var @class = new TestClass ("java.lang.Object", "com.mypackage.foo");

			generator.WriteClassHandle (@class, writer, string.Empty, options, false);

			Assert.AreEqual (@"	internal static IntPtr java_class_handle;
	internal static IntPtr class_ref {
		get {
			return JNIEnv.FindClass (""com/mypackage/foo"", ref java_class_handle);
		}
	}

", builder.ToString ());
		}

		[Test]
		public void WriteClassInvokerHandle ()
		{
			var @class = new TestClass ("java.lang.Object", "com.mypackage.foo");

			generator.WriteClassInvokerHandle (@class, writer, string.Empty, options, "Com.MyPackage.Foo");

			Assert.AreEqual (@"protected override global::System.Type ThresholdType {
	get { return typeof (Com.MyPackage.Foo); }
}

", builder.ToString ());
		}

		[Test]
		public void WriteFieldIdField ()
		{
			var @class = new TestClass ("java.lang.Object", "com.mypackage.foo");
			var field = new TestField (@class, "bar");

			generator.WriteFieldIdField (field, writer, string.Empty, options);

			Assert.AreEqual (@"static IntPtr bar_jfieldId;
", builder.ToString ());
		}

		[Test]
		public void WriteFieldGetBody ()
		{
			var @class = new TestClass ("java.lang.Object", "com.mypackage.foo");
			var field = new TestField (@class, "bar");
			field.Validate (options, new GenericParameterDefinitionList ());
			options.ContextTypes.Push (@class);
			generator.WriteFieldGetBody (field, writer, string.Empty, options);
			options.ContextTypes.Pop ();

			Assert.AreEqual (@"if (bar_jfieldId == IntPtr.Zero)
	bar_jfieldId = JNIEnv.GetFieldID (class_ref, ""bar"", ""foo"");
return JNIEnv.GetFooField (((global::Java.Lang.Object) this).Handle, bar_jfieldId);
", builder.ToString ());
		}

		[Test]
		public void WriteFieldSetBody ()
		{
			var @class = new TestClass ("java.lang.Object", "com.mypackage.foo");
			var field = new TestField (@class, "bar");
			field.Validate (options, new GenericParameterDefinitionList ());
			options.ContextTypes.Push (@class);
			generator.WriteFieldSetBody (field, writer, string.Empty, options);
			options.ContextTypes.Pop ();

			Assert.AreEqual (@"if (bar_jfieldId == IntPtr.Zero)
	bar_jfieldId = JNIEnv.GetFieldID (class_ref, ""bar"", ""foo"");
IntPtr native_value = JNIEnv.NewString (value);
try {
	JNIEnv.SetField (((global::Java.Lang.Object) this).Handle, bar_jfieldId, native_value);
} finally {
	JNIEnv.DeleteLocalRef (native_value);
}
", builder.ToString ());
		}
	}
}
