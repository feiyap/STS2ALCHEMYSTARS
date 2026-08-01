using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Combat.History;
using MegaCrit.Sts2.Core.Combat.History.Entries;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Scaffolding.Content;
using Valencina.ValencinaCode.Character;
using Valencina.ValencinaCode.Compat;
using Valencina.ValencinaCode.Extensions;
using Valencina.ValencinaCode.Patches;
using Valencina.ValencinaCode.Powers;
using Valencina.ValencinaCode.Utils;

namespace Valencina.ValencinaCode.Cards;

public abstract class ValencinaCard : ModCardTemplate
{
	private sealed class ReferenceEqualityComparer : IEqualityComparer<object>
	{
		public static readonly ReferenceEqualityComparer Instance = new ReferenceEqualityComparer();

		public new bool Equals(object? x, object? y)
		{
			return x == y;
		}

		public int GetHashCode(object obj)
		{
			return RuntimeHelpers.GetHashCode(obj);
		}
	}

	private sealed class TurnTracker
	{
		public int Token;

		public int LastCardsPlayedThisTurn;
	}

	private static readonly Dictionary<object, TurnTracker> CombatTurnTrackers = new Dictionary<object, TurnTracker>(ReferenceEqualityComparer.Instance);

	private int _pendingBreathingIntensityGain;

	private int _pendingBreathingChargesGain;

	public bool AutoAddToCardPool { get; }

	public virtual bool SpendsAmmo => false;

	public virtual int AmmoSpendPreviewAmount => 0;

	public virtual string AmmoSpendPreviewText
	{
		get
		{
			if (AmmoSpendPreviewAmount <= 0)
			{
				return string.Empty;
			}
			return AmmoSpendPreviewAmount.ToString();
		}
	}

	public virtual bool ShowAmmoSpendPreview => !string.IsNullOrEmpty(AmmoSpendPreviewText);

	public override string CustomPortraitPath => (((AbstractModel)this).Id.Entry.RemovePrefix().ToLowerInvariant() + ".png").BigCardImagePath();

	public override string PortraitPath => (((AbstractModel)this).Id.Entry.RemovePrefix().ToLowerInvariant() + ".png").CardImagePath();

	public override string BetaPortraitPath => ("beta/" + ((AbstractModel)this).Id.Entry.RemovePrefix().ToLowerInvariant() + ".png").CardImagePath();

	public override CardPoolModel VisualCardPool => (CardPoolModel)(object)ModelDb.CardPool<ValencinaCardPool>();

	protected virtual IEnumerable<CardKeyword> TooltipKeywords
	{
		get
		{
			yield break;
		}
	}

	public override IEnumerable<CardKeyword> CanonicalKeywords
	{
		get
		{
			HashSet<CardKeyword> yielded = new HashSet<CardKeyword>();
			foreach (CardKeyword item in _003C_003En__0())
			{
				if (yielded.Add(item))
				{
					yield return item;
				}
			}
			if (this is IInstantAttackCard && yielded.Add(ValencinaKeywords.Instant))
			{
				yield return ValencinaKeywords.Instant;
			}
			if (((CardModel)this).GainsBlock && yielded.Add(ValencinaKeywords.Dodge))
			{
				yield return ValencinaKeywords.Dodge;
			}
			foreach (CardKeyword item2 in MechanicKeywordsFor(((AbstractModel)this).Id.Entry))
			{
				if (yielded.Add(item2))
				{
					yield return item2;
				}
			}
		}
	}

	protected override IEnumerable<IHoverTip> AdditionalHoverTips
	{
		get
		{
			HashSet<CardKeyword> tooltipKeywords = new HashSet<CardKeyword>();
			foreach (CardKeyword tooltipKeyword in TooltipKeywords)
			{
				if (tooltipKeywords.Add(tooltipKeyword))
				{
					yield return HoverTipFactory.FromKeyword(tooltipKeyword);
				}
			}
			if (HasWeakPowerTip(((AbstractModel)this).Id.Entry))
			{
				yield return CompatHoverTips.FromPower<WeakPower>();
			}
			if (HasVulnerablePowerTip(((AbstractModel)this).Id.Entry))
			{
				yield return CompatHoverTips.FromPower<VulnerablePower>();
			}
			if (HasStrengthPowerTip(((AbstractModel)this).Id.Entry))
			{
				yield return CompatHoverTips.FromPower<StrengthPower>();
			}
			if (HasBufferPowerTip(((AbstractModel)this).Id.Entry))
			{
				yield return CompatHoverTips.FromPower<BufferPower>();
			}
			if (HasDestinedFuturePowerTip(((AbstractModel)this).Id.Entry))
			{
				yield return CompatHoverTips.FromPower<DestinedFuturePower>();
			}
			if (HasShinAmmoRefundPowerTip(((AbstractModel)this).Id.Entry))
			{
				yield return CompatHoverTips.FromPower<ShinAmmoRefundPower>();
			}
			if (HasHuntingTargetPowerTip(((AbstractModel)this).Id.Entry))
			{
				yield return CompatHoverTips.FromPower<HuntingTargetPower>();
			}
			if (HasHunterMarkPowerTip(((AbstractModel)this).Id.Entry))
			{
				yield return CompatHoverTips.FromPower<HunterMarkPower>();
			}
		}
	}

	protected ValencinaCard(int cost, CardType type, CardRarity rarity, TargetType target, bool showInCardLibrary = true, bool autoAdd = true)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		AutoAddToCardPool = autoAdd;
		((ModCardTemplate)this)._002Ector(cost, type, rarity, target, showInCardLibrary);
	}

	internal void QueueBreathingIntensityGain(int amount)
	{
		QueueBreathingMethodGain(amount, 0);
	}

	internal void QueueBreathingMethodGain(int intensity, int charges)
	{
		if (intensity > 0)
		{
			_pendingBreathingIntensityGain += intensity;
		}
		if (charges > 0)
		{
			_pendingBreathingChargesGain += charges;
		}
	}

	internal async Task FlushPendingBreathingMethodGainAsync(PlayerChoiceContext choiceContext)
	{
		int pendingBreathingIntensityGain = _pendingBreathingIntensityGain;
		int pendingBreathingChargesGain = _pendingBreathingChargesGain;
		_pendingBreathingIntensityGain = 0;
		_pendingBreathingChargesGain = 0;
		if (pendingBreathingIntensityGain > 0 || pendingBreathingChargesGain > 0)
		{
			Player owner = ((CardModel)this).Owner;
			if (((owner != null) ? owner.Creature : null) != null)
			{
				await BreathingMethodService.GainIntensityAndChargesAsync(((CardModel)this).Owner.Creature, pendingBreathingIntensityGain, pendingBreathingChargesGain, (CardModel?)(object)this, choiceContext);
			}
		}
	}

	internal static void ClearCombatTurnTrackers()
	{
		CombatTurnTrackers.Clear();
	}

	private static IEnumerable<CardKeyword> MechanicKeywordsFor(string id)
	{
		string text = NormalizeMechanicId(id);
		if (text == null)
		{
			yield break;
		}
		switch (text.Length)
		{
		case 18:
			switch (text[12])
			{
			case 'F':
				if (text == "ACCELERATED_FUTURE")
				{
					yield return ValencinaKeywords.Ammo;
				}
				break;
			case 'R':
				if (text == "ACCELERATOR_RELOAD")
				{
					yield return ValencinaKeywords.Ammo;
					yield return ValencinaKeywords.BreathingMethod;
				}
				break;
			case 'E':
				if (text == "HATRED_AND_DELIGHT")
				{
					yield return ValencinaKeywords.Ammo;
				}
				break;
			case 'U':
				if (text == "CONTINUOUS_CUTTING")
				{
					yield return ValencinaKeywords.Counter;
				}
				break;
			case 'I':
				if (text == "PREEMPTIVE_RIPOSTE")
				{
					yield return ValencinaKeywords.Ammo;
					yield return ValencinaKeywords.OdinEye;
					yield return ValencinaKeywords.Dodge;
				}
				break;
			}
			break;
		case 19:
			switch (text[0])
			{
			case 'A':
				if (text == "ACCELERATING_MOMENT")
				{
					yield return ValencinaKeywords.BreathingMethod;
				}
				break;
			case 'B':
				if (text == "BOOMERANG_SHOCKWAVE")
				{
					yield return ValencinaKeywords.Ammo;
					yield return ValencinaKeywords.Tremor;
					yield return ValencinaKeywords.TremorDetonation;
				}
				break;
			case 'E':
				if (text == "ENDURED_HUMILIATION")
				{
					yield return ValencinaKeywords.Ammo;
				}
				break;
			case 'O':
				if (text == "OVERLOADED_MAGAZINE")
				{
					yield return ValencinaKeywords.Ammo;
				}
				break;
			}
			break;
		case 22:
			switch (text[0])
			{
			case 'A':
				if (text == "ACCUMULATED_EXPERIENCE")
				{
					yield return ValencinaKeywords.Ammo;
					yield return ValencinaKeywords.Tremor;
					yield return ValencinaKeywords.Burn;
				}
				break;
			case 'H':
				if (text == "HATRED_FUTURE_DISPOSAL")
				{
					yield return ValencinaKeywords.Disposal;
					yield return ValencinaKeywords.Ammo;
					yield return ValencinaKeywords.Tremor;
					yield return ValencinaKeywords.AmplitudeConversion;
					yield return ValencinaKeywords.TremorDetonation;
					yield return ValencinaKeywords.Gaze;
				}
				break;
			case 'S':
				if (text == "SCORCHING_BREAKTHROUGH")
				{
					yield return ValencinaKeywords.Burn;
				}
				break;
			case 'O':
				if (text == "OVERWHELMING_TECHNIQUE")
				{
					yield return ValencinaKeywords.Counter;
					yield return ValencinaKeywords.BreathingMethod;
				}
				break;
			case 'T':
				if (text == "THROUGH_FIRE_AND_WATER")
				{
					yield return ValencinaKeywords.Ammo;
					yield return ValencinaKeywords.OdinEye;
				}
				break;
			}
			break;
		case 9:
			switch (text[0])
			{
			case 'A':
				if (text == "AFTERGLOW")
				{
					yield return ValencinaKeywords.Burn;
				}
				break;
			case 'D':
				if (text == "DISMEMBER")
				{
					yield return ValencinaKeywords.Ammo;
					yield return ValencinaKeywords.Tremor;
					yield return ValencinaKeywords.AmplitudeConversion;
				}
				break;
			case 'L':
				if (text == "LEAD_RAIN")
				{
					yield return ValencinaKeywords.Ammo;
				}
				break;
			case 'I':
				if (text == "IMPARTIAL")
				{
					yield return ValencinaKeywords.Ammo;
					yield return ValencinaKeywords.OdinEye;
					yield return ValencinaKeywords.Dodge;
				}
				break;
			case 'S':
				if (text == "SIDE_STEP")
				{
					yield return ValencinaKeywords.Ammo;
					yield return ValencinaKeywords.Dodge;
				}
				break;
			case 'J':
				if (text == "JIE_XIANG")
				{
					yield return ValencinaKeywords.Counter;
					yield return ValencinaKeywords.TremorDetonation;
				}
				break;
			}
			break;
		case 13:
			switch (text[2])
			{
			case 'H':
				if (text == "ACHILLES_TEAR")
				{
					yield return ValencinaKeywords.Ammo;
				}
				break;
			case 'A':
				if (text == "CHAMBER_SMOKE")
				{
					yield return ValencinaKeywords.Ammo;
					yield return ValencinaKeywords.Tremor;
					yield return ValencinaKeywords.OdinEye;
				}
				break;
			case 'Y':
				if (text == "CRYSTAL_CLEAR")
				{
					yield return ValencinaKeywords.BreathingMethod;
				}
				break;
			case 'T':
				if (text == "CUTTING_SWORD")
				{
					yield return ValencinaKeywords.BreathingMethod;
				}
				break;
			case 'P':
				if (text == "EMPTY_CHAMBER")
				{
					yield return ValencinaKeywords.Ammo;
				}
				break;
			case 'G':
				if (text == "HIGH_PRESSURE")
				{
					yield return ValencinaKeywords.Ammo;
				}
				break;
			case 'N':
				if (text == "GUN_EXECUTION")
				{
					yield return ValencinaKeywords.Ammo;
				}
				break;
			case 'I':
				if (text == "ODIN_EYE_CARD")
				{
					yield return ValencinaKeywords.OdinEye;
					yield return ValencinaKeywords.Dodge;
				}
				break;
			case 'S':
				if (text == "VISCERA_CRUSH")
				{
					yield return ValencinaKeywords.Ammo;
					yield return ValencinaKeywords.Counter;
				}
				break;
			case 'L':
				if (text == "WELL_PREPARED")
				{
					yield return ValencinaKeywords.Ammo;
					yield return ValencinaKeywords.Dodge;
				}
				break;
			}
			break;
		case 17:
			switch (text[0])
			{
			case 'A':
				if (text == "AIM_FOR_THE_HEART")
				{
					yield return ValencinaKeywords.Counter;
				}
				break;
			case 'B':
				if (text == "BULLET_PROPULSION")
				{
					yield return ValencinaKeywords.Ammo;
				}
				break;
			case 'C':
				if (text == "CRIPPLED_ODIN_EYE")
				{
					yield return ValencinaKeywords.OdinEye;
				}
				break;
			case 'P':
				if (text == "PALERMO_EXECUTION")
				{
					yield return ValencinaKeywords.Counter;
				}
				break;
			case 'Q':
				if (text == "QUICK_CALCULATION")
				{
					yield return ValencinaKeywords.OdinEye;
				}
				break;
			}
			break;
		case 15:
			switch (text[9])
			{
			case 'E':
				if (text == "BLESSED_RELEASE")
				{
					yield return ValencinaKeywords.Tremor;
				}
				break;
			case 'Q':
				if (text == "CLASS_ETIQUETTE")
				{
					yield return ValencinaKeywords.TremorDetonation;
				}
				break;
			case 'S':
				if (text == "FUTURE_DISPOSAL")
				{
					yield return ValencinaKeywords.Disposal;
					yield return ValencinaKeywords.Ammo;
					yield return ValencinaKeywords.Tremor;
					yield return ValencinaKeywords.AmplitudeConversion;
					yield return ValencinaKeywords.TremorDetonation;
					yield return ValencinaKeywords.Gaze;
				}
				break;
			case 'R':
				if (text == "INFINITE_RELOAD")
				{
					yield return ValencinaKeywords.Ammo;
					yield return ValencinaKeywords.Tremor;
					yield return ValencinaKeywords.TremorDetonation;
				}
				break;
			case 'B':
				if (text == "PIERCING_BULLET")
				{
					yield return ValencinaKeywords.Tremor;
					yield return ValencinaKeywords.Burn;
					yield return ValencinaKeywords.TremorDetonation;
				}
				break;
			case 'C':
				if (text == "RECKLESS_CHARGE")
				{
					yield return ValencinaKeywords.Burn;
					yield return ValencinaKeywords.OdinEye;
				}
				break;
			case 'N':
				if (text == "RIFLING_ENGRAVE")
				{
					yield return ValencinaKeywords.Ammo;
					yield return ValencinaKeywords.Counter;
				}
				break;
			case '_':
				if (text == "VIBRATING_BLADE")
				{
					yield return ValencinaKeywords.Tremor;
					yield return ValencinaKeywords.TremorDetonation;
				}
				break;
			case 'F':
				if (text == "PURSUING_FLURRY")
				{
					yield return ValencinaKeywords.Ammo;
					yield return ValencinaKeywords.Counter;
				}
				break;
			}
			break;
		case 14:
			switch (text[4])
			{
			case 'E':
				if (text == "BULLET_TRIBUTE")
				{
					yield return ValencinaKeywords.Ammo;
					yield return ValencinaKeywords.Dodge;
				}
				break;
			case 'G':
				if (text == "EMERGENCY_AMMO")
				{
					yield return ValencinaKeywords.Ammo;
				}
				break;
			case '_':
				if (text == "FACE_MY_HATRED")
				{
					yield return ValencinaKeywords.Burn;
				}
				break;
			case 'W':
				if (text == "HOT_WINE_BLADE")
				{
					yield return ValencinaKeywords.Ammo;
					yield return ValencinaKeywords.Burn;
				}
				break;
			case 'R':
				if (text == "PALERMO_RAPIER")
				{
					yield return ValencinaKeywords.Tremor;
					yield return ValencinaKeywords.Burn;
				}
				break;
			case 'S':
				if (text == "PRESSURING_YOU")
				{
					yield return ValencinaKeywords.BreathingMethod;
				}
				break;
			case 'I':
				if (text == "SEARING_STRIKE")
				{
					yield return ValencinaKeywords.Burn;
				}
				break;
			}
			break;
		case 10:
			switch (text[2])
			{
			case 'O':
				if (text == "CLOSING_IN")
				{
					yield return ValencinaKeywords.Ammo;
				}
				break;
			case 'S':
				if (text == "DISCIPLINE")
				{
					yield return ValencinaKeywords.OdinEye;
					yield return ValencinaKeywords.BreathingMethod;
				}
				break;
			case 'E':
				if (text == "DUEL_TEMPO")
				{
					yield return ValencinaKeywords.OdinEye;
					yield return ValencinaKeywords.Dodge;
				}
				break;
			case 'M':
				if (text == "HEMOSTASIS")
				{
					yield return ValencinaKeywords.Dodge;
				}
				break;
			case 'D':
				if (text == "RED_THREAD")
				{
					yield return ValencinaKeywords.Dodge;
				}
				break;
			case 'W':
				if (text == "SAW_IT_ALL")
				{
					yield return ValencinaKeywords.Acceleration;
				}
				break;
			case 'I':
				if (text == "SLICED_EYE")
				{
					yield return ValencinaKeywords.OdinEye;
				}
				break;
			case 'Y':
				if (text == "UNYIELDING")
				{
				}
				break;
			case 'L':
				if (text == "VAL_DEFEND")
				{
					yield return ValencinaKeywords.Dodge;
				}
				break;
			case 'N':
				if (text == "VENT_ANGER")
				{
					yield return ValencinaKeywords.Ammo;
				}
				break;
			}
			break;
		case 5:
			switch (text[0])
			{
			case 'C':
				if (text == "COWER")
				{
					yield return ValencinaKeywords.Ammo;
					yield return ValencinaKeywords.Dodge;
				}
				break;
			case 'L':
				if (text == "LUCIO")
				{
					yield return ValencinaKeywords.Ammo;
					yield return ValencinaKeywords.Tremor;
					yield return ValencinaKeywords.TremorDetonation;
				}
				break;
			case 'P':
				if (text == "PRIDE")
				{
					yield return ValencinaKeywords.BreathingMethod;
				}
				break;
			case 'T':
				if (text == "TAUNT")
				{
					yield return ValencinaKeywords.Counter;
				}
				break;
			}
			break;
		case 11:
			switch (text[3])
			{
			case 'S':
				if (text == "CROSS_SLASH")
				{
					yield return ValencinaKeywords.Ammo;
				}
				break;
			case 'N':
				if (text == "DAMN_MAGGOT")
				{
					yield return ValencinaKeywords.Tremor;
				}
				break;
			case 'E':
				if (text == "FIRE_TONGUE")
				{
					yield return ValencinaKeywords.Ammo;
					yield return ValencinaKeywords.Tremor;
				}
				break;
			case 'T':
				if (text == "LAST_BULLET")
				{
					yield return ValencinaKeywords.Ammo;
				}
				break;
			case 'L':
				if (text == "ROLLING_HOT")
				{
					yield return ValencinaKeywords.Tremor;
					yield return ValencinaKeywords.Burn;
					yield return ValencinaKeywords.TremorDetonation;
					yield return ValencinaKeywords.Counter;
				}
				break;
			case 'R':
				if (text == "SCORCH_MARK")
				{
					yield return ValencinaKeywords.Tremor;
					yield return ValencinaKeywords.Burn;
				}
				break;
			case 'A':
				if (text == "SO_ANNOYING")
				{
					yield return ValencinaKeywords.Ammo;
					yield return ValencinaKeywords.Tremor;
				}
				break;
			case '_':
				if (text == "LIE_IN_WAIT")
				{
					yield return ValencinaKeywords.Disposal;
				}
				break;
			}
			break;
		case 20:
			switch (text[0])
			{
			case 'D':
				if (text == "DESPAIR_HOPE_NO_HOPE")
				{
					yield return ValencinaKeywords.Counter;
				}
				break;
			case 'S':
				if (text == "SCORCHING_EYE_SOCKET")
				{
					yield return ValencinaKeywords.OdinEye;
				}
				break;
			case 'W':
				if (text == "WEAKPOINT_DETONATION")
				{
					yield return ValencinaKeywords.Tremor;
					yield return ValencinaKeywords.TremorDetonation;
				}
				break;
			}
			break;
		case 6:
			switch (text[3])
			{
			case 'O':
				if (text == "DETOUR")
				{
					yield return ValencinaKeywords.Dodge;
					yield return ValencinaKeywords.BreathingMethod;
				}
				break;
			case 'R':
				if (text == "HATRED")
				{
					yield return ValencinaKeywords.Disposal;
				}
				break;
			case 'T':
				if (text == "HUNTER")
				{
					yield return ValencinaKeywords.Dodge;
				}
				break;
			case 'U':
				if (text == "INSULT")
				{
					yield return ValencinaKeywords.Dodge;
				}
				break;
			case 'I':
				if (text == "SADISM")
				{
					yield return ValencinaKeywords.Ammo;
					yield return ValencinaKeywords.Dodge;
					yield return ValencinaKeywords.Burn;
				}
				break;
			case 'H':
				if (text == "SO_HOT")
				{
					yield return ValencinaKeywords.Burn;
				}
				break;
			case '_':
				if (!(text == "JIE_TU"))
				{
					if (text == "JIE_LU")
					{
						yield return ValencinaKeywords.Counter;
						yield return ValencinaKeywords.Burn;
						yield return ValencinaKeywords.Tremor;
					}
				}
				else
				{
					yield return ValencinaKeywords.Counter;
					yield return ValencinaKeywords.Burn;
					yield return ValencinaKeywords.Tremor;
				}
				break;
			}
			break;
		case 16:
			switch (text[0])
			{
			case 'D':
				if (text == "DISINTEGRATE_ALL")
				{
					yield return ValencinaKeywords.Ammo;
				}
				break;
			case 'C':
				if (text == "COORDINATED_HUNT")
				{
					yield return ValencinaKeywords.Counter;
				}
				break;
			case 'H':
				if (text == "HIGH_TEMPERATURE")
				{
					yield return ValencinaKeywords.Burn;
				}
				break;
			case 'I':
				if (text == "IGNITING_BULLETS")
				{
					yield return ValencinaKeywords.Ammo;
					yield return ValencinaKeywords.Burn;
					yield return ValencinaKeywords.BreathingMethod;
					yield return ValencinaKeywords.Unfired;
				}
				break;
			case 'K':
				if (text == "KINETIC_RECOVERY")
				{
					yield return ValencinaKeywords.Ammo;
				}
				break;
			case 'P':
				if (text == "POINT_BLANK_SHOT")
				{
					yield return ValencinaKeywords.Tremor;
					yield return ValencinaKeywords.Burn;
				}
				break;
			}
			break;
		case 8:
			switch (text[0])
			{
			case 'D':
				if (text == "DISPOSAL")
				{
					yield return ValencinaKeywords.Disposal;
				}
				break;
			case 'F':
				if (text == "FORESEEN")
				{
					yield return ValencinaKeywords.Dodge;
				}
				break;
			case 'G':
				if (text == "GET_LOST")
				{
					yield return ValencinaKeywords.Dodge;
					yield return ValencinaKeywords.Ammo;
				}
				break;
			case 'S':
				if (text == "SLIPSTEP")
				{
					yield return ValencinaKeywords.Ammo;
				}
				break;
			case 'T':
				if (text == "TAKE_AIM")
				{
					yield return ValencinaKeywords.Tremor;
				}
				break;
			case 'B':
				if (text == "BUILD_UP")
				{
					yield return ValencinaKeywords.Dodge;
					yield return ValencinaKeywords.Acceleration;
				}
				break;
			case 'W':
				if (text == "WAR_HERO")
				{
					yield return ValencinaKeywords.Ammo;
				}
				break;
			}
			break;
		case 12:
			switch (text[0])
			{
			case 'E':
				if (text == "EJECT_CASING")
				{
					yield return ValencinaKeywords.Ammo;
				}
				break;
			case 'F':
				if (text == "FUTURE_SIGHT")
				{
					yield return ValencinaKeywords.Ammo;
					yield return ValencinaKeywords.Dodge;
				}
				break;
			case 'G':
				if (text == "GUARD_STANCE")
				{
					yield return ValencinaKeywords.Dodge;
				}
				break;
			case 'P':
				if (text == "PRECOG_DODGE")
				{
					yield return ValencinaKeywords.Ammo;
					yield return ValencinaKeywords.Dodge;
				}
				break;
			case 'S':
				if (!(text == "SHATTER_REND"))
				{
					if (text == "STOP_STARING")
					{
						yield return ValencinaKeywords.Ammo;
					}
				}
				else
				{
					yield return ValencinaKeywords.Burn;
				}
				break;
			case 'C':
				if (text == "CERTAIN_PATH")
				{
					yield return ValencinaKeywords.Ammo;
					yield return ValencinaKeywords.Acceleration;
				}
				break;
			}
			break;
		case 21:
			switch (text[0])
			{
			case 'F':
				if (text == "FLEXIBLE_COORDINATION")
				{
					yield return ValencinaKeywords.Dodge;
				}
				break;
			case 'H':
				if (text == "HIGH_SPEED_TRAJECTORY")
				{
					yield return ValencinaKeywords.Ammo;
					yield return ValencinaKeywords.Dodge;
				}
				break;
			}
			break;
		case 24:
			switch (text[0])
			{
			case 'H':
				if (text == "HIGH_ENERGY_ANNIHILATION")
				{
				}
				break;
			case 'P':
				if (text == "PALERMO_SWORDPLAY_SECRET")
				{
					yield return ValencinaKeywords.Counter;
					yield return ValencinaKeywords.Ammo;
				}
				break;
			}
			break;
		case 4:
			switch (text[1])
			{
			case 'A':
				if (text == "MAIM")
				{
					yield return ValencinaKeywords.Ammo;
				}
				break;
			case 'H':
				if (text == "SHIN")
				{
					yield return ValencinaKeywords.Ammo;
				}
				break;
			case 'M':
				if (text == "SMOG")
				{
					yield return ValencinaKeywords.Ammo;
					yield return ValencinaKeywords.Dodge;
					yield return ValencinaKeywords.BreathingMethod;
					yield return ValencinaKeywords.Unfired;
				}
				break;
			}
			break;
		case 7:
			if (text == "VAGRANT")
			{
				yield return ValencinaKeywords.Dodge;
			}
			break;
		}
	}

	private static string NormalizeMechanicId(string id)
	{
		if (id.StartsWith("VALENCINASTS2-", StringComparison.Ordinal))
		{
			string text = id;
			return text.Substring(14, text.Length - 14);
		}
		if (id.StartsWith("VALENCINASTS2_", StringComparison.Ordinal))
		{
			string text = id;
			return text.Substring(14, text.Length - 14);
		}
		if (id.StartsWith("VALENCINA-", StringComparison.Ordinal))
		{
			string text = id;
			return text.Substring(10, text.Length - 10);
		}
		if (id.StartsWith("VALENCINA_", StringComparison.Ordinal))
		{
			string text = id;
			return text.Substring(10, text.Length - 10);
		}
		return id;
	}

	private static bool HasWeakPowerTip(string id)
	{
		switch (NormalizeMechanicId(id))
		{
		case "ACHILLES_TEAR":
		case "SHATTER_REND":
		case "WEAKPOINT_DETONATION":
			return true;
		default:
			return false;
		}
	}

	private static bool HasVulnerablePowerTip(string id)
	{
		switch (NormalizeMechanicId(id))
		{
		case "CROSS_SLASH":
		case "MAIM":
		case "PRESSURING_YOU":
		case "SHATTER_REND":
		case "WEAKPOINT_DETONATION":
			return true;
		default:
			return false;
		}
	}

	private static bool HasStrengthPowerTip(string id)
	{
		switch (NormalizeMechanicId(id))
		{
		case "DAMN_MAGGOT":
		case "HIGH_TEMPERATURE":
		case "SO_WEAK":
			return true;
		default:
			return false;
		}
	}

	private static bool HasBufferPowerTip(string id)
	{
		if (NormalizeMechanicId(id) == "SHIN")
		{
			return true;
		}
		return false;
	}

	private static bool HasDestinedFuturePowerTip(string id)
	{
		switch (NormalizeMechanicId(id))
		{
		case "FORESEEN":
		case "BUILD_UP":
		case "DISPOSAL":
		case "SAW_IT_ALL":
		case "UNYIELDING":
		case "HIGH_ENERGY_ANNIHILATION":
		case "CERTAIN_PATH":
			return true;
		default:
			return false;
		}
	}

	private static bool HasShinAmmoRefundPowerTip(string id)
	{
		if (NormalizeMechanicId(id) == "SHIN")
		{
			return true;
		}
		return false;
	}

	private static bool HasHuntingTargetPowerTip(string id)
	{
		string text = NormalizeMechanicId(id);
		if (text == "FIRE_SPREAD" || text == "TARGET_DECISION")
		{
			return true;
		}
		return false;
	}

	private static bool HasHunterMarkPowerTip(string id)
	{
		if (NormalizeMechanicId(id) == "HUNTER")
		{
			return true;
		}
		return false;
	}

	protected Task ExecuteAttackAsync(PlayerChoiceContext choiceContext, CardPlay play, int hitCount = 1, string? vfx = null, string? sfx = null, string? tmpSfx = null)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Invalid comparison between Unknown and I4
		if (play.Target == null)
		{
			if ((int)((CardModel)this).TargetType != 3)
			{
				return Task.CompletedTask;
			}
			return ExecuteAttackAllEnemiesAsync(choiceContext, hitCount, vfx, sfx, tmpSfx);
		}
		Player owner = ((CardModel)this).Owner;
		ValencinaAnimation.QueueNextAttackVariant((owner != null) ? owner.Creature : null, hitCount);
		return ExecuteAttackCommandAsync(choiceContext, CommonActions.CardAttack((CardModel)(object)this, play, hitCount, vfx, sfx, tmpSfx));
	}

	protected Task ExecuteAttackAsync(PlayerChoiceContext choiceContext, Creature? target, int hitCount = 1, string? vfx = null, string? sfx = null, string? tmpSfx = null)
	{
		if (target == null)
		{
			return Task.CompletedTask;
		}
		Player owner = ((CardModel)this).Owner;
		ValencinaAnimation.QueueNextAttackVariant((owner != null) ? owner.Creature : null, hitCount);
		return ExecuteAttackCommandAsync(choiceContext, CommonActions.CardAttack((CardModel)(object)this, target, hitCount, vfx, sfx, tmpSfx));
	}

	protected Task ExecuteAttackAsync(PlayerChoiceContext choiceContext, Creature? target, decimal damage, int hitCount = 1, string? vfx = null, string? sfx = null, string? tmpSfx = null)
	{
		if (target == null)
		{
			return Task.CompletedTask;
		}
		Player owner = ((CardModel)this).Owner;
		ValencinaAnimation.QueueNextAttackVariant((owner != null) ? owner.Creature : null, hitCount);
		return ExecuteAttackCommandAsync(choiceContext, CommonActions.CardAttack((CardModel)(object)this, target, damage, hitCount, vfx, sfx, tmpSfx));
	}

	internal IReadOnlyList<Creature> GetOpponentSnapshot()
	{
		return EnumerateOpponents().ToList();
	}

	protected Task ExecuteAttackAllEnemiesAsync(PlayerChoiceContext choiceContext, int hitCount = 1, string? vfx = null, string? sfx = null, string? tmpSfx = null)
	{
		Player owner = ((CardModel)this).Owner;
		ValencinaAnimation.QueueNextAttackVariant((owner != null) ? owner.Creature : null, hitCount);
		return ExecuteAttackCommandAsync(choiceContext, CommonActions.CardAttackAllOpponents((CardModel)(object)this, hitCount, vfx, sfx, tmpSfx));
	}

	protected Task ExecuteAttackAllEnemiesAsync(PlayerChoiceContext choiceContext, decimal damage, int hitCount = 1, string? vfx = null, string? sfx = null, string? tmpSfx = null)
	{
		Player owner = ((CardModel)this).Owner;
		ValencinaAnimation.QueueNextAttackVariant((owner != null) ? owner.Creature : null, hitCount);
		return ExecuteAttackCommandAsync(choiceContext, CommonActions.CardAttackAllOpponents((CardModel)(object)this, damage, hitCount, vfx, sfx, tmpSfx));
	}

	protected async Task<int> ExecuteAttackAndGetUnblockedDamageAsync(PlayerChoiceContext choiceContext, Creature? target, int hitCount = 1, string? vfx = null, string? sfx = null, string? tmpSfx = null)
	{
		if (target == null)
		{
			return 0;
		}
		AttackCommand command = CommonActions.CardAttack((CardModel)(object)this, target, hitCount, vfx, sfx, tmpSfx);
		Player owner = ((CardModel)this).Owner;
		ValencinaAnimation.QueueNextAttackVariant((owner != null) ? owner.Creature : null, hitCount);
		await ExecuteAttackCommandAsync(choiceContext, command);
		return SumUnblockedDamage(command);
	}

	protected async Task<int> ExecuteAttackAndGetUnblockedDamageAsync(PlayerChoiceContext choiceContext, Creature? target, decimal damage, int hitCount = 1, string? vfx = null, string? sfx = null, string? tmpSfx = null)
	{
		if (target == null)
		{
			return 0;
		}
		AttackCommand command = CommonActions.CardAttack((CardModel)(object)this, target, damage, hitCount, vfx, sfx, tmpSfx);
		Player owner = ((CardModel)this).Owner;
		ValencinaAnimation.QueueNextAttackVariant((owner != null) ? owner.Creature : null, hitCount);
		await ExecuteAttackCommandAsync(choiceContext, command);
		return SumUnblockedDamage(command);
	}

	protected Task ExecuteAttackCommandAsync(PlayerChoiceContext choiceContext, AttackCommand command)
	{
		return command.Execute(choiceContext);
	}

	private static int SumUnblockedDamage(AttackCommand command)
	{
		int num = 0;
		foreach (DamageResult item in CommonActions.DamageResults(command))
		{
			num += item.UnblockedDamage;
		}
		return num;
	}

	protected decimal ReadDynamicVarValue(object? dynamicVar)
	{
		if (dynamicVar == null)
		{
			return 0m;
		}
		string[] array = new string[2] { "CurrentValue", "BaseValue" };
		foreach (string name in array)
		{
			PropertyInfo property = dynamicVar.GetType().GetProperty(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			if (property == null)
			{
				continue;
			}
			try
			{
				object value = property.GetValue(dynamicVar);
				if (value is decimal result)
				{
					return result;
				}
				if (value is int num)
				{
					return num;
				}
				if (value is long num2)
				{
					return num2;
				}
				if (value is float num3)
				{
					return (decimal)num3;
				}
				if (value is double num4)
				{
					return (decimal)num4;
				}
			}
			catch
			{
			}
		}
		return 0m;
	}

	protected decimal ReadDamageValue()
	{
		return ReadDynamicVarValue(((CardModel)this).DynamicVars.Damage);
	}

	protected int CountAmmoSpendersInOwnerDeck()
	{
		if (!((AbstractModel)this).IsMutable)
		{
			return 0;
		}
		int num = 0;
		HashSet<object> hashSet = new HashSet<object>(ReferenceEqualityComparer.Instance);
		try
		{
			foreach (object item in EnumerateOwnerDeckCards())
			{
				if (item != null && hashSet.Add(item) && item is ValencinaCard { SpendsAmmo: not false })
				{
					num++;
				}
			}
			return num;
		}
		catch
		{
			return 0;
		}
	}

	protected int CountCardsPlayedThisTurn()
	{
		if (((CardModel)this).CombatState != null)
		{
			try
			{
				int num = 0;
				foreach (CardPlayStartedEntry item in CombatManager.Instance.History.CardPlaysStarted)
				{
					if (((CombatHistoryEntry)item).HappenedThisTurn(((CardModel)this).CombatState) && item.CardPlay.Card.Owner == ((CardModel)this).Owner)
					{
						num++;
					}
				}
				return num;
			}
			catch
			{
			}
		}
		object[] obj2 = new object[4]
		{
			((CardModel)this).CombatState,
			((CardModel)this).Owner,
			null,
			null
		};
		Player owner = ((CardModel)this).Owner;
		obj2[2] = ((owner != null) ? owner.Creature : null);
		obj2[3] = this;
		object[] array = obj2;
		for (int i = 0; i < array.Length; i++)
		{
			int? num2 = TryReadCollectionCount(array[i], "CardsPlayedThisTurn", "cardsPlayedThisTurn", "PlayedCardsThisTurn", "playedCardsThisTurn", "CardsPlayed", "cardsPlayed");
			if (num2.HasValue)
			{
				return num2.Value;
			}
		}
		return 0;
	}

	protected int GetObservedTurnToken()
	{
		if (((CardModel)this).CombatState == null)
		{
			return 0;
		}
		int num = CountCardsPlayedThisTurn();
		if (!CombatTurnTrackers.TryGetValue(((CardModel)this).CombatState, out TurnTracker value))
		{
			value = new TurnTracker
			{
				Token = 1,
				LastCardsPlayedThisTurn = num
			};
			CombatTurnTrackers[((CardModel)this).CombatState] = value;
			return value.Token;
		}
		if (num < value.LastCardsPlayedThisTurn)
		{
			value.Token++;
		}
		value.LastCardsPlayedThisTurn = num;
		return value.Token;
	}

	protected int CountCardsInOwnerHand()
	{
		object[] obj = new object[3]
		{
			((CardModel)this).Owner,
			null,
			null
		};
		Player owner = ((CardModel)this).Owner;
		obj[1] = ((owner != null) ? owner.Creature : null);
		obj[2] = ((CardModel)this).CombatState;
		object[] array = obj;
		for (int i = 0; i < array.Length; i++)
		{
			int? num = TryReadCollectionCount(array[i], "Hand", "hand", "HandPile", "handPile", "CardsInHand", "cardsInHand");
			if (num.HasValue)
			{
				return num.Value;
			}
		}
		return 0;
	}

	protected int ReadCurrentFloorNumber()
	{
		try
		{
			Player owner = ((CardModel)this).Owner;
			object obj = ((owner != null) ? owner.RunState : null);
			if (obj == null)
			{
				ICombatState combatState = ((CardModel)this).CombatState;
				obj = ((combatState != null) ? combatState.RunState : null);
			}
			IRunState val = (IRunState)obj;
			if (val != null)
			{
				int totalFloor = val.TotalFloor;
				if (totalFloor > 0)
				{
					return totalFloor;
				}
				int actFloor = val.ActFloor;
				if (actFloor > 0)
				{
					return actFloor;
				}
			}
		}
		catch
		{
		}
		try
		{
			Type type = typeof(CardModel).Assembly.GetType("MegaCrit.Sts2.Core.Runs.RunManager");
			if (type == null)
			{
				return 0;
			}
			object obj3 = null;
			string[] array = new string[6] { "Instance", "instance", "Singleton", "singleton", "Main", "main" };
			foreach (string name in array)
			{
				obj3 = type.GetProperty(name, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)?.GetValue(null) ?? obj3;
				if (obj3 != null)
				{
					break;
				}
				obj3 = type.GetField(name, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)?.GetValue(null) ?? obj3;
				if (obj3 != null)
				{
					break;
				}
			}
			if (obj3 != null)
			{
				int? num = TryReadIntMember(obj3, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, "FloorNum", "floorNum", "CurrentFloor", "currentFloor", "TotalFloor", "ActFloor");
				if (num.HasValue)
				{
					return Math.Max(0, num.Value);
				}
			}
			array = new string[6] { "FloorNum", "floorNum", "CurrentFloor", "currentFloor", "TotalFloor", "ActFloor" };
			foreach (string name2 in array)
			{
				PropertyInfo property = type.GetProperty(name2, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
				int? num2 = ((property != null) ? ConvertToInt(property.GetValue(null)) : ((int?)null));
				if (num2.HasValue)
				{
					return Math.Max(0, num2.Value);
				}
				FieldInfo field = type.GetField(name2, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
				int? num3 = ((field != null) ? ConvertToInt(field.GetValue(null)) : ((int?)null));
				if (num3.HasValue)
				{
					return Math.Max(0, num3.Value);
				}
			}
			return 0;
		}
		catch
		{
			return 0;
		}
	}

	protected bool IsCardUpgraded()
	{
		string[] array = new string[4] { "IsUpgraded", "Upgraded", "isUpgraded", "upgraded" };
		foreach (string name in array)
		{
			PropertyInfo property = ((object)this).GetType().GetProperty(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			if (property != null)
			{
				try
				{
					if (property.GetValue(this) is bool result)
					{
						return result;
					}
				}
				catch
				{
				}
			}
			FieldInfo field = ((object)this).GetType().GetField(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			if (!(field != null))
			{
				continue;
			}
			try
			{
				if (field.GetValue(this) is bool result2)
				{
					return result2;
				}
			}
			catch
			{
			}
		}
		return false;
	}

	private static bool TryGetValencinaKeywordLocPrefix(CardKeyword keyword, out string locPrefix)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		if (keyword == ValencinaKeywords.Ammo)
		{
			locPrefix = "VALENCINA-AMMO";
			return true;
		}
		if (keyword == ValencinaKeywords.Instant)
		{
			locPrefix = "VALENCINA-INSTANT";
			return true;
		}
		if (keyword == ValencinaKeywords.Tremor)
		{
			locPrefix = "VALENCINA-TREMOR";
			return true;
		}
		if (keyword == ValencinaKeywords.Burn)
		{
			locPrefix = "VALENCINA-BURN";
			return true;
		}
		if (keyword == ValencinaKeywords.AmplitudeConversion)
		{
			locPrefix = "VALENCINA-AMPLITUDE_CONVERSION";
			return true;
		}
		if (keyword == ValencinaKeywords.TremorDetonation)
		{
			locPrefix = "VALENCINA-TREMOR_DETONATION";
			return true;
		}
		if (keyword == ValencinaKeywords.BreathingMethod)
		{
			locPrefix = "VALENCINA-BREATHING_METHOD";
			return true;
		}
		if (keyword == ValencinaKeywords.OdinEye)
		{
			locPrefix = "VALENCINA-ODIN_EYE";
			return true;
		}
		if (keyword == ValencinaKeywords.TemporaryOdinEye)
		{
			locPrefix = "VALENCINA-TEMPORARY_ODIN_EYE";
			return true;
		}
		if (keyword == ValencinaKeywords.Dodge)
		{
			locPrefix = "VALENCINA-DODGE";
			return true;
		}
		if (keyword == ValencinaKeywords.Gaze)
		{
			locPrefix = "VALENCINA-GAZE";
			return true;
		}
		if (keyword == ValencinaKeywords.Disposal)
		{
			locPrefix = "VALENCINA-DISPOSAL_KEYWORD";
			return true;
		}
		if (keyword == ValencinaKeywords.Unfired)
		{
			locPrefix = "VALENCINA-UNFIRED";
			return true;
		}
		if (keyword == ValencinaKeywords.Counter)
		{
			locPrefix = "VALENCINA-COUNTER";
			return true;
		}
		if (keyword == ValencinaKeywords.Wounding)
		{
			locPrefix = "VALENCINA-WOUNDING";
			return true;
		}
		if (keyword == ValencinaKeywords.Acceleration)
		{
			locPrefix = "VALENCINA-ACCELERATION";
			return true;
		}
		locPrefix = string.Empty;
		return false;
	}

	private IEnumerable EnumerateOwnerDeckCards()
	{
		object[] obj = new object[3]
		{
			((CardModel)this).Owner,
			null,
			null
		};
		Player owner = ((CardModel)this).Owner;
		obj[1] = ((owner != null) ? owner.Creature : null);
		obj[2] = ((CardModel)this).CombatState;
		object?[] array = obj;
		foreach (object source in array)
		{
			foreach (object item in EnumerateCandidateCards(source, 0))
			{
				yield return item;
			}
		}
	}

	private static IEnumerable EnumerateCandidateCards(object? source, int depth)
	{
		if (source == null || depth > 2)
		{
			yield break;
		}
		string[] array = new string[8] { "MasterDeck", "masterDeck", "Deck", "deck", "Cards", "cards", "CardModels", "cardModels" };
		foreach (string name in array)
		{
			object obj = source.GetType().GetProperty(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)?.GetValue(source);
			if (obj is IEnumerable enumerable)
			{
				foreach (object item in enumerable)
				{
					yield return item;
				}
				break;
			}
			if (obj != null)
			{
				foreach (object item2 in EnumerateCandidateCards(obj, depth + 1))
				{
					yield return item2;
				}
			}
			object obj2 = source.GetType().GetField(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)?.GetValue(source);
			if (obj2 is IEnumerable enumerable2)
			{
				foreach (object item3 in enumerable2)
				{
					yield return item3;
				}
				break;
			}
			if (obj2 == null)
			{
				continue;
			}
			foreach (object item4 in EnumerateCandidateCards(obj2, depth + 1))
			{
				yield return item4;
			}
		}
	}

	private static int? TryReadCollectionCount(object? source, params string[] memberNames)
	{
		if (source == null)
		{
			return null;
		}
		foreach (string name in memberNames)
		{
			object obj = source.GetType().GetProperty(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)?.GetValue(source);
			if (obj is ICollection collection)
			{
				return collection.Count;
			}
			if (obj is IEnumerable enumerable)
			{
				int num = 0;
				foreach (object item in enumerable)
				{
					_ = item;
					num++;
				}
				return num;
			}
			object obj2 = source.GetType().GetField(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)?.GetValue(source);
			if (obj2 is ICollection collection2)
			{
				return collection2.Count;
			}
			if (!(obj2 is IEnumerable enumerable2))
			{
				continue;
			}
			int num2 = 0;
			foreach (object item2 in enumerable2)
			{
				_ = item2;
				num2++;
			}
			return num2;
		}
		return null;
	}

	protected TPower? FindPowerOn<TPower>(Creature? creature) where TPower : class
	{
		if (creature == null)
		{
			return null;
		}
		foreach (object item in EnumeratePowersOn(creature))
		{
			if (item is TPower result)
			{
				return result;
			}
		}
		return null;
	}

	protected async Task GainTemporaryDodgeThreshold(decimal amount, CardPlay? cardPlay = null)
	{
		Player owner = ((CardModel)this).Owner;
		object obj;
		if (owner == null)
		{
			obj = null;
		}
		else
		{
			Creature creature = owner.Creature;
			obj = ((creature != null) ? creature.GetPower<NoDodgeGainPower>() : null);
		}
		if (obj == null)
		{
			decimal modified = amount;
			Player owner2 = ((CardModel)this).Owner;
			if (((owner2 != null) ? owner2.Creature : null) != null && ((CardModel)this).CombatState != null)
			{
				IEnumerable<AbstractModel> enumerable = default(IEnumerable<AbstractModel>);
				modified = Hook.ModifyBlock(((CardModel)this).CombatState, ((CardModel)this).Owner.Creature, amount, (ValueProp)8, (CardModel)(object)this, cardPlay, ref enumerable);
				modified = Math.Max(0m, modified);
				await Hook.AfterModifyingBlockAmount(((CardModel)this).CombatState, modified, (CardModel)(object)this, cardPlay, enumerable);
			}
			int num = GrantTemporaryDodgeThresholdRaw(modified);
			await NotifyDodgeBlockGainedAsync(num);
		}
	}

	private int GrantTemporaryDodgeThresholdRaw(decimal amount)
	{
		if (!(amount <= 0m))
		{
			Player owner = ((CardModel)this).Owner;
			object obj;
			if (owner == null)
			{
				obj = null;
			}
			else
			{
				Creature creature = owner.Creature;
				obj = ((creature != null) ? creature.GetPower<NoDodgeGainPower>() : null);
			}
			if (obj == null)
			{
				int num = (int)Math.Floor(amount);
				if (num <= 0)
				{
					return 0;
				}
				Player owner2 = ((CardModel)this).Owner;
				object obj2;
				if (owner2 == null)
				{
					obj2 = null;
				}
				else
				{
					Creature creature2 = owner2.Creature;
					obj2 = ((creature2 != null) ? creature2.GetPower<InstantForesightPower>() : null);
				}
				((InstantForesightPower)obj2)?.GainTemporaryDodgeThreshold(num);
				return num;
			}
		}
		return 0;
	}

	protected async Task GainTemporaryDodgeThreshold(BlockVar blockEquivalent, CardPlay? cardPlay = null)
	{
		Player owner = ((CardModel)this).Owner;
		if (((owner != null) ? owner.Creature : null) == null || ((CardModel)this).CombatState == null)
		{
			await GainTemporaryDodgeThreshold(((DynamicVar)blockEquivalent).BaseValue);
			return;
		}
		decimal baseValue = ((DynamicVar)blockEquivalent).BaseValue;
		IEnumerable<AbstractModel> enumerable = default(IEnumerable<AbstractModel>);
		decimal modifiedBlock = Hook.ModifyBlock(((CardModel)this).CombatState, ((CardModel)this).Owner.Creature, baseValue, blockEquivalent.Props, (CardModel)(object)this, cardPlay, ref enumerable);
		modifiedBlock = Math.Max(0m, modifiedBlock);
		await Hook.AfterModifyingBlockAmount(((CardModel)this).CombatState, modifiedBlock, (CardModel)(object)this, cardPlay, enumerable);
		int num = GrantTemporaryDodgeThresholdRaw(modifiedBlock);
		await NotifyDodgeBlockGainedAsync(num);
	}

	protected async Task GainTemporaryDodgeThreshold(BlockVar blockEquivalent, decimal multiplier, CardPlay? cardPlay = null)
	{
		if (!(multiplier <= 0m))
		{
			Player owner = ((CardModel)this).Owner;
			if (((owner != null) ? owner.Creature : null) == null || ((CardModel)this).CombatState == null)
			{
				await GainTemporaryDodgeThreshold(((DynamicVar)blockEquivalent).BaseValue * multiplier);
				return;
			}
			IEnumerable<AbstractModel> enumerable = default(IEnumerable<AbstractModel>);
			decimal modifiedBlock = Hook.ModifyBlock(((CardModel)this).CombatState, ((CardModel)this).Owner.Creature, ((DynamicVar)blockEquivalent).BaseValue, blockEquivalent.Props, (CardModel)(object)this, cardPlay, ref enumerable);
			modifiedBlock = Math.Max(0m, modifiedBlock * multiplier);
			await Hook.AfterModifyingBlockAmount(((CardModel)this).CombatState, modifiedBlock, (CardModel)(object)this, cardPlay, enumerable);
			int num = GrantTemporaryDodgeThresholdRaw(modifiedBlock);
			await NotifyDodgeBlockGainedAsync(num);
		}
	}

	private async Task NotifyDodgeBlockGainedAsync(decimal amount)
	{
		if (!(amount <= 0m))
		{
			Player owner = ((CardModel)this).Owner;
			if (((owner != null) ? owner.Creature : null) != null && ((CardModel)this).CombatState != null)
			{
				await Hook.AfterBlockGained(((CardModel)this).CombatState, ((CardModel)this).Owner.Creature, amount, (ValueProp)8, (CardModel)(object)this);
			}
		}
	}

	protected Task GainTemporaryDodgeThreshold(DynamicVar amount, CardPlay? cardPlay = null)
	{
		BlockVar val = (BlockVar)(object)((amount is BlockVar) ? amount : null);
		if (val == null)
		{
			return GainTemporaryDodgeThreshold(amount.BaseValue, cardPlay);
		}
		return GainTemporaryDodgeThreshold(val, cardPlay);
	}

	protected async Task GainTemporaryPrecognition(decimal amount)
	{
		Player owner = ((CardModel)this).Owner;
		object obj;
		if (owner == null)
		{
			obj = null;
		}
		else
		{
			Creature creature = owner.Creature;
			obj = ((creature != null) ? creature.GetPower<InstantForesightPower>() : null);
		}
		InstantForesightPower instantForesightPower = (InstantForesightPower)obj;
		if (instantForesightPower != null && instantForesightPower.IsOverheated)
		{
			await instantForesightPower.GainTemporaryPrecognitionAsync((int)Math.Floor(amount), (CardModel?)(object)this);
		}
	}

	protected Task GainTemporaryPrecognition(DynamicVar amount)
	{
		return GainTemporaryPrecognition(amount.BaseValue);
	}

	protected Task GainPrecognition(decimal amount)
	{
		Player owner = ((CardModel)this).Owner;
		object obj;
		if (owner == null)
		{
			obj = null;
		}
		else
		{
			Creature creature = owner.Creature;
			obj = ((creature != null) ? creature.GetPower<InstantForesightPower>() : null);
		}
		((InstantForesightPower)obj)?.GainPrecognition((int)Math.Ceiling(amount));
		return Task.CompletedTask;
	}

	protected Task GainPrecognition(DynamicVar amount)
	{
		return GainPrecognition(amount.BaseValue);
	}

	protected int CountDebuffPowers(Creature? creature)
	{
		if (creature == null)
		{
			return 0;
		}
		int num = 0;
		foreach (object item in EnumeratePowersOn(creature))
		{
			if (IsDebuffPowerForCurrentAmount(item))
			{
				num++;
			}
		}
		return num;
	}

	protected static bool IsDebuffPowerForCurrentAmount(object? power)
	{
		if (power == null)
		{
			return false;
		}
		Type type = power.GetType();
		object obj = type.GetProperty("TypeForCurrentAmount", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)?.GetValue(power);
		if (obj == null)
		{
			decimal num = ReadPowerAmount(power);
			obj = type.GetMethod("GetTypeForAmount", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new Type[1] { typeof(decimal) }, null)?.Invoke(power, new object[1] { num });
		}
		if (obj == null)
		{
			obj = type.GetProperty("Type", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)?.GetValue(power) ?? type.GetField("Type", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)?.GetValue(power);
		}
		return string.Equals(obj?.ToString(), "Debuff", StringComparison.OrdinalIgnoreCase);
	}

	protected static decimal ReadPowerAmount(object target)
	{
		return ReadDecimalMember(target, "Amount", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
	}

	private static decimal ReadDecimalMember(object target, string memberName, BindingFlags flags)
	{
		object obj = target.GetType().GetProperty(memberName, flags)?.GetValue(target) ?? target.GetType().GetField(memberName, flags)?.GetValue(target);
		if (!(obj is int num))
		{
			if (!(obj is long num2))
			{
				if (!(obj is float num3))
				{
					if (!(obj is double num4))
					{
						if (obj is decimal result)
						{
							return result;
						}
						return 0m;
					}
					return (decimal)num4;
				}
				return (decimal)num3;
			}
			return num2;
		}
		return num;
	}

	protected IEnumerable<Creature> EnumerateOpponents()
	{
		if (((CardModel)this).CombatState == null)
		{
			yield break;
		}
		Player owner = ((CardModel)this).Owner;
		if (((owner != null) ? owner.Creature : null) == null)
		{
			yield break;
		}
		string[] array = new string[6] { "Creatures", "creatures", "AllCreatures", "allCreatures", "CombatCreatures", "combatCreatures" };
		foreach (string name in array)
		{
			if (((object)((CardModel)this).CombatState).GetType().GetProperty(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)?.GetValue(((CardModel)this).CombatState) is IEnumerable enumerable)
			{
				foreach (object item in enumerable)
				{
					Creature val = (Creature)((item is Creature) ? item : null);
					if (val != null && val != ((CardModel)this).Owner.Creature && val.Side != ((CardModel)this).Owner.Creature.Side && val.CurrentHp > 0)
					{
						yield return val;
					}
				}
				break;
			}
			if (!(((object)((CardModel)this).CombatState).GetType().GetField(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)?.GetValue(((CardModel)this).CombatState) is IEnumerable enumerable2))
			{
				continue;
			}
			foreach (object item2 in enumerable2)
			{
				Creature val2 = (Creature)((item2 is Creature) ? item2 : null);
				if (val2 != null && val2 != ((CardModel)this).Owner.Creature && val2.Side != ((CardModel)this).Owner.Creature.Side && val2.CurrentHp > 0)
				{
					yield return val2;
				}
			}
			break;
		}
	}

	protected static IEnumerable<object?> EnumeratePowersOn(Creature creature)
	{
		string[] array = new string[4] { "Powers", "powers", "ActivePowers", "_powers" };
		foreach (string name in array)
		{
			if (((object)creature).GetType().GetProperty(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)?.GetValue(creature) is IEnumerable enumerable)
			{
				foreach (object item in enumerable)
				{
					yield return item;
				}
				break;
			}
			if (!(((object)creature).GetType().GetField(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)?.GetValue(creature) is IEnumerable enumerable2))
			{
				continue;
			}
			foreach (object item2 in enumerable2)
			{
				yield return item2;
			}
			break;
		}
	}

	protected void TryEnableRetain(bool value = true)
	{
		TrySetBooleanFlag(this, value, "Retain", "IsRetain", "RetainThisTurn", "IsRetainedThisTurn", "ShouldRetainThisTurn", "HasSingleTurnRetain");
	}

	protected void TryEnableInnate(bool value = true)
	{
		TrySetBooleanFlag(this, value, "Innate", "IsInnate", "AppliedInnate");
	}

	protected static void TrySetBooleanFlag(object target, bool value, params string[] memberNames)
	{
		foreach (string name in memberNames)
		{
			PropertyInfo property = target.GetType().GetProperty(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			if ((object)property != null && property.CanWrite && (property.PropertyType == typeof(bool) || property.PropertyType == typeof(bool?)))
			{
				try
				{
					property.SetValue(target, value);
					break;
				}
				catch
				{
				}
			}
			FieldInfo field = target.GetType().GetField(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			if (field != null && (field.FieldType == typeof(bool) || field.FieldType == typeof(bool?)))
			{
				try
				{
					field.SetValue(target, value);
					break;
				}
				catch
				{
				}
			}
		}
	}

	protected static bool TrySetNumericAmount(object target, int amount)
	{
		Type type = target.GetType();
		string[] array = new string[2] { "SetStacks", "SetAmount" };
		foreach (string b in array)
		{
			MethodInfo[] methods = type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			foreach (MethodInfo methodInfo in methods)
			{
				if (!string.Equals(methodInfo.Name, b, StringComparison.Ordinal))
				{
					continue;
				}
				ParameterInfo[] parameters = methodInfo.GetParameters();
				try
				{
					if (parameters.Length == 1)
					{
						object obj = ConvertNumeric(amount, parameters[0].ParameterType);
						if (obj != null)
						{
							methodInfo.Invoke(target, new object[1] { obj });
							TryInvokeNoArg(target, "InitInternalData");
							TryInvokeNoArg(target, "UpdateDescription");
							TryInvokeNoArg(target, "InvokeDisplayAmountChanged");
							return true;
						}
					}
					else if (parameters.Length == 2 && parameters[1].ParameterType == typeof(bool))
					{
						object obj2 = ConvertNumeric(amount, parameters[0].ParameterType);
						if (obj2 != null)
						{
							methodInfo.Invoke(target, new object[2] { obj2, false });
							TryInvokeNoArg(target, "InitInternalData");
							TryInvokeNoArg(target, "UpdateDescription");
							TryInvokeNoArg(target, "InvokeDisplayAmountChanged");
							return true;
						}
					}
				}
				catch
				{
				}
			}
		}
		array = new string[2] { "Amount", "amount" };
		foreach (string name in array)
		{
			PropertyInfo property = type.GetProperty(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			if ((object)property != null && property.CanWrite)
			{
				try
				{
					object obj4 = ConvertNumeric(amount, property.PropertyType);
					if (obj4 != null)
					{
						property.SetValue(target, obj4);
						TryInvokeNoArg(target, "InitInternalData");
						TryInvokeNoArg(target, "UpdateDescription");
						TryInvokeNoArg(target, "InvokeDisplayAmountChanged");
						return true;
					}
				}
				catch
				{
				}
			}
			FieldInfo field = type.GetField(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			if (!(field != null))
			{
				continue;
			}
			try
			{
				object obj6 = ConvertNumeric(amount, field.FieldType);
				if (obj6 != null)
				{
					field.SetValue(target, obj6);
					TryInvokeNoArg(target, "InitInternalData");
					TryInvokeNoArg(target, "UpdateDescription");
					TryInvokeNoArg(target, "InvokeDisplayAmountChanged");
					return true;
				}
			}
			catch
			{
			}
		}
		return false;
	}

	protected static void TryInvokeNoArg(object target, string methodName)
	{
		try
		{
			target.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Type.EmptyTypes, null)?.Invoke(target, null);
		}
		catch
		{
		}
	}

	private static int? TryReadIntMember(object source, BindingFlags flags, params string[] memberNames)
	{
		foreach (string name in memberNames)
		{
			PropertyInfo property = source.GetType().GetProperty(name, flags);
			if (property != null)
			{
				try
				{
					int? result = ConvertToInt(property.GetValue(source));
					if (result.HasValue)
					{
						return result;
					}
				}
				catch
				{
				}
			}
			FieldInfo field = source.GetType().GetField(name, flags);
			if (!(field != null))
			{
				continue;
			}
			try
			{
				int? result2 = ConvertToInt(field.GetValue(source));
				if (result2.HasValue)
				{
					return result2;
				}
			}
			catch
			{
			}
		}
		return null;
	}

	private static int? ConvertToInt(object? value)
	{
		if (!(value is int value2))
		{
			if (!(value is long num))
			{
				if (!(value is short value3))
				{
					if (!(value is byte value4))
					{
						if (!(value is float num2))
						{
							if (!(value is double num3))
							{
								if (value is decimal num4)
								{
									return (int)num4;
								}
								return null;
							}
							return (int)num3;
						}
						return (int)num2;
					}
					return value4;
				}
				return value3;
			}
			return (int)num;
		}
		return value2;
	}

	private static object? ConvertNumeric(int value, Type targetType)
	{
		if (targetType == typeof(int) || targetType == typeof(int?))
		{
			return value;
		}
		if (targetType == typeof(long) || targetType == typeof(long?))
		{
			return (long)value;
		}
		if (targetType == typeof(float) || targetType == typeof(float?))
		{
			return (float)value;
		}
		if (targetType == typeof(double) || targetType == typeof(double?))
		{
			return (double)value;
		}
		if (targetType == typeof(decimal) || targetType == typeof(decimal?))
		{
			return (decimal)value;
		}
		if (targetType == typeof(short) || targetType == typeof(short?))
		{
			return (short)value;
		}
		if (targetType == typeof(byte) || targetType == typeof(byte?))
		{
			return (byte)Math.Max(0, Math.Min(255, value));
		}
		return null;
	}

	[CompilerGenerated]
	[DebuggerHidden]
	private IEnumerable<CardKeyword> _003C_003En__0()
	{
		return ((CardModel)this).CanonicalKeywords;
	}
}
