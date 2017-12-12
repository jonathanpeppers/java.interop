using System;
using System.Reflection;
using System.CodeDom.Compiler;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using NUnit.Framework;

namespace generatortests
{
	public static class Compiler
	{
		const string RoslynEnvironmentVariable = "ROSLYN_COMPILER_LOCATION";
		private static string unitTestFrameworkAssemblyPath = typeof(Assert).Assembly.Location;
		private static string supportFilePath = typeof(Compiler).Assembly.Location;

		static CodeDomProvider GetCodeDomProvider ()
		{
			if (Environment.OSVersion.Platform == PlatformID.Win32NT) {
				//NOTE: there is an issue where Roslyn's csc.exe isn't copied to output for non-ASP.NET projects
				// Comments on this here: https://stackoverflow.com/a/40311406/132442
				// They added an environment variable as a workaround: https://github.com/aspnet/RoslynCodeDomProvider/pull/12
				if (string.IsNullOrEmpty (Environment.GetEnvironmentVariable (RoslynEnvironmentVariable, EnvironmentVariableTarget.Process))) {
					string roslynPath = Path.GetFullPath (Path.Combine (unitTestFrameworkAssemblyPath, "..", "..", "..", "packages", "Microsoft.Net.Compilers.2.1.0", "tools"));
					Environment.SetEnvironmentVariable (RoslynEnvironmentVariable, roslynPath, EnvironmentVariableTarget.Process);
				}

				return new Microsoft.CodeDom.Providers.DotNetCompilerPlatform.CSharpCodeProvider ();
			} else {
				return new Microsoft.CSharp.CSharpCodeProvider ();
			}
		}

		public static string CompileSupportAssembly (Xamarin.Android.Binder.CodeGeneratorOptions options,
			IEnumerable<string> additionalSupportDirectories,
			out bool hasErrors, out string output)
		{
			var generatedCodePath = options.ManagedCallableWrapperSourceOutputDirectory;
			var sourceFiles = Directory.EnumerateFiles (generatedCodePath, "Java.Lang.*.cs", SearchOption.AllDirectories)
				.Select (x => Path.GetFullPath (x))
				.ToList ();

			var supportFiles = Directory.EnumerateFiles (Path.Combine (Path.GetDirectoryName (supportFilePath), "SupportFiles"),
				"*.cs", SearchOption.AllDirectories);
			sourceFiles.AddRange (supportFiles);

			foreach (var dir in additionalSupportDirectories) {
				var additonal = Directory.EnumerateFiles (dir, "*.cs", SearchOption.AllDirectories);
				sourceFiles.AddRange (additonal);
			}

			var path = Path.Combine (Path.GetTempPath (), Path.GetRandomFileName ());
			var parameters = CreateParameters (path, false);
			using (var codeProvider = GetCodeDomProvider ()) {
				var results = codeProvider.CompileAssemblyFromFile (parameters, sourceFiles.ToArray ());

				hasErrors = false;

				//NOTE: due to the tests generating Java.Lang.Object or Java.Lang.String, we will get some warnings
				foreach (CompilerError message in results.Errors) {
					hasErrors |= !message.IsWarning || !message.ErrorText.Contains ("Java.Lang.");
				}
				output = string.Join (Environment.NewLine, results.Output.Cast<string> ());

				return results.PathToAssembly;
			}
		}

		public static Assembly Compile (Xamarin.Android.Binder.CodeGeneratorOptions options,
			IEnumerable<string> additionalSourceDirectories,
			string assemblyFileName, string supportAssemblyPath,
			out bool hasErrors, out string output, bool allowWarnings)
		{
			var generatedCodePath = options.ManagedCallableWrapperSourceOutputDirectory;
			var sourceFiles = Directory.EnumerateFiles (generatedCodePath, "*.cs", SearchOption.AllDirectories)
				.Where (x => !x.Contains ("Java.Lang."))
				.Select (x => Path.GetFullPath(x))
				.ToList ();

			foreach (var dir in additionalSourceDirectories) {
				var additonal = Directory.EnumerateFiles (dir, "*.cs", SearchOption.AllDirectories);
				sourceFiles.AddRange (additonal);
			}

			var parameters = CreateParameters (assemblyFileName, false);
			parameters.ReferencedAssemblies.Add (supportAssemblyPath);

			using (var codeProvider = GetCodeDomProvider ()) {
				CompilerResults results = codeProvider.CompileAssemblyFromFile (parameters, sourceFiles.ToArray ());

				hasErrors = false;

				foreach (CompilerError message in results.Errors) {
					hasErrors |= !message.IsWarning || !allowWarnings;
				}
				output = string.Join (Environment.NewLine, results.Output.Cast<string> ());

				return results.CompiledAssembly;
			}
		}

		static CompilerParameters CreateParameters (string assemblyFileName, bool inMemory)
		{
			var parameters = new CompilerParameters ();
			parameters.GenerateExecutable = false;
			parameters.GenerateInMemory = inMemory;
			parameters.CompilerOptions = "/unsafe";
			parameters.OutputAssembly = assemblyFileName;
			parameters.ReferencedAssemblies.Add (unitTestFrameworkAssemblyPath);
			parameters.ReferencedAssemblies.Add (typeof (Enumerable).Assembly.Location);

			var binDir = Path.GetDirectoryName (typeof (BaseGeneratorTest).Assembly.Location);
			var facDir = GetFacadesPath ();
			parameters.ReferencedAssemblies.Add (Path.Combine (binDir, "Java.Interop.dll"));
			parameters.ReferencedAssemblies.Add (Path.Combine (facDir, "System.Runtime.dll"));
#if DEBUG
			parameters.IncludeDebugInformation = true;
#else
			parameters.IncludeDebugInformation = false;
#endif
			return parameters;
		}

		static string GetFacadesPath ()
		{
			var env = Environment.GetEnvironmentVariable ("FACADES_PATH");
			if (env != null)
				return env;

			var dir = Path.GetDirectoryName (typeof (object).Assembly.Location);
			var facades = Path.Combine (dir, "Facades");
			if (Directory.Exists (facades))
				return facades;

			return dir;
		}
	}
}

