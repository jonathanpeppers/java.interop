using Android.Runtime;
using Mono.Cecil;
using MonoDroid.Generation;
using NUnit.Framework;
using System.IO;
using System.Linq;

namespace Com.Mypackage
{
	[Register ("com/mypackage/foo")]
	public class Foo
	{
		[Register ("bar", "()V", "")]
		public void Bar () { }
	}
}

namespace generatortests
{
	[TestFixture]
	public class ManagedTests
	{
		string tempFile;
		ModuleDefinition module;

		[SetUp]
		public void SetUp ()
		{
			tempFile = Path.GetTempFileName ();
			File.Copy (GetType ().Assembly.Location, tempFile, true);
			module = ModuleDefinition.ReadModule (tempFile);
		}

		[TearDown]
		public void TearDown ()
		{
			module.Dispose ();
			if (File.Exists (tempFile))
				File.Delete (tempFile);
		}

		[Test]
		public void Class ()
		{
			var @class = new ManagedClassGen (module.GetType ("Com.Mypackage.Foo"));
			Assert.AreEqual ("public", @class.Visibility);
			Assert.AreEqual ("Foo", @class.Name);
			Assert.AreEqual ("com.mypackage.foo", @class.JavaName);
			Assert.AreEqual ("Lcom/mypackage/foo;", @class.JniName);
			Assert.IsFalse (@class.IsAbstract);
			Assert.IsFalse (@class.IsFinal);
			Assert.IsFalse (@class.IsDeprecated);
			Assert.IsNull (@class.DeprecatedComment);
		}

		[Test]
		public void Method ()
		{
			var type = module.GetType ("Com.Mypackage.Foo");
			var @class = new ManagedClassGen (type);
			var method = new ManagedMethod (@class, type.Methods.First (m => m.Name == "Bar"));
			method.FillReturnType ();
			Assert.IsTrue (method.Validate (new CodeGenerationOptions (), new GenericParameterDefinitionList ()), "method.Validate failed!");

			Assert.AreEqual ("public", method.Visibility);
			Assert.AreEqual ("void", method.Return);
			Assert.AreEqual ("System.Void", method.ReturnType);
			Assert.AreEqual ("Bar", method.Name);
			Assert.AreEqual ("bar", method.JavaName);
			Assert.AreEqual ("()V", method.JniSignature);
			Assert.IsFalse (method.IsAbstract);
			Assert.IsFalse (method.IsFinal);
			Assert.IsFalse (method.IsStatic);
			Assert.IsNull (method.Deprecated);
		}
	}
}
