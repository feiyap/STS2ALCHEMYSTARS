using System;
using System.Reflection;

namespace Valencina.ValencinaCode.Systems;

public static class ValencinaRunTracker
{
	public static bool IsValencinaRun { get; private set; }

	public static void Clear()
	{
		IsValencinaRun = false;
	}

	public static void MarkCharacter(object? model)
	{
		IsValencinaRun = IsValencinaModel(model);
	}

	public static bool IsValencinaModel(object? model)
	{
		if (model == null)
		{
			return false;
		}
		_003C_003Ey__InlineArray5<string> buffer = default(_003C_003Ey__InlineArray5<string>);
		global::_003CPrivateImplementationDetails_003E.InlineArrayElementRef<_003C_003Ey__InlineArray5<string>, string>(ref buffer, 0) = model.GetType().FullName ?? string.Empty;
		global::_003CPrivateImplementationDetails_003E.InlineArrayElementRef<_003C_003Ey__InlineArray5<string>, string>(ref buffer, 1) = model.GetType().Name;
		global::_003CPrivateImplementationDetails_003E.InlineArrayElementRef<_003C_003Ey__InlineArray5<string>, string>(ref buffer, 2) = ReadStringMember(model, "Id");
		global::_003CPrivateImplementationDetails_003E.InlineArrayElementRef<_003C_003Ey__InlineArray5<string>, string>(ref buffer, 3) = ReadStringMember(model, "CharacterId");
		global::_003CPrivateImplementationDetails_003E.InlineArrayElementRef<_003C_003Ey__InlineArray5<string>, string>(ref buffer, 4) = ReadStringMember(model, "Name");
		string text = string.Join(" | ", global::_003CPrivateImplementationDetails_003E.InlineArrayAsReadOnlySpan<_003C_003Ey__InlineArray5<string>, string>(in buffer, 5));
		if (!text.Contains("VALENCINA", StringComparison.OrdinalIgnoreCase) && !text.Contains("Valencina", StringComparison.OrdinalIgnoreCase))
		{
			return text.Contains("瓦伦希娜", StringComparison.OrdinalIgnoreCase);
		}
		return true;
	}

	private static string ReadStringMember(object obj, string memberName)
	{
		try
		{
			object obj2 = obj.GetType().GetProperty(memberName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)?.GetValue(obj);
			if (obj2 != null)
			{
				return obj2.ToString() ?? string.Empty;
			}
		}
		catch
		{
		}
		try
		{
			object obj4 = obj.GetType().GetField(memberName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)?.GetValue(obj);
			if (obj4 != null)
			{
				return obj4.ToString() ?? string.Empty;
			}
		}
		catch
		{
		}
		return string.Empty;
	}
}
