using System;
using System.Threading;

namespace Valencina.ValencinaCode.Utils;

public static class InstantAttackState
{
	private sealed class Scope : IDisposable
	{
		private bool _disposed;

		public void Dispose()
		{
			if (!_disposed)
			{
				_disposed = true;
				if (Depth.Value > 0)
				{
					Depth.Value -= 1;
				}
			}
		}
	}

	private static readonly AsyncLocal<int> Depth = new AsyncLocal<int>();

	public static bool IsActive => Depth.Value > 0;

	public static IDisposable Enter()
	{
		Depth.Value += 1;
		return new Scope();
	}
}
