using System.Linq;
using System.IO;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Utils;
using System.Reflection.PortableExecutable;
using NUnit.Framework;
using System.Reflection.Metadata.Ecma335;
using System.Collections.Immutable;

namespace System.Reflection.Metadata.UtilsTests
{
	[TestFixture]
	public class TypeDefinitionExtensionsTests
	{
		const string Namespace = "Test";
		PEReader peReader;
		MetadataReader reader;

		[SetUp]
		public void SetUp ()
		{
			var path = GetType ().Assembly.Location;
			peReader = new PEReader (File.OpenRead (path));
			reader = peReader.GetMetadataReader ();
		}

		[TearDown]
		public void TearDown ()
		{
			peReader.Dispose ();
		}

		TypeDefinition FindType (string name, string @namespace = Namespace)
		{
			foreach (var t in reader.TypeDefinitions) {
				var type = reader.GetTypeDefinition (t);
				if (reader.AreEqual (type, @namespace, name))
					return type;
			}
			throw new Exception ($"Could not find `{@namespace}.{name}`!");
		}

		[Test]
		public void Class ()
		{
			var name = "Foo";
			var foo = FindType (name);
			Assert.IsTrue (foo.IsClass (), $"{name} is a class!");
			Assert.IsFalse (foo.IsInterface (), $"{name} is *not* an interface!");
			Assert.AreEqual ($"{Namespace}.{name}", foo.FullName (reader));
		}

		[Test]
		public void Interface ()
		{
			var name = "IFoo";
			var ifoo = FindType (name);
			Assert.IsTrue (ifoo.IsInterface (), $"{name} is an interface!");
			Assert.IsFalse (ifoo.IsClass (), $"{name} is *not* a class!");
			Assert.AreEqual ($"{Namespace}.{name}", ifoo.FullName (reader));
		}

		[Test]
		public void Struct ()
		{
			var name = "MyStruct";
			var @struct = FindType (name);
			Assert.IsTrue (@struct.IsValueType (reader), $"{name} is a value type!");
		}

		[Test]
		public void Enum ()
		{
			var name = "MyEnum";
			var @enum = FindType (name);
			Assert.IsTrue (@enum.IsEnum (reader), $"{name} is an enum!");
			Assert.IsTrue (@enum.IsValueType (reader), $"{name} is a value type!");
		}

		[Test]
		public void GetBaseTypes ()
		{
			var name = "Foo";
			var @base = "Bar";
			var foo = FindType (name);
			var bar = foo.GetBaseTypes (reader).Single ();
			Assert.IsTrue (reader.AreEqual (bar, Namespace, @base), $"Only BaseType should be {@base}!");
		}

		[Test]
		public void GetTypeAndBaseTypes ()
		{
			var name = "Foo";
			var foo = FindType (name);
			var baseTypes = foo.GetTypeAndBaseTypes (reader);
			var actual = baseTypes.First ();
			Assert.IsTrue (reader.AreEqual (actual, Namespace, name), $"First BaseType should be {name}!");

			name = "Bar";
			var bar = baseTypes.Skip (1).Single ();
			Assert.IsTrue (reader.AreEqual (bar, Namespace, name), $"Second BaseType should be {name}!");
		}

		[Test]
		public void ImplementsInterface ()
		{
			var iface = "IBar";
			var name = "Bar";
			var type = FindType (name);
			Assert.IsTrue (type.ImplementsInterface (reader, Namespace, iface), $"{name} implements {iface}!");

			name = "Foo";
			type = FindType (name);
			Assert.IsTrue (type.ImplementsInterface (reader, Namespace, iface), $"{name} implements {iface}!");

			name = "Disposable";
			iface = "IDisposable";
			type = FindType (name);
			Assert.IsTrue (type.ImplementsInterface (reader, "System", iface), $"{name} implements {iface}!");
		}

		[Test]
		public void Generic ()
		{
			//TODO: unsure if this properly matches Mono.Cecil
			var name = "Generic`1";
			var generic = FindType (name);
			Assert.IsTrue (generic.IsGeneric (), $"{name} is generic!");
			Assert.IsTrue (generic.IsClass (), $"{name} is a class!");
			Assert.IsFalse (generic.IsInterface (), $"{name} is *not* an interface!");
			Assert.AreEqual ($"{Namespace}.{name}", generic.FullName (reader));
		}
	}
}
