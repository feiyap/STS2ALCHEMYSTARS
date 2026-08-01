using System;

namespace Valencina.ValencinaCode.Extensions;

public static class StringExtensions
{
	public static string ImagePath(this string path)
	{
		return "res://Valencina/images/" + path;
	}

	public static string CardImagePath(this string path)
	{
		return "res://Valencina/images/card_portraits/" + path;
	}

	public static string BigCardImagePath(this string path)
	{
		return "res://Valencina/images/card_portraits/big/" + path;
	}

	public static string PowerImagePath(this string path)
	{
		return "res://Valencina/images/powers/" + path;
	}

	public static string BigPowerImagePath(this string path)
	{
		return "res://Valencina/images/powers/big/" + path;
	}

	public static string RelicImagePath(this string path)
	{
		return "res://Valencina/images/relics/" + path;
	}

	public static string BigRelicImagePath(this string path)
	{
		return "res://Valencina/images/relics/big/" + path;
	}

	public static string CharacterUiPath(this string path)
	{
		return "res://Valencina/images/charui/" + path;
	}

	public static string RemovePrefix(this string entry)
	{
		if (string.IsNullOrWhiteSpace(entry))
		{
			return entry;
		}
		string text = entry.Replace('-', '_');
		string[] array = new string[10] { "VALENCINA_CARD_", "VALENCINA_RELIC_", "VALENCINA_POTION_", "VALENCINA_POWER_", "VALENCINA_KEYWORD_", "VALENCINASTS2_CARD_", "VALENCINASTS2_RELIC_", "VALENCINASTS2_POWER_", "VALENCINA_", "VALENCINASTS2_" };
		string text3;
		foreach (string text2 in array)
		{
			if (text.StartsWith(text2, StringComparison.OrdinalIgnoreCase))
			{
				text3 = text;
				int length = text2.Length;
				return text3.Substring(length, text3.Length - length);
			}
		}
		int num = entry.IndexOf('-', StringComparison.Ordinal);
		if (num < 0 || num + 1 >= entry.Length)
		{
			return text;
		}
		text3 = entry;
		int i = num + 1;
		return text3.Substring(i, text3.Length - i);
	}
}
