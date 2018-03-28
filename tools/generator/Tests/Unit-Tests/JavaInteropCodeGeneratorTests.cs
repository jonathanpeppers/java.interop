using MonoDroid.Generation;
using NUnit.Framework;
using System.IO;
using System.Text;

namespace generatortests.Unit_Tests
{
	[TestFixture]
	class JavaInteropCodeGeneratorTests
	{
		CodeGenerator generator;
		StringBuilder builder;
		StringWriter writer;
		CodeGenerationOptions options;

		[SetUp]
		public void SetUp ()
		{
			builder = new StringBuilder ();
			writer = new StringWriter (builder);
			options = new CodeGenerationOptions {
				CodeGenerationTarget = Xamarin.Android.Binder.CodeGenerationTarget.JavaInterop1,
			};
			generator = options.CodeGenerator;
		}

		[TearDown]
		public void TearDown ()
		{
			writer.Dispose ();
		}

		[Test]
		public void WriteClassHandle ()
		{
			var @class = new TestClass ("java.lang.Object", "com.mypackage.foo");

			generator.WriteClassHandle (@class, writer, string.Empty, options, false);

			Assert.AreEqual (@"	internal            static  readonly    JniPeerMembers  _members    = new JniPeerMembers (""com/mypackage/foo"", typeof (foo));
	internal static IntPtr class_ref {
		get {
			return _members.JniPeerType.PeerReference.Handle;
		}
	}

", builder.ToString ());
		}

		[Test]
		public void WriteClassInvokerHandle ()
		{
			var @class = new TestClass ("java.lang.Object", "com.mypackage.foo");

			generator.WriteClassInvokerHandle (@class, writer, string.Empty, options, "Com.MyPackage.Foo");

			Assert.AreEqual (@"internal    new     static  readonly    JniPeerMembers  _members    = new JniPeerMembers (""com/mypackage/foo"", typeof (Com.MyPackage.Foo));

public override global::Java.Interop.JniPeerMembers JniPeerMembers {
	get { return _members; }
}

protected override global::System.Type ThresholdType {
	get { return _members.ManagedPeerType; }
}

", builder.ToString ());
		}

		[Test]
		public void WriteFieldIdField ()
		{
			var @class = new TestClass ("java.lang.Object", "com.mypackage.foo");
			var field = new TestField (@class, "bar");

			generator.WriteFieldIdField (field, writer, string.Empty, options);

			//NOTE: not needed for JavaInteropCodeGenerator
			Assert.AreEqual ("", builder.ToString ());
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

			Assert.AreEqual (@"const string __id = ""bar.foo"";

var __v = _members.InstanceFields.GetFooValue (__id, this);
return __v;
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

			Assert.AreEqual (@"const string __id = ""bar.foo"";

IntPtr native_value = JNIEnv.NewString (value);
try {
	_members.InstanceFields.SetValue (__id, this, native_value);
} finally {
	JNIEnv.DeleteLocalRef (native_value);
}
", builder.ToString ());
		}

		[Test]
		public void Field_Generate ()
		{
			var @class = new TestClass ("java.lang.Object", "com.mypackage.foo");
			var field = new TestField (@class, "bar");
			field.Validate (options, new GenericParameterDefinitionList ());
			options.ContextTypes.Push (@class);
			field.Generate (writer, string.Empty, options, @class);
			options.ContextTypes.Pop ();

			Assert.AreEqual (@"
// Metadata.xml XPath field reference: path=""/api/package[@name='com.mypackage']/class[@name='foo']/field[@name='bar']""
[Register (""bar"")]
public foo bar {
	get {
		const string __id = ""bar.foo"";

		var __v = _members.InstanceFields.GetFooValue (__id, this);
		return __v;
	}
	set {
		const string __id = ""bar.foo"";

		IntPtr native_value = JNIEnv.NewString (value);
		try {
			_members.InstanceFields.SetValue (__id, this, native_value);
		} finally {
			JNIEnv.DeleteLocalRef (native_value);
		}
	}
}
", builder.ToString ());
		}
	}
}
