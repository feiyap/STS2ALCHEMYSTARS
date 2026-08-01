using System;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using Valencina.ValencinaCode.Compat;

namespace Valencina.ValencinaCode.Powers;

public sealed class OdinEyePower : ValencinaPower
{
	private const int MaxStacks = 1;

	private bool _triggered;

	public override PowerType Type => (PowerType)1;

	public override PowerStackType StackType => (PowerStackType)2;

	public override bool AllowNegative => false;

	public override bool TryModifyPowerAmountReceived(PowerModel canonicalPower, Creature target, decimal amount, Creature? applier, out decimal modifiedAmount)
	{
		modifiedAmount = amount;
		if (target != ((PowerModel)this).Owner || !(canonicalPower is OdinEyePower) || amount <= 0m)
		{
			return false;
		}
		modifiedAmount = Math.Max(0m, Math.Min(amount, 1 - ((PowerModel)this).Amount));
		return modifiedAmount != amount;
	}

	public async Task ValencinaAfterPowerAmountChanged(PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
	{
		if ((object)power == this && ((PowerModel)this).Owner != null && !(amount == 0m))
		{
			await CompatPowerCmd.Apply<StrengthPower>((PlayerChoiceContext)new BlockingPlayerChoiceContext(), ((PowerModel)this).Owner, amount * 2m, ((PowerModel)this).Owner, cardSource, silent: false);
		}
	}

	public override decimal ModifyHpLostBeforeOsty(Creature target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
	{
		if (((PowerModel)this).Owner == null || target != ((PowerModel)this).Owner || ((PowerModel)this).Amount <= 0 || amount <= 0m)
		{
			return amount;
		}
		decimal num = 5m * (decimal)((PowerModel)this).Amount;
		return Math.Max(0m, amount - num);
	}

	public override Task AfterModifyingHpLostBeforeOsty()
	{
		((PowerModel)this).Flash();
		return Task.CompletedTask;
	}

	public override Task AfterDamageReceived(PlayerChoiceContext choiceContext, Creature target, DamageResult result, ValueProp props, Creature? dealer, CardModel? cardSource)
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		if (((PowerModel)this).Owner != null && target == ((PowerModel)this).Owner && ((PowerModel)this).Amount > 0 && result.UnblockedDamage > 0 && ((PowerModel)this).CombatState.CurrentSide != ((PowerModel)this).Owner.Side)
		{
			_triggered = true;
		}
		return Task.CompletedTask;
	}

	public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
	{
		if (((PowerModel)this).Owner != null && player.Creature == ((PowerModel)this).Owner && _triggered && ((PowerModel)this).Amount > 0)
		{
			_triggered = false;
			((PowerModel)this).Flash();
			await CommonActions.Apply<WeakPower>(choiceContext, ((PowerModel)this).Owner, (CardModel?)null, 1m, silent: false);
			await CommonActions.Apply<VulnerablePower>(choiceContext, ((PowerModel)this).Owner, (CardModel?)null, 1m, silent: false);
			await CompatPowerCmd.ModifyAmount(choiceContext, (PowerModel)(object)this, -1m, null, null);
		}
	}

	public override async Task AfterRemoved(Creature oldOwner)
	{
		if (((PowerModel)this).Amount > 0)
		{
			await CompatPowerCmd.Apply<StrengthPower>((PlayerChoiceContext)new BlockingPlayerChoiceContext(), oldOwner, -2m * (decimal)((PowerModel)this).Amount, oldOwner, (CardModel?)null, silent: false);
		}
	}
}
