using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Combat.History;
using MegaCrit.Sts2.Core.Combat.History.Entries;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace Valencina.ValencinaCode.Cards;

public sealed class GunExecution : ValencinaPlaceholderCard
{
	protected override IEnumerable<DynamicVar> CanonicalVars => new _003C_003Ez__ReadOnlySingleElementList<DynamicVar>((DynamicVar)new DamageVar(20m, (ValueProp)8));

	public GunExecution()
		: base(3, (CardType)1, (CardRarity)3, (TargetType)2, showInCardLibrary: false, autoAdd: false)
	{
	}

	public override bool TryModifyEnergyCostInCombat(CardModel card, decimal originalCost, out decimal modifiedCost)
	{
		if ((object)card == this)
		{
			modifiedCost = Math.Max(0m, originalCost - (decimal)CountAmmoSpendingCardsPlayedThisTurnBeforeThis());
			return modifiedCost != originalCost;
		}
		return ((AbstractModel)this).TryModifyEnergyCostInCombat(card, originalCost, ref modifiedCost);
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
	{
		await ExecuteAttackAsync(choiceContext, play, 1, "vfx/vfx_attack_slash");
	}

	protected override void OnUpgrade()
	{
		((DynamicVar)((CardModel)this).DynamicVars.Damage).UpgradeValueBy(5m);
	}

	private int CountAmmoSpendingCardsPlayedThisTurnBeforeThis()
	{
		Player owner = ((CardModel)this).Owner;
		if (((CardModel)this).CombatState == null)
		{
			return 0;
		}
		return CombatManager.Instance.History.CardPlaysFinished.Count((CardPlayFinishedEntry entry) => ((CombatHistoryEntry)entry).HappenedThisTurn(((CardModel)this).CombatState) && entry.CardPlay.Card.Owner == owner && entry.CardPlay.Card is ValencinaCard valencinaCard && valencinaCard.SpendsAmmo);
	}
}
