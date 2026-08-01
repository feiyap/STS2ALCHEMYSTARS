using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using Valencina.ValencinaCode.Powers;

namespace Valencina.ValencinaCode.Cards;

public sealed class Hatred : ValencinaCard
{
	protected override IEnumerable<DynamicVar> CanonicalVars => new _003C_003Ez__ReadOnlyArray<DynamicVar>((DynamicVar[])(object)new DynamicVar[2]
	{
		(DynamicVar)new CardsVar(1),
		new DynamicVar("IfUpgraded", 0m)
	});

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

	protected override IEnumerable<CardKeyword> TooltipKeywords
	{
		get
		{
			yield return (CardKeyword)2;
		}
	}

	public Hatred()
		: base(1, (CardType)2, (CardRarity)3, (TargetType)1, showInCardLibrary: false, autoAdd: false)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
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
			obj = ((creature != null) ? creature.CombatState : null);
		}
		if (obj == null || ((CardModel)this).Owner == null)
		{
			return;
		}
		LieInWaitPower power = ((CardModel)this).Owner.Creature.GetPower<LieInWaitPower>();
		bool retainDisposal = power != null && ((PowerModel)power).Amount > 0;
		for (int i = 0; i < ((DynamicVar)((CardModel)this).DynamicVars.Cards).IntValue; i++)
		{
			HatredFutureDisposal hatredFutureDisposal = DisposalAttackHelper.Configure(((CardModel)this).Owner.Creature.CombatState.CreateCard<HatredFutureDisposal>(((CardModel)this).Owner), 0, DisposalGenerationEnhancement.None, retainDisposal);
			if (IsCardUpgraded() && ((CardModel)hatredFutureDisposal).IsUpgradable)
			{
				CardCmd.Upgrade((CardModel)(object)hatredFutureDisposal, (CardPreviewStyle)1);
			}
			await CardPileCmd.AddGeneratedCardToCombat((CardModel)(object)hatredFutureDisposal, (PileType)2, ((CardModel)this).Owner, (CardPilePosition)2);
		}
	}

	protected override void OnUpgrade()
	{
		((CardModel)this).DynamicVars["IfUpgraded"].UpgradeValueBy(1m);
	}
}
