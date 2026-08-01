using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Combat.History;
using MegaCrit.Sts2.Core.Combat.History.Entries;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using Valencina.ValencinaCode.Compat;
using Valencina.ValencinaCode.Powers;
using Valencina.ValencinaCode.Systems;

namespace Valencina.ValencinaCode.Cards;

public sealed class InfiniteReload : ValencinaCard
{
	protected override IEnumerable<DynamicVar> CanonicalVars => new _003C_003Ez__ReadOnlySingleElementList<DynamicVar>((DynamicVar)new DamageVar(7m, (ValueProp)8));

	public InfiniteReload()
		: base(1, (CardType)1, (CardRarity)4, (TargetType)2)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
	{
		int hits = Math.Max(1, CountAttackCardsPlayedThisTurnBeforeThis() + 1);
		await ExecuteAttackAsync(choiceContext, play.Target, hits, "vfx/vfx_attack_slash");
		int num = await AmmoSystem.AddAmmoAsync(((CardModel)this).Owner.Creature, hits, (CardModel?)(object)this, choiceContext);
		if (num > 0)
		{
			await CommonActions.ApplySelf<BreathingMethodPower>(choiceContext, (CardModel)(object)this, (decimal)num, silent: false);
		}
	}

	protected override void OnUpgrade()
	{
		((DynamicVar)((CardModel)this).DynamicVars.Damage).UpgradeValueBy(2m);
	}

	private int CountAttackCardsPlayedThisTurnBeforeThis()
	{
		if (((CardModel)this).CombatState == null || ((CardModel)this).Owner == null)
		{
			return 0;
		}
		return CombatManager.Instance.History.CardPlaysFinished.Count((CardPlayFinishedEntry entry) => ((CombatHistoryEntry)entry).HappenedThisTurn(((CardModel)this).CombatState) && entry.CardPlay.Card.Owner == ((CardModel)this).Owner && (int)entry.CardPlay.Card.Type == 1);
	}
}
