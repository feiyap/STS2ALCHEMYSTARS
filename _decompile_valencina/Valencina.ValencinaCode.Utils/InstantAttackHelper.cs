using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using Valencina.ValencinaCode.Cards;
using Valencina.ValencinaCode.Compat;
using Valencina.ValencinaCode.Patches;
using Valencina.ValencinaCode.Systems;

namespace Valencina.ValencinaCode.Utils;

public static class InstantAttackHelper
{
	public static async Task ExecuteAgainstTargetAsync(ValencinaCard source, PlayerChoiceContext choiceContext, Creature? target, int hitCount, string vfx = "vfx/vfx_attack_slash")
	{
		int instantAmmoCost = GetInstantAmmoCost(source);
		await ExecuteAgainstTargetAsync(source, choiceContext, target, instantAmmoCost, hitCount, vfx);
	}

	public static async Task ExecuteAgainstTargetAsync(ValencinaCard source, PlayerChoiceContext choiceContext, Creature? target, int ammoCost, int hitCount, string vfx = "vfx/vfx_attack_slash")
	{
		await ExecuteAgainstTargetAsync(source, choiceContext, target, ammoCost, hitCount, vfx, 1m);
	}

	public static async Task ExecuteAgainstTargetAsync(ValencinaCard source, PlayerChoiceContext choiceContext, Creature? target, int ammoCost, int hitCount, string vfx, decimal breathingMethodDamageMultiplier)
	{
		Player owner = ((CardModel)source).Owner;
		Creature owner2 = ((owner != null) ? owner.Creature : null);
		if (owner2 == null || target == null)
		{
			return;
		}
		await AmmoSystem.TryConsumeAsync(owner2, ammoCost, (CardModel?)(object)source, grantBreathingMethod: true, choiceContext);
		await ValencinaAttackScope.RunAsync(owner2, preserveBreathingMethod: true, breathingMethodDamageMultiplier, async delegate
		{
			InstantAttackBreathingMethodRegistry.Begin(owner2, BreathingMethodStateHelper.GetAmount(owner2));
			try
			{
				ValencinaAnimation.QueueNextAttackVariant(owner2, hitCount);
				await CommonActions.CardAttack((CardModel)(object)source, target, hitCount, vfx).Execute(choiceContext);
			}
			finally
			{
				InstantAttackBreathingMethodRegistry.End(owner2);
			}
		});
	}

	public static async Task ExecuteAgainstPlayAsync(ValencinaCard source, PlayerChoiceContext choiceContext, CardPlay play, int hitCount, string vfx = "vfx/vfx_attack_slash")
	{
		int instantAmmoCost = GetInstantAmmoCost(source);
		await ExecuteAgainstAllEnemiesAsync(source, choiceContext, play, instantAmmoCost, hitCount, vfx);
	}

	public static async Task ExecuteAgainstAllEnemiesAsync(ValencinaCard source, PlayerChoiceContext choiceContext, CardPlay play, int hitCount, string vfx = "vfx/vfx_attack_slash")
	{
		int instantAmmoCost = GetInstantAmmoCost(source);
		await ExecuteAgainstAllEnemiesAsync(source, choiceContext, play, instantAmmoCost, hitCount, vfx);
	}

	public static async Task ExecuteAgainstAllEnemiesAsync(ValencinaCard source, PlayerChoiceContext choiceContext, CardPlay play, int ammoCost, int hitCount, string vfx = "vfx/vfx_attack_slash")
	{
		await ExecuteAgainstAllEnemiesAsync(source, choiceContext, play, ammoCost, hitCount, vfx, 1m);
	}

	public static async Task ExecuteAgainstAllEnemiesAsync(ValencinaCard source, PlayerChoiceContext choiceContext, CardPlay play, int ammoCost, int hitCount, string vfx, decimal breathingMethodDamageMultiplier)
	{
		Player owner = ((CardModel)source).Owner;
		Creature owner2 = ((owner != null) ? owner.Creature : null);
		if (owner2 == null)
		{
			return;
		}
		await AmmoSystem.TryConsumeAsync(owner2, ammoCost, (CardModel?)(object)source, grantBreathingMethod: true, choiceContext);
		await ValencinaAttackScope.RunAsync(owner2, preserveBreathingMethod: true, breathingMethodDamageMultiplier, async delegate
		{
			InstantAttackBreathingMethodRegistry.Begin(owner2, BreathingMethodStateHelper.GetAmount(owner2));
			try
			{
				ValencinaAnimation.QueueNextAttackVariant(owner2, hitCount);
				if (play.Target != null)
				{
					await CommonActions.CardAttack((CardModel)(object)source, play, hitCount, vfx).Execute(choiceContext);
				}
				else
				{
					await CommonActions.CardAttackAllOpponents((CardModel)(object)source, hitCount, vfx).Execute(choiceContext);
				}
			}
			finally
			{
				InstantAttackBreathingMethodRegistry.End(owner2);
			}
		});
	}

	private static int GetInstantAmmoCost(ValencinaCard source)
	{
		if (!(source is IInstantAttackCard instantAttackCard))
		{
			return 0;
		}
		return instantAttackCard.InstantAmmoCost;
	}
}
