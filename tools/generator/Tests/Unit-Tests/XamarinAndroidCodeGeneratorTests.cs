using MonoDroid.Generation;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
			options = new CodeGenerationOptions {

			};
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
	}
}
