﻿using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

using Xamarin.Android.Tools.Bytecode;

using NUnit.Framework;

namespace Xamarin.Android.Tools.BytecodeTests {

	public class ClassFileFixture {

		protected static ClassFile LoadClassFile (string resource)
		{
			using (var s = Assembly.GetExecutingAssembly ().GetManifestResourceStream (resource)) {
				return new ClassFile (s);
			}
		}

		protected static string LoadString (string resource)
		{
			var builder = new StringBuilder();
			using (var s = Assembly.GetExecutingAssembly ().GetManifestResourceStream (resource))
			using (var r = new StreamReader (s)) {
				while (!r.EndOfStream) {
					if (builder.Length > 0)
						builder.Append ('\n');
					builder.Append (r.ReadLine ());
				}
			}
			return builder.ToString ();
		}

		protected static void AssertXmlDeclaration (string classResource, string xmlResource, string documentationPath = null, JavaDocletType? javaDocletType = null)
		{
			var classPathBuilder    = new ClassPath () {
				ApiSource           = "class-parse",
				DocumentationPaths  = new string[] {
					documentationPath,
				},
			};
			if (javaDocletType.HasValue)
				classPathBuilder.DocletType = javaDocletType.Value;
			classPathBuilder.Add (LoadClassFile (classResource));

			var actual  = new StringWriter ();
			classPathBuilder.ApiSource  = "class-parse";
			if (javaDocletType.HasValue)
				classPathBuilder.DocletType = javaDocletType.Value;
			classPathBuilder.SaveXmlDescription (actual);

			var expected    = LoadString (xmlResource);

			Assert.AreEqual (expected, actual.ToString ());
		}

		protected static void AssertXmlDeclaration (string[] classResources, string xmlResource, string documentationPath = null)
		{
			var classPathBuilder    = new ClassPath () {
				ApiSource           = "class-parse",
				DocumentationPaths  = new string[] {
					documentationPath,
				},
				AutoRename = true
			};
			foreach(var classFile in classResources.Select(s => LoadClassFile (s)))
				classPathBuilder.Add (classFile);

			var actual  = new StringWriter ();
			classPathBuilder.SaveXmlDescription (actual);

			var expected    = LoadString (xmlResource);

			Assert.AreEqual (expected, actual.ToString ());
		}
	}
}

