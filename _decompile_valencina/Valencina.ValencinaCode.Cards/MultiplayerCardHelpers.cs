using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using Valencina.ValencinaCode.Powers;

namespace Valencina.ValencinaCode.Cards;

internal static class MultiplayerCardHelpers
{
	internal static async Task GainDodgeAsync(PlayerChoiceContext choiceContext, Creature? creature, int amount, CardPlay? cardPlay)
	{
		if (creature != null && amount > 0 && creature.GetPower<NoDodgeGainPower>() == null)
		{
			InstantForesightPower power = creature.GetPower<InstantForesightPower>();
			if (power != null)
			{
				power.GainTemporaryDodgeThreshold(amount);
				await Task.CompletedTask;
			}
		}
	}

	internal static async Task GainDodgeAsync(PlayerChoiceContext choiceContext, Creature? creature, BlockVar amount, CardPlay? cardPlay)
	{
		if (creature == null || creature.GetPower<NoDodgeGainPower>() != null)
		{
			return;
		}
		InstantForesightPower power = creature.GetPower<InstantForesightPower>();
		if (power == null)
		{
			return;
		}
		decimal modified = ((DynamicVar)amount).BaseValue;
		CardModel sourceCard = ((cardPlay != null) ? cardPlay.Card : null);
		if (((sourceCard != null) ? sourceCard.CombatState : null) != null && ValuePropExtensions.IsPoweredCardOrMonsterMoveBlock(amount.Props))
		{
			IEnumerable<AbstractModel> enumerable = default(IEnumerable<AbstractModel>);
			modified = Hook.ModifyBlock(sourceCard.CombatState, creature, ((DynamicVar)amount).BaseValue, amount.Props, sourceCard, cardPlay, ref enumerable);
			modified = Math.Max(0m, modified);
			await Hook.AfterModifyingBlockAmount(sourceCard.CombatState, modified, sourceCard, cardPlay, enumerable);
		}
		int num = (int)Math.Floor(modified);
		if (num > 0)
		{
			power.GainTemporaryDodgeThreshold(num);
			if (((sourceCard != null) ? sourceCard.CombatState : null) != null)
			{
				await Hook.AfterBlockGained(sourceCard.CombatState, creature, (decimal)num, (ValueProp)8, sourceCard);
			}
		}
	}
}
