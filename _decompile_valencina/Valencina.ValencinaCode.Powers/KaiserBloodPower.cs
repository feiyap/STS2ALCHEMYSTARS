using System;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using Valencina.ValencinaCode.Monsters;

namespace Valencina.ValencinaCode.Powers;

public sealed class KaiserBloodPower : ValencinaPower
{
	private bool _pendingPhaseTransition;

	public override PowerType Type => (PowerType)1;

	public override PowerStackType StackType => (PowerStackType)2;

	protected override bool IsVisibleInternal => false;

	public override decimal ModifyDamageMultiplicative(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
	{
		if (target != ((PowerModel)this).Owner || amount <= 0m)
		{
			return 1m;
		}
		if (!(((PowerModel)this).Owner.Monster is UngezieferKaiser { IsEmperorBloodDisabled: not false }))
		{
			return 0.5m;
		}
		return 1m;
	}

	public override decimal ModifyHpLostAfterOstyLate(Creature target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
	{
		if (target != ((PowerModel)this).Owner || amount <= 0m || !(((PowerModel)this).Owner.Monster is UngezieferKaiser ungezieferKaiser))
		{
			return amount;
		}
		if (ungezieferKaiser.IsEmperorBloodDisabled)
		{
			return amount;
		}
		int currentPhaseTransitionLockHp = ungezieferKaiser.CurrentPhaseTransitionLockHp;
		if (ungezieferKaiser.ShouldLockHpForPhaseTransition && (decimal)((PowerModel)this).Owner.CurrentHp - amount <= (decimal)currentPhaseTransitionLockHp)
		{
			_pendingPhaseTransition = true;
			ungezieferKaiser.MarkPhaseTransitionPending();
			((PowerModel)this).Flash();
			return Math.Min(amount, Math.Max(0m, ((PowerModel)this).Owner.CurrentHp - currentPhaseTransitionLockHp));
		}
		return amount;
	}

	public override async Task AfterModifyingHpLostAfterOsty()
	{
		MonsterModel monster = ((PowerModel)this).Owner.Monster;
		if (monster is UngezieferKaiser kaiser)
		{
			await kaiser.ApplyQuarterHpStrengthLosses((PlayerChoiceContext)new BlockingPlayerChoiceContext());
			if (_pendingPhaseTransition)
			{
				_pendingPhaseTransition = false;
				await kaiser.TryEnterPhaseTwoFromCurrentHp((PlayerChoiceContext)new BlockingPlayerChoiceContext());
			}
		}
	}
}
