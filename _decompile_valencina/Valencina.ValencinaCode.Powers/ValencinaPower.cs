using System;
using System.Collections.Generic;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Scaffolding.Content;
using Valencina.ValencinaCode.Cards;
using Valencina.ValencinaCode.Compat;
using Valencina.ValencinaCode.Extensions;

namespace Valencina.ValencinaCode.Powers;

public abstract class ValencinaPower : ModPowerTemplate
{
	public override string? CustomIconPath => PowerIconRegistry.GetPackedIconPath(((object)this).GetType(), ((AbstractModel)this).Id.Entry.RemovePrefix().ToLowerInvariant() + ".png");

	public override string? CustomBigIconPath => PowerIconRegistry.GetBigIconPath(((object)this).GetType(), ((AbstractModel)this).Id.Entry.RemovePrefix().ToLowerInvariant() + ".png");

	protected override IEnumerable<IHoverTip> AdditionalHoverTips
	{
		get
		{
			HashSet<CardKeyword> yieldedKeywords = new HashSet<CardKeyword>();
			foreach (CardKeyword item in KeywordTipsForPower(((object)this).GetType()))
			{
				if (yieldedKeywords.Add(item))
				{
					yield return HoverTipFactory.FromKeyword(item);
				}
			}
			foreach (IHoverTip item2 in VanillaPowerTipsForPower(((object)this).GetType()))
			{
				yield return item2;
			}
		}
	}

	private static IEnumerable<CardKeyword> KeywordTipsForPower(Type powerType)
	{
		string name = powerType.Name;
		bool flag = !HasSelfContainedAmmoTooltip(powerType);
		if (flag)
		{
			bool flag2 = name.Contains("Ammo", StringComparison.Ordinal);
			if (!flag2)
			{
				bool flag3;
				switch (name)
				{
				case "OutlawPower":
				case "ThroughFireAndWaterPower":
				case "EmptyChamberPower":
				case "FutureSightPower":
					flag3 = true;
					break;
				default:
					flag3 = false;
					break;
				}
				flag2 = flag3;
			}
			flag = flag2;
		}
		if (flag)
		{
			yield return ValencinaKeywords.Ammo;
		}
		flag = name.Contains("Burn", StringComparison.Ordinal);
		if (!flag)
		{
			bool flag2;
			switch (name)
			{
			case "BurningTremorPower":
			case "ScorchMarkPower":
			case "RollingHotPower":
			case "AfterglowPower":
				flag2 = true;
				break;
			default:
				flag2 = false;
				break;
			}
			flag = flag2;
		}
		if (flag)
		{
			yield return ValencinaKeywords.Burn;
		}
		flag = name.Contains("Tremor", StringComparison.Ordinal);
		if (!flag)
		{
			bool flag2;
			switch (name)
			{
			case "BurningTremorPower":
			case "ScorchMarkPower":
			case "VisceraCrushPower":
			case "RollingHotPower":
			case "SoWeakPower":
				flag2 = true;
				break;
			default:
				flag2 = false;
				break;
			}
			flag = flag2;
		}
		if (flag)
		{
			yield return ValencinaKeywords.Tremor;
		}
		flag = name.Contains("Dodge", StringComparison.Ordinal);
		if (!flag)
		{
			bool flag2;
			switch (name)
			{
			case "DuelTempoPower":
			case "RedThreadPower":
			case "HunterMarkPower":
			case "InstantForesightPower":
			case "CrystalClearPower":
			case "SettlementCompensationPower":
			case "FutureSightPower":
				flag2 = true;
				break;
			default:
				flag2 = false;
				break;
			}
			flag = flag2;
		}
		if (flag)
		{
			yield return ValencinaKeywords.Dodge;
		}
		flag = name.Contains("Counter", StringComparison.Ordinal) && !(name == "GetLostCounterPower");
		if (!flag)
		{
			bool flag2;
			switch (name)
			{
			case "CrystalClearPower":
			case "VisceraCrushPower":
			case "HuntsEndPower":
			case "TightBitePower":
			case "OverwhelmingTechniquePower":
			case "SharpPower":
			case "SecondAccelerationPower":
			case "DespairHopeNoHopePower":
			case "RollingHotPower":
			case "CoordinatedHuntPower":
				flag2 = true;
				break;
			default:
				flag2 = false;
				break;
			}
			flag = flag2;
		}
		if (flag)
		{
			yield return ValencinaKeywords.Counter;
		}
		flag = name.Contains("Foresight", StringComparison.Ordinal) || name.Contains("Odin", StringComparison.Ordinal);
		if (!flag)
		{
			bool flag2;
			switch (name)
			{
			case "OverheatProtectionPower":
			case "ScorchingEyeSocketPower":
			case "FaceMyHatredPower":
			case "ThroughFireAndWaterPower":
			case "FarewellPower":
			case "UnyieldingPower":
			case "MemoryExpansionPower":
				flag2 = true;
				break;
			default:
				flag2 = false;
				break;
			}
			flag = flag2;
		}
		if (flag)
		{
			yield return ValencinaKeywords.OdinEye;
		}
		flag = name.Contains("BreathingMethod", StringComparison.Ordinal) && powerType != typeof(BreathingMethodPower);
		if (!flag)
		{
			bool flag2;
			switch (name)
			{
			case "OverwhelmingTechniquePower":
			case "AcceleratingMomentPower":
			case "ShatterRendPower":
				flag2 = true;
				break;
			default:
				flag2 = false;
				break;
			}
			flag = flag2;
		}
		if (flag)
		{
			yield return ValencinaKeywords.BreathingMethod;
		}
		if (name == "BurningTremorPower")
		{
			yield return ValencinaKeywords.AmplitudeConversion;
		}
		switch (name)
		{
		case "BurningTremorPower":
		case "VisceraCrushPower":
		case "RollingHotPower":
		case "ShatterRendPower":
			flag = true;
			break;
		default:
			flag = false;
			break;
		}
		if (flag)
		{
			yield return ValencinaKeywords.TremorDetonation;
		}
	}

	private static IEnumerable<IHoverTip> VanillaPowerTipsForPower(Type powerType)
	{
		string name = powerType.Name;
		string text = name;
		if ((text == "OdinEyePower" || text == "BoundKingPower") ? true : false)
		{
			yield return CompatHoverTips.FromPower<WeakPower>();
		}
		text = name;
		if ((text == "OdinEyePower" || text == "BoundKingPower") ? true : false)
		{
			yield return CompatHoverTips.FromPower<VulnerablePower>();
		}
		if (name == "BoundKingPower")
		{
			yield return CompatHoverTips.FromPower<FrailPower>();
		}
		text = name;
		if ((text == "HeathcliffWarningPower" || text == "TemporaryThornsPower") ? true : false)
		{
			yield return CompatHoverTips.FromPower<ThornsPower>();
		}
		text = name;
		if ((text == "HeathcliffWarningPower" || text == "TearBladePower") ? true : false)
		{
			yield return CompatHoverTips.FromPower<StrengthPower>();
		}
	}

	private static bool HasSelfContainedAmmoTooltip(Type powerType)
	{
		return powerType == typeof(AmmoGainBlockedPower);
	}
}
