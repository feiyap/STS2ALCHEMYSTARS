using System;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using Valencina.ValencinaCode.Monsters;

namespace Valencina.ValencinaCode.Powers;

public sealed class KaiserImperialMandatePower : ValencinaPower, IAddDumbVariablesToPowerDescription
{
	private bool _pendingPhaseTransition;

	public override PowerType Type => (PowerType)1;

	public override PowerStackType StackType => (PowerStackType)2;

	public void AddDumbVariablesToPowerDescription(LocString description)
	{
		object obj;
		if (!((AbstractModel)this).IsMutable)
		{
			obj = null;
		}
		else
		{
			Creature owner = ((PowerModel)this).Owner;
			obj = ((owner != null) ? owner.Monster : null) as UngezieferKaiser;
		}
		bool flag = ((UngezieferKaiser)obj)?.IsEmperorSubjectsDisabled ?? false;
		bool flag2 = ((UngezieferKaiser)obj)?.IsEmperorBloodDisabled ?? false;
		bool flag3 = ((UngezieferKaiser)obj)?.HasEnteredPhaseTwo ?? false;
		description.Add("Subjects", Text(flag ? "VALENCINA_KAISER_MANDATE_SUBJECTS_DISABLED" : "VALENCINA_KAISER_MANDATE_SUBJECTS_ACTIVE", flag ? "[red]Emperor's Subjects: disabled.[/red]" : "[gold]Emperor's Subjects:[/gold] starts with [blue]20[/blue] Defense Pest; recovers [blue]20[/blue]/[blue]15[/blue]/[blue]10[/blue]/[blue]5[/blue] on the first four Kaiser turns, capped at [blue]20[/blue]. If it ends a turn at [red]0[/red], skip the next recovery and gain [red]1[/red] Vulnerable."));
		description.Add("Cloak", Text("VALENCINA_KAISER_MANDATE_CLOAK", "[gold]Emperor's Cloak:[/gold] at turn start, gain Block equal to Defense Pest x [blue]20[/blue] and [gold]50[/gold] Hard to Kill. [gold]Excision[/gold] turns remove Hard to Kill. Immune to Block clearing effects."));
		description.Add("Army", Text("VALENCINA_KAISER_MANDATE_ARMY", "[gold]Emperor's Army:[/gold] attacks that hit and deal damage apply [red]3[/red] Attack Pest. Whenever the player shuffles, add [gold]1[/gold] Infection to the discard pile."));
		description.Add("Blood", Text(flag2 ? "VALENCINA_KAISER_MANDATE_BLOOD_DISABLED" : "VALENCINA_KAISER_MANDATE_BLOOD_ACTIVE", flag2 ? "[red]Emperor's Blood: disabled.[/red]" : "[gold]Emperor's Blood:[/gold] debuff damage and direct damage are reduced by [blue]50%[/blue]. For every [red]25%[/red] max HP lost, permanently lose [red]1[/red] Strength. Immune to Stun."));
		description.Add("Wrath", Text(flag3 ? "VALENCINA_KAISER_MANDATE_WRATH_PHASE_TWO" : "VALENCINA_KAISER_MANDATE_WRATH_PHASE_ONE", flag3 ? "[red]Emperor's Wrath: phase transition completed; phase two no longer locks HP.[/red]" : "[gold]Emperor's Wrath:[/gold] before the phase transition, HP cannot drop below [gold]50%[/gold] of max HP. Upon reaching that threshold, immediately enter phase two: remove debuffs, gain [blue]20[/blue] Defense Pest, and next turn uses [gold]Son, Aim for the Heart[/gold]."));
	}

	private static string Text(string key, string fallback)
	{
		LocString ifExists = LocString.GetIfExists("powers", key);
		return ((ifExists != null) ? ifExists.GetFormattedText() : null) ?? fallback;
	}

	public override bool ShouldClearBlock(Creature creature)
	{
		return true;
	}

	public override decimal ModifyHpLostAfterOstyLate(Creature target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
	{
		if (target != ((PowerModel)this).Owner || amount <= 0m || !(((PowerModel)this).Owner.Monster is UngezieferKaiser { CurrentPhaseTransitionLockHp: var currentPhaseTransitionLockHp } ungezieferKaiser))
		{
			return amount;
		}
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
		if (_pendingPhaseTransition && ((PowerModel)this).Owner.Monster is UngezieferKaiser ungezieferKaiser)
		{
			_pendingPhaseTransition = false;
			await ungezieferKaiser.TryEnterPhaseTwoFromCurrentHp((PlayerChoiceContext)new BlockingPlayerChoiceContext());
		}
	}

	public override bool ShouldDie(Creature creature)
	{
		if (((PowerModel)this).Owner.Monster is UngezieferKaiser ungezieferKaiser && ungezieferKaiser.ShouldPreventPhaseTransitionDeath(creature))
		{
			_pendingPhaseTransition = true;
			ungezieferKaiser.MarkPhaseTransitionPending();
			return false;
		}
		return true;
	}

	public override bool ShouldDieLate(Creature creature)
	{
		if (((PowerModel)this).Owner.Monster is UngezieferKaiser ungezieferKaiser && ungezieferKaiser.ShouldPreventPhaseTransitionDeath(creature))
		{
			_pendingPhaseTransition = true;
			ungezieferKaiser.MarkPhaseTransitionPending();
			return false;
		}
		return true;
	}

	public override async Task AfterPreventingDeath(Creature creature)
	{
		if (((PowerModel)this).Owner.Monster is UngezieferKaiser ungezieferKaiser && ungezieferKaiser.ShouldPreventPhaseTransitionDeath(creature))
		{
			_pendingPhaseTransition = false;
			await ungezieferKaiser.TryEnterPhaseTwoFromCurrentHp((PlayerChoiceContext)new BlockingPlayerChoiceContext());
		}
	}
}
