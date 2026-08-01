using MegaCrit.Sts2.Core.Entities.Cards;
using STS2RitsuLib.Content;
using STS2RitsuLib.Keywords;

namespace Valencina.ValencinaCode.Cards;

public static class ValencinaKeywords
{
	private static readonly string[] Stems = new string[16]
	{
		"AMMO", "INSTANT", "TREMOR", "BURN", "AMPLITUDE_CONVERSION", "TREMOR_DETONATION", "GAZE", "ODIN_EYE", "TEMPORARY_ODIN_EYE", "DODGE",
		"COUNTER", "BREATHING_METHOD", "UNFIRED", "WOUNDING", "ACCELERATION", "DISPOSAL_KEYWORD"
	};

	public static CardKeyword Ammo => Get("AMMO");

	public static CardKeyword Instant => Get("INSTANT");

	public static CardKeyword Tremor => Get("TREMOR");

	public static CardKeyword Burn => Get("BURN");

	public static CardKeyword AmplitudeConversion => Get("AMPLITUDE_CONVERSION");

	public static CardKeyword TremorDetonation => Get("TREMOR_DETONATION");

	public static CardKeyword Gaze => Get("GAZE");

	public static CardKeyword OdinEye => Get("ODIN_EYE");

	public static CardKeyword TemporaryOdinEye => Get("TEMPORARY_ODIN_EYE");

	public static CardKeyword Dodge => Get("DODGE");

	public static CardKeyword Counter => Get("COUNTER");

	public static CardKeyword BreathingMethod => Get("BREATHING_METHOD");

	public static CardKeyword Unfired => Get("UNFIRED");

	public static CardKeyword Wounding => Get("WOUNDING");

	public static CardKeyword Acceleration => Get("ACCELERATION");

	public static CardKeyword Disposal => Get("DISPOSAL_KEYWORD");

	public static void RegisterAll()
	{
		ModKeywordRegistry val = ModKeywordRegistry.For("Valencina");
		string[] stems = Stems;
		foreach (string text in stems)
		{
			val.RegisterOwned(text, "card_keywords", LegacyLocKey(text, "title"), "card_keywords", LegacyLocKey(text, "description"), (string)null);
		}
	}

	public static string Id(string stem)
	{
		return ModContentRegistry.GetQualifiedKeywordId("Valencina", stem);
	}

	private static CardKeyword Get(string stem)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		return ModKeywordRegistry.GetCardKeyword(Id(stem));
	}

	private static string LegacyLocKey(string stem, string suffix)
	{
		return $"{"Valencina".ToUpperInvariant()}_{stem}.{suffix}";
	}
}
