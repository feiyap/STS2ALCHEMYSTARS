using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using Valencina.ValencinaCode.Cards;
using Valencina.ValencinaCode.Extensions;

namespace Valencina.ValencinaCode.Precognition;

public abstract class ValencinaCounterPreviewCard(int level) : ValencinaCard(0, (CardType)1, (CardRarity)5, (TargetType)2, showInCardLibrary: false)
{
	private readonly int _level = level;

	private ValencinaCounterDefinition Definition => ValencinaCounterLevelHelper.GetDefinition(level);

	public override bool CanBeGeneratedInCombat => false;

	public override bool SpendsAmmo => false;

	public override int AmmoSpendPreviewAmount => Definition.AmmoCost;

	public override string CustomPortraitPath => PortraitName.BigCardImagePath();

	public override string PortraitPath => PortraitName.CardImagePath();

	public override string BetaPortraitPath => ("beta/" + PortraitName).CardImagePath();

	private string PortraitName => "basic_counter.png";

	public override IEnumerable<CardKeyword> CanonicalKeywords
	{
		get
		{
			foreach (CardKeyword canonicalKeyword in base.CanonicalKeywords)
			{
				yield return canonicalKeyword;
			}
			yield return ValencinaKeywords.Counter;
		}
	}

	protected override IEnumerable<DynamicVar> CanonicalVars => new _003C_003Ez__ReadOnlyArray<DynamicVar>((DynamicVar[])(object)new DynamicVar[5]
	{
		(DynamicVar)new DamageVar(Definition.Damage, (ValueProp)12),
		new DynamicVar("Amount", (decimal)Definition.AmmoCost),
		new DynamicVar("Hits", (decimal)Definition.BaseHitCount),
		new DynamicVar("Future", 1m),
		new DynamicVar("Cards", 1m)
	});

	protected override Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
	{
		return Task.CompletedTask;
	}
}
