using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.ValueProps;
using Valencina.ValencinaCode.Compat;
using Valencina.ValencinaCode.Patches;
using Valencina.ValencinaCode.Powers;
using Valencina.ValencinaCode.Systems;
using Valencina.ValencinaCode.Utils;

namespace Valencina.ValencinaCode.Cards;

public static class DisposalAttackHelper
{
	public const decimal InsightPercentPerStackPerPrecognition = 0.5m;

	public const int ZeroCostGazeDamagePercent = 100;

	public static decimal GetDamageBonusPercent(CardModel card, Creature? target, ValueProp props)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		if (!(card is IDisposalAttackCard disposal) || !ValuePropExtensions.IsPoweredAttack(props))
		{
			return 0m;
		}
		decimal result = default(decimal);
		if (IsZeroCostDisposal(card, disposal))
		{
			result += 100m;
		}
		HuntingTargetPower huntingTargetPower = CreaturePowerAccess.Find<HuntingTargetPower>(target);
		if (huntingTargetPower != null && ((PowerModel)huntingTargetPower).Amount > 0)
		{
			result += huntingTargetPower.GetDisposalDamageBonusPercent(card, props);
		}
		return result;
	}

	public static decimal GetInsightDamageBonusPercent(IDisposalAttackCard disposal)
	{
		return 0m;
	}

	private static bool IsZeroCostDisposal(CardModel card, IDisposalAttackCard disposal)
	{
		if (disposal.ForceZeroCost)
		{
			return true;
		}
		try
		{
			return !card.EnergyCost.CostsX && card.EnergyCost.GetWithModifiers((CostModifiers)0) <= 0;
		}
		catch
		{
			return false;
		}
	}

	public static TCard Configure<TCard>(TCard card, int insight, DisposalGenerationEnhancement enhancement, bool forceRetain = false) where TCard : GeneratedDisposalCard
	{
		card.ConfigureDisposal(insight, enhancement.ExtraHits, enhancement.ExtraTremorDetonations, enhancement.ForceZeroCost, forceRetain, enhancement.UpgradeGeneratedDisposal);
		return card;
	}

	public static async Task PlayAsync(GeneratedDisposalCard card, PlayerChoiceContext choiceContext, Creature? target)
	{
		if (target == null || target.IsDead || !target.IsAlive)
		{
			return;
		}
		bool targetWasAlive = target.IsAlive && !target.IsDead;
		HuntingTargetPower huntingTargetPower = CreaturePowerAccess.Find<HuntingTargetPower>(target);
		bool targetWasMarked = huntingTargetPower != null && huntingTargetPower.ActiveStacks > 0;
		await StatusSystem.TryConvertTremorToBurningAsync(target, (CardModel?)(object)card, choiceContext);
		if (target.IsAlive && !target.IsDead)
		{
			Player owner = ((CardModel)card).Owner;
			ValencinaAnimation.QueueNextDisposalAttack((owner != null) ? owner.Creature : null, target);
			AttackCommand command = CommonActions.CardAttack((CardModel)(object)card, target, ((DynamicVar)((CardModel)card).DynamicVars.Damage).BaseValue, card.HitCount, "vfx/vfx_attack_slash");
			await card.ExecuteDisposalAttackCommandAsync(choiceContext, command);
		}
		if (target.IsAlive && !target.IsDead)
		{
			for (int i = 0; i < card.TremorDetonationCount; i++)
			{
				await StatusSystem.DetonateTremorAsync(target, (CardModel?)(object)card, consumeStacks: true, choiceContext);
				if (target.IsDead || !target.IsAlive)
				{
					break;
				}
			}
		}
		bool killedByThisDisposal = targetWasAlive && target.IsDead && !target.IsAlive;
		await RewardHuntingTargetExecutionCopyAsync(card, target, targetWasMarked, killedByThisDisposal);
		Player owner2 = ((CardModel)card).Owner;
		await AmmoSystem.ReloadToFullAsync((owner2 != null) ? owner2.Creature : null, (CardModel?)(object)card, choiceContext);
	}

	private static async Task RewardHuntingTargetExecutionCopyAsync(GeneratedDisposalCard source, Creature target, bool targetWasMarked, bool killedByThisDisposal)
	{
		Player owner = ((CardModel)source).Owner;
		if (!targetWasMarked || !killedByThisDisposal || owner == null || target.IsAlive || !target.IsDead)
		{
			return;
		}
		CardModel val = ((CardModel)source).CreateClone();
		if (val is GeneratedDisposalCard generatedDisposalCard)
		{
			generatedDisposalCard.ConfigureDisposal(source.Insight, source.ExtraHits, source.ExtraTremorDetonations, source.ForceZeroCost, source.ForceRetain, source.ForceUpgrade);
			while (((CardModel)generatedDisposalCard).CurrentUpgradeLevel < ((CardModel)source).CurrentUpgradeLevel && ((CardModel)generatedDisposalCard).IsUpgradable)
			{
				CardCmd.Upgrade((CardModel)(object)generatedDisposalCard, (CardPreviewStyle)2);
			}
		}
		await CardPileCmd.AddGeneratedCardToCombat(val, (PileType)2, owner, (CardPilePosition)2);
	}
}
