#nullable enable

using System;
using System.Runtime.ExceptionServices;

namespace Java.Interop
{
	partial class JniEnvironment
	{
		partial class StaticFields
		{
#if !NETSTANDARD2_0
			public static unsafe JniFieldInfo GetStaticFieldID (JniObjectReference type, ReadOnlySpan<char> name, ReadOnlySpan<char> signature)
			{
				if (!type.IsValid)
					throw new ArgumentException ("Handle must be valid.", "type");
				if (name.IsEmpty)
					throw new ArgumentNullException ("name");
				if (signature.IsEmpty)
					throw new ArgumentNullException ("signature");

				fixed (char* ptr = signature) {
					var tmp = NativeMethods.java_interop_jnienv_get_static_field_id (JniEnvironment.EnvironmentPointer, out var thrown, type.Handle, name.ToString (), ptr);

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