using MonoDroid.Generation;
using NUnit.Framework;
using System.IO;
using System.Xml.Linq;

namespace generatortests
{
	[TestFixture]
	public class XmlTests
	{
		XDocument xml;
		XElement package;

		[SetUp]
		public void SetUp()
		{
			using (var reader = new StringReader (@"<api>
	<package name=""com.mypackage"">
		<class abstract=""false"" deprecated=""not deprecated"" final=""false"" name=""foo"" static=""false"" visibility=""public"">
			<method abstract=""false"" deprecated=""not deprecated"" final=""false"" name=""bar"" native=""false"" return=""void"" static=""false"" synchronized=""false"" visibility=""public"" managedReturn=""System.Void"" />
		</class>
	</package>
</api>")) {
				xml = XDocument.Load (reader);
			}

			package = xml.Element ("api").Element ("package");
		}

		[Test]
		public void Class ()
		{
			var element = package.Element ("class");
			var @class = new XmlClassGen (package, element);
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
		public void Method()
		{
			var element = package.Element ("class");
			var @class = new XmlClassGen (package, element);
			var method = new XmlMethod (@class, element.Element ("method"));
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
