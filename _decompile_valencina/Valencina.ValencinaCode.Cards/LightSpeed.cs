using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using Valencina.ValencinaCode.Compat;
using Valencina.ValencinaCode.Powers;

namespace Valencina.ValencinaCode.Cards;

public sealed class LightSpeed : ValencinaCard
{
	protected override IEnumerable<DynamicVar> CanonicalVars => new _003C_003Ez__ReadOnlySingleElementList<DynamicVar>(new DynamicVar("Amount", 6m));

	protected override bool IsPlayable => CountCardsPlayedThisTurn() >= (int)((CardModel)this).DynamicVars["Amount"].BaseValue;

	protected override bool ShouldGlowGoldInternal => CountCardsPlayedThisTurn() >= (int)((CardModel)this).DynamicVars["Amount"].BaseValue;

	public override IEnumerable<CardKeyword> CanonicalKeywords
	{
		get
		{
			foreach (CardKeyword canonicalKeyword in base.CanonicalKeywords)
			{
				yield return canonicalKeyword;
			}
			yield return (CardKeyword)5;
			yield return (CardKeyword)1;
		}
	}

	public LightSpeed()
		: base(0, (CardType)2, (CardRarity)4, (TargetType)1)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
	{
		await CommonActions.ApplySelf<LightSpeedExtraTurnPower>(choiceContext, (CardModel)(object)this, 1m, silent: true);
		PlayerCmd.EndTurn(((CardModel)this).Owner, false, (Func<Task>)null);
	}

	protected override void OnUpgrade()
	{
		((CardModel)this).DynamicVars["Amount"].UpgradeValueBy(-1m);
	}
}
