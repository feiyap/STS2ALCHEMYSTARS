using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using Valencina.ValencinaCode.Extensions;
using Valencina.ValencinaCode.Relics.Rien;

namespace Valencina.ValencinaCode.Cards;

public abstract class GeneratedDisposalCard : ValencinaCard, IDisposalAttackCard
{
	private const int BaseDamage = 3;

	private const int BaseHits = 4;

	private const int BaseDetonations = 1;

	public int Insight { get; set; }

	public int ExtraHits { get; set; }

	public int ExtraTremorDetonations { get; set; }

	public bool ForceZeroCost { get; set; }

	public bool ForceUpgrade { get; set; }

	public bool ForceRetain { get; set; }

	public int HitCount => 4 + Math.Max(0, ExtraHits);

	public int TremorDetonationCount => 1 + Math.Max(0, ExtraTremorDetonations);

	public decimal InsightPercent => DisposalAttackHelper.GetInsightDamageBonusPercent(this);

	public override bool CanBeGeneratedInCombat => false;

	public override string CustomPortraitPath => "disposal.png".BigCardImagePath();

	public override string PortraitPath => "disposal.png".CardImagePath();

	public override string BetaPortraitPath => "beta/disposal.png".CardImagePath();

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
			bool oraclePreserved = IsOraclePreserved;
			CardKeyword[] array = (CardKeyword[])(object)((!_003Cethereal_003EP || ForceRetain || oraclePreserved) ? ((!oraclePreserved) ? new CardKeyword[5]
			{
				(CardKeyword)1,
				(CardKeyword)(int)ValencinaKeywords.AmplitudeConversion,
				(CardKeyword)(int)ValencinaKeywords.TremorDetonation,
				(CardKeyword)(int)ValencinaKeywords.Ammo,
				(CardKeyword)(int)ValencinaKeywords.Gaze
			} : new CardKeyword[4]
			{
				(CardKeyword)(int)ValencinaKeywords.AmplitudeConversion,
				(CardKeyword)(int)ValencinaKeywords.TremorDetonation,
				(CardKeyword)(int)ValencinaKeywords.Ammo,
				(CardKeyword)(int)ValencinaKeywords.Gaze
			}) : new CardKeyword[6]
			{
				(CardKeyword)2,
				(CardKeyword)1,
				(CardKeyword)(int)ValencinaKeywords.AmplitudeConversion,
				(CardKeyword)(int)ValencinaKeywords.TremorDetonation,
				(CardKeyword)(int)ValencinaKeywords.Ammo,
				(CardKeyword)(int)ValencinaKeywords.Gaze
			});
			CardKeyword[] array2 = array;
			foreach (CardKeyword val in array2)
			{
				if (emitted.Add(val))
				{
					yield return val;
				}
			}
			if ((ForceRetain || oraclePreserved) && emitted.Add((CardKeyword)5))
			{
				yield return (CardKeyword)5;
			}
		}
	}

	protected override IEnumerable<DynamicVar> CanonicalVars => new _003C_003Ez__ReadOnlyArray<DynamicVar>((DynamicVar[])(object)new DynamicVar[6]
	{
		(DynamicVar)new DamageVar(3m, (ValueProp)8),
		new DynamicVar("Hits", 4m),
		new DynamicVar("Amount", 1m),
		new DynamicVar("Insight", 0m),
		new DynamicVar("InsightPercent", 0m),
		new DynamicVar("InsightPercentPerPrecognition", 0.5m)
	});

	public bool IsOraclePreserved
	{
		get
		{
			try
			{
				Player owner = ((CardModel)this).Owner;
				return ((owner != null) ? owner.GetRelic<ScatteredOracle>() : null) != null;
			}
			catch
			{
				return false;
			}
		}
	}

	protected GeneratedDisposalCard(bool ethereal, bool autoAdd = true)
	{
		_003Cethereal_003EP = ethereal;
		base._002Ector(1, (CardType)1, (CardRarity)5, (TargetType)2, showInCardLibrary: false, autoAdd);
	}

	public void ConfigureDisposal(int insight, int extraHits = 0, int extraTremorDetonations = 0, bool forceZeroCost = false, bool forceRetain = false, bool forceUpgrade = false)
	{
		Insight = Math.Max(0, insight);
		ExtraHits = Math.Max(0, extraHits);
		ExtraTremorDetonations = Math.Max(0, extraTremorDetonations);
		ForceZeroCost = forceZeroCost;
		ForceUpgrade = forceUpgrade;
		ForceRetain = forceRetain || IsOraclePreserved;
		((CardModel)this).DynamicVars["Hits"].BaseValue = HitCount;
		((CardModel)this).DynamicVars["Amount"].BaseValue = TremorDetonationCount;
		((CardModel)this).DynamicVars["Insight"].BaseValue = Insight;
		((CardModel)this).DynamicVars["InsightPercent"].BaseValue = InsightPercent;
		((CardModel)this).DynamicVars["InsightPercentPerPrecognition"].BaseValue = 0.5m;
		if (ForceZeroCost)
		{
			((CardModel)this).EnergyCost.SetCustomBaseCost(0);
		}
		if (ForceRetain)
		{
			((CardModel)this).RemoveKeyword((CardKeyword)2);
			((CardModel)this).AddKeyword((CardKeyword)5);
			TryEnableRetain();
		}
		if (IsOraclePreserved)
		{
			((CardModel)this).RemoveKeyword((CardKeyword)1);
		}
	}

	public override (PileType, CardPilePosition) ModifyCardPlayResultPileTypeAndPosition(CardModel card, bool isAutoPlay, ResourceInfo resources, PileType pileType, CardPilePosition position)
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		if ((object)card == this)
		{
			return ((PileType)((!IsOraclePreserved) ? 4 : ((int)pileType)), position);
		}
		return (pileType, position);
	}

	public override decimal ModifyDamageMultiplicative(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		if ((object)cardSource != this || !ValuePropExtensions.IsPoweredAttack(props))
		{
			return 1m;
		}
		return 1m + DisposalAttackHelper.GetDamageBonusPercent((CardModel)(object)this, target, props) / 100m;
	}

	protected override Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
	{
		return DisposalAttackHelper.PlayAsync(this, choiceContext, play.Target);
	}

	internal Task ExecuteDisposalAttackCommandAsync(PlayerChoiceContext choiceContext, AttackCommand command)
	{
		return ExecuteAttackCommandAsync(choiceContext, command);
	}

	protected override void OnUpgrade()
	{
		((CardModel)this).EnergyCost.UpgradeBy(-1);
	}
}
