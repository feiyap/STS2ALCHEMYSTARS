using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace Valencina.ValencinaCode.Cards;

public abstract class KaiserPhaseChoiceCard : ValencinaCard
{
	public override bool CanBeGeneratedInCombat => false;

	public override string CustomPortraitPath => "res://Valencina/images/card_portraits/big/aim_for_the_heart.png";

	public override string PortraitPath => "res://Valencina/images/card_portraits/aim_for_the_heart.png";

	protected override IEnumerable<DynamicVar> CanonicalVars => Array.Empty<DynamicVar>();

	protected KaiserPhaseChoiceCard()
		: base(0, (CardType)2, (CardRarity)6, (TargetType)0, showInCardLibrary: false, autoAdd: false)
	{
	}

	protected override Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
	{
		return Task.CompletedTask;
	}
}
