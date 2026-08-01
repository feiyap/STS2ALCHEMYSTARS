using System;
using Godot;

namespace Valencina.ValencinaCode.Utils;

internal static class ValencinaMobileCompat
{
	private static bool? _isAndroid;

	internal static bool IsAndroid
	{
		get
		{
			if (_isAndroid.HasValue)
			{
				return _isAndroid.Value;
			}
			try
			{
				_isAndroid = string.Equals(OS.GetName(), "Android", StringComparison.OrdinalIgnoreCase);
			}
			catch
			{
				_isAndroid = OperatingSystem.IsAndroid();
			}
			return _isAndroid.Value;
		}
	}

	internal static bool UsePoollessKaiserFinalBoss => IsAndroid;
}
