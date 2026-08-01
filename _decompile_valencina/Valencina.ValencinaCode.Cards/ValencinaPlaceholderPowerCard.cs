using System.Collections.Generic;
using MegaCrit.Sts2.Core.Entities.Cards;
using Valencina.ValencinaCode.Extensions;

namespace Valencina.ValencinaCode.Cards;

public abstract class ValencinaPlaceholderPowerCard : ValencinaCard
{
	public override string CustomPortraitPath => "card.png".BigCardImagePath();

	public override string PortraitPath => "card.png".CardImagePath();

	public override string BetaPortraitPath => "card.png".CardImagePath();

	public override IEnumerable<CardKeyword> CanonicalKeywords
	{
		get
		{
			HashSet<CardKeyword> emitted = new HashSet<CardKeyword>();
			foreach (CardKeyword canonicalKeyword in base.CanonicalKeywords)
			{
				if (emitted.Add(canonicalKeyword))
				{
					yield return canonicalKeyword;
				}
			}
			foreach (CardKeyword item in PrecognitionPowerKeywordsFor(((object)this).GetType().Name))
			{
				if (emitted.Add(item))
				{
					yield return item;
				}
			}
		}
	}

	protected ValencinaPlaceholderPowerCard(int cost, CardRarity rarity, bool showInCardLibrary = true, bool autoAdd = true)
		: base(cost, (CardType)3, rarity, (TargetType)1, showInCardLibrary, autoAdd)
	{
	}//IL_0003: Unknown result type (might be due to invalid IL or missing references)


	private static IEnumerable<CardKeyword> PrecognitionPowerKeywordsFor(string typeName)
	{
		if (typeName == null)
		{
			yield break;
		}
		switch (typeName.Length)
		{
		case 12:
			switch (typeName[0])
			{
			case 'F':
				if (typeName == "FaceMyHatred")
				{
					yield return ValencinaKeywords.OdinEye;
				}
				break;
			case 'E':
				if (typeName == "EmptyChamber")
				{
					yield return ValencinaKeywords.Ammo;
				}
				break;
			case 'C':
				if (typeName == "CrystalClear")
				{
					yield return ValencinaKeywords.BreathingMethod;
				}
				break;
			}
			break;
		case 10:
			switch (typeName[0])
			{
			case 'U':
				if (typeName == "Unyielding")
				{
					yield return ValencinaKeywords.Acceleration;
				}
				break;
			case 'F':
				if (typeName == "FireSpread")
				{
					yield return ValencinaKeywords.Disposal;
				}
				break;
			}
			break;
		case 11:
			if (typeName == "OdinEyeCard")
			{
				yield return ValencinaKeywords.OdinEye;
				yield return ValencinaKeywords.Dodge;
			}
			break;
		case 18:
			if (typeName == "ScorchingEyeSocket")
			{
				yield return ValencinaKeywords.OdinEye;
			}
			break;
		case 17:
			if (typeName == "DespairHopeNoHope")
			{
				yield return ValencinaKeywords.Counter;
			}
			break;
		case 14:
			if (typeName == "GunMaintenance")
			{
				yield return ValencinaKeywords.Ammo;
				yield return ValencinaKeywords.BreathingMethod;
			}
			break;
		case 13:
			if (typeName == "CalmReloading")
			{
			}
			break;
		case 9:
			if (typeName == "Afterglow")
			{
				yield return ValencinaKeywords.BreathingMethod;
			}
			break;
		case 5:
			if (typeName == "SoHot")
			{
				yield return ValencinaKeywords.Burn;
			}
			break;
		case 6:
			if (typeName == "SoWeak")
			{
				yield return ValencinaKeywords.Tremor;
			}
			break;
		}
	}
}
