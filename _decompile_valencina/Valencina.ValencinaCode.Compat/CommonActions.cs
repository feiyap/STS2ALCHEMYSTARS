using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace Valencina.ValencinaCode.Compat;

public static class CommonActions
{
	public static async Task<TPower?> Apply<TPower>(PlayerChoiceContext choiceContext, Creature target, CardModel? sourceCard, decimal amount, bool silent = false) where TPower : PowerModel
	{
		object obj;
		if (sourceCard == null)
		{
			obj = null;
		}
		else
		{
			Player owner = sourceCard.Owner;
			obj = ((owner != null) ? owner.Creature : null);
		}
		Creature val = (Creature)obj;
		return (await PowerCmd.Apply<TPower>(choiceContext, (IEnumerable<Creature>)(object)new Creature[1] { target }, amount, val, sourceCard, silent)).FirstOrDefault();
	}

	public static async Task<TPower?> ApplySelf<TPower>(PlayerChoiceContext choiceContext, CardModel sourceCard, decimal amount, bool silent = false) where TPower : PowerModel
	{
		Player owner = sourceCard.Owner;
		return (await PowerCmd.Apply<TPower>(choiceContext, (IEnumerable<Creature>)(object)new Creature[1] { owner.Creature }, amount, owner.Creature, sourceCard, silent)).FirstOrDefault();
	}

	public static Task<IEnumerable<CardModel>> Draw(CardModel sourceCard, PlayerChoiceContext choiceContext, decimal count = 1m)
	{
		return CardPileCmd.Draw(choiceContext, count, sourceCard.Owner, false);
	}

	public static AttackCommand CardAttack(CardModel sourceCard, CardPlay cardPlay, int hitCount = 1, string? vfx = null, string? sfx = null, string? tmpSfx = null)
	{
		return CardAttack(sourceCard, cardPlay.Target, ((DynamicVar)sourceCard.DynamicVars.Damage).BaseValue, hitCount, vfx, sfx, tmpSfx);
	}

	public static AttackCommand CardAttack(CardModel sourceCard, Creature? target, int hitCount = 1, string? vfx = null, string? sfx = null, string? tmpSfx = null)
	{
		return CardAttack(sourceCard, target, ((DynamicVar)sourceCard.DynamicVars.Damage).BaseValue, hitCount, vfx, sfx, tmpSfx);
	}

	public static AttackCommand CardAttack(CardModel sourceCard, Creature? target, decimal damage, int hitCount = 1, string? vfx = null, string? sfx = null, string? tmpSfx = null)
	{
		if (target == null)
		{
			throw new InvalidOperationException("Cannot create card attack for '" + ((object)sourceCard).GetType().Name + "' without a target.");
		}
		return DamageCmd.Attack(damage).FromCard(sourceCard).Targeting(target)
			.WithHitCount(hitCount)
			.WithHitFx(vfx, sfx, tmpSfx);
	}

	public static AttackCommand CardAttackAllOpponents(CardModel sourceCard, int hitCount = 1, string? vfx = null, string? sfx = null, string? tmpSfx = null)
	{
		return CardAttackAllOpponents(sourceCard, ((DynamicVar)sourceCard.DynamicVars.Damage).BaseValue, hitCount, vfx, sfx, tmpSfx);
	}

	public static AttackCommand CardAttackAllOpponents(CardModel sourceCard, decimal damage, int hitCount = 1, string? vfx = null, string? sfx = null, string? tmpSfx = null)
	{
		if (sourceCard.CombatState == null)
		{
			throw new InvalidOperationException("Cannot create AOE card attack for '" + ((object)sourceCard).GetType().Name + "' without combat state.");
		}
		return DamageCmd.Attack(damage).FromCard(sourceCard).TargetingAllOpponents(sourceCard.CombatState)
			.WithHitCount(hitCount)
			.WithHitFx(vfx, sfx, tmpSfx);
	}

	public static IEnumerable<DamageResult> DamageResults(AttackCommand command)
	{
		return command.Results.SelectMany((List<DamageResult> results) => results);
	}
}
