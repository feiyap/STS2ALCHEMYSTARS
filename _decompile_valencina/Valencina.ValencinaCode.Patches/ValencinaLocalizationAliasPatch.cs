using System;
using HarmonyLib;
using MegaCrit.Sts2.Core.Localization;

namespace Valencina.ValencinaCode.Patches;

[HarmonyPatch(typeof(LocTable), "GetRawText")]
internal static class ValencinaLocalizationAliasPatch
{
	private const string CanonicalModId = "VALENCINA";

	private const string ModPrefix = "Valencina_";

	private static readonly string[] ModelKindPrefixes = new string[7] { "POWER", "CHARACTER", "EVENT", "ANCIENT", "ENCOUNTER", "MONSTER", "ENCHANTMENT" };

	private static void Prefix(ref string key)
	{
		key = ToLegacyLocKey(key);
	}

	internal static string ToLegacyLocKey(string key)
	{
		if (!key.StartsWith("Valencina_", StringComparison.OrdinalIgnoreCase))
		{
			return key;
		}
		if (key.StartsWith("VALENCINA_EVENT_COCKROACH_EMPEROR_PASSIVE_DISABLE_EVENT", StringComparison.OrdinalIgnoreCase))
		{
			return key;
		}
		string text = key;
		int length = "Valencina_".Length;
		string text2 = text.Substring(length, text.Length - length);
		if (text2.StartsWith("KEYWORD_", StringComparison.OrdinalIgnoreCase))
		{
			text = text2;
			length = "KEYWORD_".Length;
			return "VALENCINA_" + UppercaseEntryPart(text.Substring(length, text.Length - length));
		}
		string[] modelKindPrefixes = ModelKindPrefixes;
		for (length = 0; length < modelKindPrefixes.Length; length++)
		{
			string text3 = modelKindPrefixes[length] + "_";
			if (text2.StartsWith(text3, StringComparison.OrdinalIgnoreCase))
			{
				text = text2;
				int length2 = text3.Length;
				return "VALENCINA-" + UppercaseEntryPart(text.Substring(length2, text.Length - length2));
			}
		}
		return "VALENCINA_" + UppercaseEntryPart(text2);
	}

	private static string UppercaseEntryPart(string value)
	{
		int num = value.IndexOf('.', StringComparison.Ordinal);
		if (num < 0)
		{
			return value.ToUpperInvariant();
		}
		string text = value.Substring(0, num).ToUpperInvariant();
		int num2 = num;
		return text + value.Substring(num2, value.Length - num2);
	}
}
