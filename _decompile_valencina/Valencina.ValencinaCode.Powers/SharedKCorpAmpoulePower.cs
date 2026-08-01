using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using Valencina.ValencinaCode.Compat;

namespace Valencina.ValencinaCode.Powers;

public sealed class SharedKCorpAmpoulePower : ValencinaPower
{
	private sealed class SharedAmpouleState
	{
		public bool Available { get; set; } = true;
	}

	private static readonly Dictionary<ICombatState, SharedAmpouleState> States = new Dictionary<ICombatState, SharedAmpouleState>();

	private bool _pendingFullHeal;

	public override PowerType Type => (PowerType)1;

	public override PowerStackType StackType => (PowerStackType)1;

	public static void ResetForCombat(ICombatState combatState)
	{
		States[combatState] = new SharedAmpouleState();
	}

	public static bool IsAvailable(ICombatState? combatState)
	{
		if (combatState != null)
		{
			return GetState(combatState).Available;
		}
		return false;
	}

	public override decimal ModifyHpLostAfterOstyLate(Creature target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
	{
		if (target != ((PowerModel)this).Owner || ((PowerModel)this).Amount <= 0 || amount <= 0m || ((PowerModel)this).Owner.CurrentHp <= 0 || !IsAvailable(((PowerModel)this).Owner.CombatState))
		{
			return amount;
		}
		if (amount < (decimal)((PowerModel)this).Owner.CurrentHp)
		{
			return amount;
		}
		_pendingFullHeal = true;
		((PowerModel)this).Flash();
		return 0m;
	}

	public override async Task AfterModifyingHpLostAfterOsty()
	{
		if (_pendingFullHeal && ((PowerModel)this).Amount > 0 && IsAvailable(((PowerModel)this).Owner.CombatState))
		{
			_pendingFullHeal = false;
			await TriggerSharedFullHeal();
		}
	}

	public override bool ShouldDie(Creature creature)
	{
		if (creature != ((PowerModel)this).Owner || ((PowerModel)this).Amount <= 0 || !IsAvailable(((PowerModel)this).Owner.CombatState))
		{
			return true;
		}
		_pendingFullHeal = true;
		return false;
	}

	public override bool ShouldDieLate(Creature creature)
	{
		if (creature != ((PowerModel)this).Owner || ((PowerModel)this).Amount <= 0 || !IsAvailable(((PowerModel)this).Owner.CombatState))
		{
			return true;
		}
		_pendingFullHeal = true;
		return false;
	}

	public override async Task AfterPreventingDeath(Creature creature)
	{
		if (creature == ((PowerModel)this).Owner && ((PowerModel)this).Amount > 0 && IsAvailable(((PowerModel)this).Owner.CombatState))
		{
			_pendingFullHeal = false;
			await TriggerSharedFullHeal();
		}
	}

	public override async Task AfterDeath(PlayerChoiceContext choiceContext, Creature creature, bool wasRemovalPrevented, float deathAnimLength)
	{
		if (wasRemovalPrevented && creature == ((PowerModel)this).Owner && ((PowerModel)this).Amount > 0 && IsAvailable(((PowerModel)this).Owner.CombatState))
		{
			_pendingFullHeal = false;
			await TriggerSharedFullHeal();
		}
	}

	public override bool ShouldCreatureBeRemovedFromCombatAfterDeath(Creature creature)
	{
		if (creature == ((PowerModel)this).Owner && ((PowerModel)this).Amount > 0)
		{
			return !IsAvailable(((PowerModel)this).Owner.CombatState);
		}
		return true;
	}

	public override bool ShouldPowerBeRemovedAfterOwnerDeath()
	{
		return false;
	}

	public override bool ShouldStopCombatFromEnding()
	{
		return false;
	}

	private async Task TriggerSharedFullHeal()
	{
		ICombatState combatState = ((PowerModel)this).Owner.CombatState;
		if (combatState == null)
		{
			return;
		}
		SharedAmpouleState state = GetState(combatState);
		if (!state.Available)
		{
			return;
		}
		state.Available = false;
		((PowerModel)this).Flash();
		await CreatureCmd.SetCurrentHp(((PowerModel)this).Owner, (decimal)((PowerModel)this).Owner.MaxHp);
		await CompatPowerCmd.Apply<VulnerablePower>((PlayerChoiceContext)new BlockingPlayerChoiceContext(), ((PowerModel)this).Owner, 1m, ((PowerModel)this).Owner, (CardModel?)null, silent: false);
		foreach (SharedKCorpAmpoulePower item in (from power in combatState.Enemies.Select(delegate(Creature enemy)
			{
				if (enemy == null)
				{
					return (SharedKCorpAmpoulePower)null;
				}
				MonsterModel monster = enemy.Monster;
				if (monster == null)
				{
					return (SharedKCorpAmpoulePower)null;
				}
				Creature creature = monster.Creature;
				return (creature == null) ? null : creature.GetPower<SharedKCorpAmpoulePower>();
			})
			where power != null
			select power).Cast<SharedKCorpAmpoulePower>().ToList())
		{
			((PowerModel)item).SetAmount(0, false);
			await PowerCmd.Remove((PowerModel)(object)item);
		}
	}

	private static SharedAmpouleState GetState(ICombatState combatState)
	{
		if (!States.TryGetValue(combatState, out SharedAmpouleState value))
		{
			value = new SharedAmpouleState();
			States[combatState] = value;
		}
		return value;
	}
}
