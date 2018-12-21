using System;

namespace Test
{
	public class Foo : Bar { }

	public class Bar : IBar
	{
		public int GetInt () => throw new NotImplementedException ();

		public int [] GetIntArray () => throw new NotImplementedException ();
	}

	public interface IFoo : IBar { }

	public interface IBar
	{
		int GetInt ();

		int [] GetIntArray ();
	}

	public struct MyStruct { }

	public enum MyEnum
	{
		A,
		B,
		C
	}

	public class Disposable : IDisposable
	{
		public void Dispose () => throw new NotImplementedException ();
	}

	public class Generic<T> { }
}
