using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace Java.Interop.Tools.JavaCallableWrappers
{
	public abstract class BaseTypeNameMapGenerator : IDisposable
	{
		protected Action<TraceLevel, string> Log;

		public BaseTypeNameMapGenerator (Action<TraceLevel, string> logger)
		{
			if (logger == null)
				throw new ArgumentNullException (nameof (logger));

			Log = logger;
		}

		protected static void WriteBinaryMapping (Stream o, Dictionary<string, string> mapping)
		{
			var encoding = Encoding.UTF8;
			var binary = ToBinary (mapping, encoding);

			var keyLen = binary.Keys.Max (v => v?.Length) ?? 0;
			var valueLen = binary.Values.Max (v => v?.Length) ?? 0;

			WriteHeader (o, binary.Count, keyLen, valueLen, encoding);

			foreach (var key in binary.Keys.OrderBy (k => k, new ArrayComparer<byte> ())) {
				Write (o, key, keyLen);
				Write (o, binary [key], valueLen);
			}
			o.WriteByte (0x0);
		}

		protected static Dictionary<byte [], byte []> ToBinary (Dictionary<string, string> map, Encoding encoding)
		{
			return map.ToDictionary (e => encoding.GetBytes (e.Key), e => encoding.GetBytes (e.Value));
		}

		protected static void WriteHeader (Stream o, int entries, int keyLen, int valueLen, Encoding encoding)
		{
			var header = string.Format ("version=1\u0000entry-count={0}\u0000entry-len={1}\u0000value-offset={2}\u0000", entries, checked(keyLen + 1 + valueLen + 1), keyLen + 1);
			WriteString (o, header, encoding);
		}

		protected static void WriteString (Stream o, string value, Encoding encoding)
		{
			var data = encoding.GetBytes (value);
			o.Write (data, 0, data.Length);
		}

		protected static void Write (Stream o, byte [] value, int length)
		{
			o.Write (value, 0, value.Length);
			for (int i = value.Length; i < length; ++i)
				o.WriteByte (0x0);
			o.WriteByte (0x0);
		}

		public abstract void WriteJavaToManaged (Stream output);

		public abstract void WriteManagedToJava (Stream output);

		public void Dispose ()
		{
			Dispose (true);
			GC.SuppressFinalize (this);
		}

		protected abstract void Dispose (bool disposing);
	}
}
