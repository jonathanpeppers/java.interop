using MonoDroid.Generation;
using NUnit.Framework;
using System.IO;
using System.Text;

namespace generatortests
{
	[TestFixture]
	public class XamarinAndroidCodeGeneratorTests
	{
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
