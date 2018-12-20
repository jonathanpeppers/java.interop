using System;
using System.Diagnostics;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Utils;
using System.Text;

using Java.Interop.Tools.TypeNameMappings;

namespace Java.Interop.Tools.JavaCallableWrappers {

	/*
	 * TypeNameMapGenerator is responsible for generating a "mapping file" to
	 * support CPU-efficient mapping between Java types and Managed types
	 * (wherein "CPU-efficient" means "NO REFLECTION REQUIRED").
	 *
	 * This is done by storing the data in a form that can be easily passed
	 * to bsearch(3), allowing a binary search of the contents.
	 *
	 * The format of the file is:
	 *
	 *      // header
	 *      "version="      INT \0 (must be '1')
	 *      "entry-count="  INT \0 (bsearch(3) `nel` value)
	 *      "entry-len="    INT \0 (bsearch(3) `width` value)
	 *      "value-offset=" INT \0 (offset to value within entry)
	 *
	 *      ENTRIES
	 *      \0
	 *
	 * The header consists of NUL-separated key=value string pairs, including
	 * the file format version ("1"). (Why strings? So I don't have to worry
	 * about byte-ordering concerns. Besides, the ENTRIES will be strings,
	 * and it makes viewing the file contents with strings(1) easy.)
	 *
	 * ENTRIES consists of rows that are `entry-len` bytes in length, containing
	 * a "key" and a "value". The "key" is NUL-padded so that the "value" starts
	 * at `value-offset` bytes into the row, and "value" is NUL-padded to ensure
	 * that the entire row length is `entry-len` bytes in length.
	 *
	 * After ENTRIES is a final terminating NUL.
	 *
	 * In string-form, a valid mapping would be:
	 *
	 *      "version=1\u0000" +
	 *      "entry-count=1\u0000" +
	 *      "entry-len=10\u0000" +
	 *      "value-offset=4\u0000" +
	 *      "key\u0000value\u0000" +
	 *      "\u0000"
	 *
	 * The rows MUST be sorted so that strcmp(3) can be used to compare keys
	 * values between rows
	 */
	public class TypeNameMapGenerator {

		Action<TraceLevel, string>      Log;
		List<TypeDefinitionAndAssembly> Types;
		JavaTypeScanner                 Scanner;

		[Obsolete ("Use TypeNameMapGenerator(IEnumerable<string>, Action<TraceLevel, string>)")]
		public TypeNameMapGenerator (IEnumerable<string> assemblies, Action<string, object []> logMessage)
			: this (assemblies, (TraceLevel level, string value) => logMessage?.Invoke ("{0}", new[]{value}))
		{
			if (logMessage == null)
				throw new ArgumentNullException (nameof (logMessage));
		}

		public TypeNameMapGenerator (IEnumerable<string> assemblies, Action<TraceLevel, string> logger)
		{
			if (assemblies == null)
				throw new ArgumentNullException ("assemblies");
			if (logger == null)
				throw new ArgumentNullException (nameof (logger));

			Log             = logger;
			var Assemblies  = assemblies.ToList ();

			foreach (var assembly in Assemblies) {
				var directory   = Path.GetDirectoryName (assembly);
				if (string.IsNullOrEmpty (directory))
					continue;
			}

			Scanner     = new JavaTypeScanner (Log) {
				ErrorOnCustomJavaObject     = false,
			};
			Types       = Scanner.GetJavaTypes (Assemblies);
		}

		[Obsolete ("Use TypeNameMapGenerator(IEnumerable<TypeDefinition>, Action<TraceLevel, string>)")]
		public TypeNameMapGenerator (IEnumerable<TypeDefinitionAndAssembly> types, Action<string, object[]> logMessage)
			: this (types, (TraceLevel level, string value) => logMessage?.Invoke ("{0}", new [] { value }))
		{
			if (logMessage == null)
				throw new ArgumentNullException (nameof (logMessage));
		}

		public TypeNameMapGenerator (IEnumerable<TypeDefinitionAndAssembly> types, Action<TraceLevel, string> logger)
		{
			if (types == null)
				throw new ArgumentNullException ("types");
			if (logger == null)
				throw new ArgumentNullException (nameof (logger));

			Log         = logger;
			Types       = types.ToList ();
		}

		public void WriteJavaToManaged (Stream output)
		{
			if (output == null)
				throw new ArgumentNullException ("output");

			var typeMap = GetTypeMapping (
					t => t.Type.IsInterface () || t.Type.HasGenericParameters (),
					t => JavaNativeTypeManager.ToJniName (t.Type),
					t => t.Assembly.Name);

			WriteBinaryMapping (output, typeMap);
		}

		Dictionary<string, string> GetTypeMapping (Func<TypeDefinitionAndAssembly, bool> skipType, Func<TypeDefinitionAndAssembly, string> key, Func<TypeDefinitionAndAssembly, string> value)
		{
			var typeMap     = new Dictionary<string, TypeDefinitionAndAssembly> ();
			var aliases     = new Dictionary<string, List<string>> ();
			foreach (var type in Types) {
				if (skipType (type))
					continue;

				var k = key (type);

				TypeDefinitionAndAssembly existing;
				if (!typeMap.TryGetValue (k, out existing)) {
					typeMap.Add (k, type);
					continue;
				}

				var typeDef = type.Type;
				var existingDef = existing.Type;
				if (typeDef.IsAbstract () || typeDef.IsInterface () || existingDef.IsAbstract () || existingDef.IsInterface ()) {
					// Two separate types w/ the same key; a JavaToManaged issue.
					// Prefer the base (abstract?) class over the invoker.
					var b = existing;
					if (typeDef.IsAssignableFrom (existing))
						b = type;
					typeMap [k] = b;
				} else {
					List<string> a;
					if (!aliases.TryGetValue (k, out a)) {
						aliases.Add (k, a = new List<string> ());
						a.Add (value (existing));
					}
					a.Add (value (type));
				}
			}
			foreach (var e in aliases.OrderBy (e => e.Key)) {
				Log (TraceLevel.Warning, $"Mapping for type '{e.Key}' is ambiguous between {e.Value.Count} types.");
				Log (TraceLevel.Warning, $"     Using: {e.Value.First ()}");
				foreach (var o in e.Value.Skip (1)) {
					Log (TraceLevel.Info, $"  Ignoring: {o}");
				}
			}
			return typeMap.ToDictionary (e => e.Key, e => value (e.Value));
		}

		static void WriteBinaryMapping (Stream o, Dictionary<string, string> mapping)
		{
			var encoding    = Encoding.UTF8;
			var binary      = ToBinary (mapping, encoding);

			var keyLen      = binary.Keys.Max (v => v?.Length) ?? 0;
			var valueLen    = binary.Values.Max (v => v?.Length) ?? 0;

			WriteHeader (o, binary.Count, keyLen, valueLen, encoding);

			foreach (var key in binary.Keys.OrderBy (k => k, new ArrayComparer<byte> ())) {
				Write (o, key, keyLen);
				Write (o, binary [key], valueLen);
			}
			o.WriteByte (0x0);
		}

		static Dictionary<byte[], byte[]> ToBinary(Dictionary<string, string> map, Encoding encoding)
		{
			return map.ToDictionary (e => encoding.GetBytes (e.Key), e => encoding.GetBytes (e.Value));
		}

		static void WriteHeader (Stream o, int entries, int keyLen, int valueLen, Encoding encoding)
		{
			var header  = string.Format ("version=1\u0000entry-count={0}\u0000entry-len={1}\u0000value-offset={2}\u0000", entries, checked (keyLen + 1 + valueLen + 1), keyLen + 1);
			WriteString (o, header, encoding);
		}

		static void WriteString (Stream o, string value, Encoding encoding)
		{
			var data = encoding.GetBytes (value);
			o.Write (data, 0, data.Length);
		}

		static void Write (Stream o, byte[] value, int length)
		{
			o.Write (value, 0, value.Length);
			for (int i = value.Length; i < length; ++i)
				o.WriteByte (0x0);
			o.WriteByte (0x0);
		}

		public void WriteManagedToJava (Stream output)
		{
			if (output == null)
				throw new ArgumentNullException ("output");

			var typeMap = GetTypeMapping (
					t => false,
					t => t.Assembly.Name,
					JavaNativeTypeManager.ToJniName);

			WriteBinaryMapping (output, typeMap);
		}
	}

	class ArrayComparer<T> : IComparer<T[]> {

		public int Compare (T[] x, T[] y)
		{
			if (x == null && y == null)
				return 0;
			if (x == null)
				return -1;
			if (y == null)
				return 1;
			int len = Math.Min (x.Length, y.Length);
			for (int i = 0; i < len; ++i) {
				var c = Comparer<T>.Default.Compare (x [i], y [i]);
				if (c != 0)
					return c;
			}
			if (x.Length == y.Length)
				return 0;
			if (x.Length > y.Length)
				return 1;
			return -1;
		}
	}
}
