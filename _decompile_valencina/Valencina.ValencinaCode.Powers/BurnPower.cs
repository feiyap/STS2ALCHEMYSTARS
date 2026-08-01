using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using Valencina.ValencinaCode.Systems;
using Valencina.ValencinaCode.Utils;

namespace Valencina.ValencinaCode.Powers;

public sealed class BurnPower : ValencinaPower
{
	public override PowerType Type => (PowerType)2;

	public override PowerStackType StackType => (PowerStackType)1;

	public override bool AllowNegative => false;

	private int TriggerCount
	{
		get
		{
			Creature owner = ((PowerModel)this).Owner;
			if (((owner != null) ? owner.CombatState : null) == null || ((PowerModel)this).Amount <= 0)
			{
				return 1;
			}
			int num = (from creature in ((PowerModel)this).Owner.CombatState.GetOpponentsOf(((PowerModel)this).Owner)
				where creature.IsAlive
				select creature).Sum(delegate(Creature creature)
			{
				AfterglowPower? afterglowPower = CreaturePowerAccess.Find<AfterglowPower>(creature);
				return Math.Max(0, (afterglowPower != null) ? ((PowerModel)afterglowPower).Amount : 0);
			});
			return Math.Clamp(1 + num, 1, Math.Max(1, ((PowerModel)this).Amount));
		}
	}

	public void SetStacks(decimal amount)
	{
		if (amount < 0m)
		{
			amount = default(decimal);
		}
		int num = (int)decimal.Truncate(amount);
		((PowerModel)this).SetAmount(num, false);
		((PowerModel)this).InitInternalData();
		((PowerModel)this).InvokeDisplayAmountChanged();
	}

	public void SetStacks(int amount)
	{
		if (amount < 0)
		{
			amount = 0;
		}
		((PowerModel)this).SetAmount(amount, false);
		((PowerModel)this).InitInternalData();
		((PowerModel)this).InvokeDisplayAmountChanged();
	}

	public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		if (((PowerModel)this).Owner == null || side == ((PowerModel)this).Owner.Side || ((PowerModel)this).Amount <= 0)
		{
			return;
		}
		int triggerCount = TriggerCount;
		for (int i = 0; i < triggerCount; i++)
		{
			if (((PowerModel)this).Owner == null)
			{
				break;
			}
			if (!((PowerModel)this).Owner.IsAlive)
			{
				break;
			}
			if (((PowerModel)this).Owner.IsDead)
			{
				break;
			}
			if (((PowerModel)this).Amount <= 0)
			{
				break;
			}
			await ResolveBurnOnceAsync(choiceContext);
		}
	}

	private async Task ResolveBurnOnceAsync(PlayerChoiceContext choiceContext)
	{
		if (((PowerModel)this).Owner == null || ((PowerModel)this).Amount <= 0)
		{
			return;
		}
		int damage = ((PowerModel)this).Amount;
		int nextAmount = damage / 2;
		((PowerModel)this).Flash();
		bool ownerSurvived = true;
		if (StatusSystem.HasHemostasisProtection(((PowerModel)this).Owner))
		{
			MainFile.Logger.Info($"[BurnPower] Hemostasis prevented {damage} non-attack HP loss on {((PowerModel)this).Owner.Name}.", 1);
		}
		else
		{
			int hpBeforeDamage = ((PowerModel)this).Owner.CurrentHp;
			await CreatureCmd.Damage(choiceContext, ((PowerModel)this).Owner, (decimal)damage, (ValueProp)6, (Creature)null, (CardModel)null);
			ownerSurvived = ((PowerModel)this).Owner.IsAlive && !((PowerModel)this).Owner.IsDead;
			MainFile.Logger.Info($"[BurnPower] {((PowerModel)this).Owner.Name} loses {damage} HP. nextHp={((PowerModel)this).Owner.CurrentHp}.", 1);
			await StatusSystem.RefreshMonsterIntentAfterPossibleStatusReviveAsync(((PowerModel)this).Owner, hpBeforeDamage, damage, "burn");
		}
		if (!ownerSurvived)
		{
			MainFile.Logger.Info("[BurnPower] skipped stack update on " + ((PowerModel)this).Owner.Name + "; owner died or entered a revive state.", 1);
			await Cmd.CustomScaledWait(0.1f, 0.25f, false, default(CancellationToken));
			return;
		}
		int reducedAmount = Math.Max(0, damage - nextAmount);
		if (nextAmount <= 0)
		{
			Creature burnOwner = ((PowerModel)this).Owner;
			await PowerCmd.Remove((PowerModel)(object)this);
			MainFile.Logger.Info("[BurnPower] removed from " + burnOwner.Name + ".", 1);
			await NotifyEnemyBurnReducedAsync(choiceContext, burnOwner, reducedAmount);
			return;
		}
		SetStacks(nextAmount);
		MainFile.Logger.Info($"[BurnPower] {((PowerModel)this).Owner.Name} -> {nextAmount}.", 1);
		await NotifyEnemyBurnReducedAsync(choiceContext, ((PowerModel)this).Owner, reducedAmount);
	}

	private async Task NotifyEnemyBurnReducedAsync(PlayerChoiceContext choiceContext, Creature burnOwner, int reducedAmount)
	{
		if (reducedAmount <= 0)
		{
			return;
		}
		ICombatState combatState = burnOwner.CombatState;
		foreach (Creature item in ((combatState != null) ? combatState.Creatures.Where((Creature creature) => creature.Side != burnOwner.Side && creature.IsAlive).OrderBy(StableCreatureKey).ToList() : null) ?? new List<Creature>())
		{
			foreach (IBurnReducedListener item2 in CreaturePowerAccess.Enumerate(item).OfType<IBurnReducedListener>().OrderBy(StableBurnReducedListenerKey)
				.ToList())
			{
				await item2.OnEnemyBurnReducedAsync(choiceContext, burnOwner, reducedAmount);
			}
		}
	}

	private static string StableCreatureKey(Creature creature)
	{
		object obj = creature.CombatId?.ToString("D10");
		if (obj == null)
		{
			Player player = creature.Player;
			obj = ((player != null) ? player.NetId.ToString() : null);
			if (obj == null)
			{
				MonsterModel monster = creature.Monster;
				obj = ((monster != null) ? ((AbstractModel)monster).Id.Entry : null) ?? creature.Name;
			}
		}
		return (string)obj;
	}

	private static string StableBurnReducedListenerKey(IBurnReducedListener listener)
	{
		PowerModel val = (PowerModel)((listener is PowerModel) ? listener : null);
		string text;
		if (val == null)
		{
			text = listener.GetType().FullName;
			if (text == null)
			{
				return listener.GetType().Name;
			}
		}
		else
		{
			Creature owner = val.Owner;
			text = (((owner == null) ? null : owner.CombatId?.ToString("D10")) ?? "no-owner") + ":" + ((AbstractModel)val).Id.Entry;
		}
		return text;
	}
}
