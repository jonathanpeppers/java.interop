#nullable enable

using System;
using System.Diagnostics;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;

namespace Java.Interop
{
	partial class JniEnvironment
	{
		partial class StaticFields
		{
#if !NETSTANDARD2_0
			internal static unsafe JniFieldInfo GetStaticFieldID (JniObjectReference type, ReadOnlySpan<char> name, ReadOnlySpan<char> signature)
			{
				if (!type.IsValid)
					throw new ArgumentException ("Handle must be valid.", "type");
				if (name.IsEmpty)
					throw new ArgumentNullException ("name");
				if (signature.IsEmpty)
					throw new ArgumentNullException ("signature");

				fixed (char* sig = &MemoryMarshal.GetReference (signature)) {
					Debug.Assert (sig [signature.Length] == 0, signature.ToString () + " should be a null-terminated string");
					var tmp = NativeMethods.java_interop_jnienv_get_static_field_id (JniEnvironment.EnvironmentPointer, out var thrown, type.Handle, name.ToString (), sig);

					var __e = JniEnvironment.GetExceptionForLastThrowable (thrown);
					if (__e != null)
						ExceptionDispatchInfo.Capture (__e).Throw ();
					if (tmp == IntPtr.Zero)
						return null!;
					return new JniFieldInfo (name, signature, tmp, isStatic: true);
				}
			}
#endif
		}
	}
}
