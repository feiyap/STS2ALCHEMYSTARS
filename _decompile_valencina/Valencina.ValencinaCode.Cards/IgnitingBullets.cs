using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using Valencina.ValencinaCode.Systems;

namespace Valencina.ValencinaCode.Cards;

public sealed class IgnitingBullets : ValencinaCard, IBurnApplyingCard
{
	public override bool SpendsAmmo => true;

	public override string AmmoSpendPreviewText => "X";

	public override IEnumerable<CardKeyword> CanonicalKeywords
	{
		get
		{
			foreach (CardKeyword canonicalKeyword in base.CanonicalKeywords)
			{
				yield return canonicalKeyword;
			}
			yield return (CardKeyword)1;
		}
	}

	protected override IEnumerable<DynamicVar> CanonicalVars => new _003C_003Ez__ReadOnlySingleElementList<DynamicVar>(new DynamicVar("Amount", 3m));

	public IgnitingBullets()
		: base(2, (CardType)2, (CardRarity)3, (TargetType)3, showInCardLibrary: false, autoAdd: false)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
	{
		int num = AmmoSystem.CurrentAmmo(((CardModel)this).Owner.Creature);
		if (num > 0)
		{
			await AmmoSystem.TryConsumeAsync(((CardModel)this).Owner.Creature, num, (CardModel?)(object)this, grantBreathingMethod: true, choiceContext);
		}
		int burn = Math.Max(0, AmmoSystem.MaxAmmoFor(((CardModel)this).Owner.Creature)) * (int)((CardModel)this).DynamicVars["Amount"].BaseValue;
		foreach (Creature item in EnumerateOpponents())
		{
			await StatusSystem.ApplyBurnAsync(item, burn, (CardModel?)(object)this, choiceContext);
		}
	}

	protected override void OnUpgrade()
	{
		((CardModel)this).DynamicVars["Amount"].UpgradeValueBy(1m);
	}
}
