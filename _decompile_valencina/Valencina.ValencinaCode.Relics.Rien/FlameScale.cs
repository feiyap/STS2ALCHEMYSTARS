using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.ValueProps;
using Valencina.ValencinaCode.Powers;
using Valencina.ValencinaCode.Utils;

namespace Valencina.ValencinaCode.Relics.Rien;

public sealed class FlameScale : RienRelic
{
	private const string HpLossKey = "HpLoss";

	private const string CurrentHpPercentKey = "CurrentHpPercent";

	private readonly Dictionary<object, int> _burnBeforeTurnEnd = new Dictionary<object, int>(ReferenceEqualityComparer.Instance);

	public override bool HasUponPickupEffect => true;

	protected override IEnumerable<DynamicVar> CanonicalVars => (IEnumerable<DynamicVar>)(object)new DynamicVar[2]
	{
		(DynamicVar)new DamageVar("HpLoss", 20m, (ValueProp)6),
		new DynamicVar("CurrentHpPercent", 1m)
	};

	public override async Task AfterObtained()
	{
		await CreatureCmd.Damage((PlayerChoiceContext)new ThrowingPlayerChoiceContext(), ((RelicModel)this).Owner.Creature, (DamageVar)((RelicModel)this).DynamicVars["HpLoss"], (Creature)null, (CardModel)null);
	}

	public override Task BeforeCombatStart()
	{
		_burnBeforeTurnEnd.Clear();
		return Task.CompletedTask;
	}

	public override Task BeforeSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		if (side != ((RelicModel)this).Owner.Creature.Side || ((RelicModel)this).Owner.Creature.CombatState == null)
		{
			return Task.CompletedTask;
		}
		_burnBeforeTurnEnd.Clear();
		foreach (Creature item in ((RelicModel)this).Owner.Creature.CombatState.HittableEnemies.OrderBy(StableCreatureKey))
		{
			BurnPower burnPower = CreaturePowerAccess.Find<BurnPower>(item);
			if (burnPower != null && ((PowerModel)burnPower).Amount > 0)
			{
				_burnBeforeTurnEnd[item] = ((PowerModel)burnPower).Amount;
			}
		}
		return Task.CompletedTask;
	}

	public override async Task AfterSideTurnEndLate(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		if (side != ((RelicModel)this).Owner.Creature.Side || ((RelicModel)this).Owner.Creature.CombatState == null || _burnBeforeTurnEnd.Count == 0)
		{
			return;
		}
		bool flashed = false;
		foreach (Creature item in ((RelicModel)this).Owner.Creature.CombatState.HittableEnemies.OrderBy(StableCreatureKey).ToList())
		{
			if (!_burnBeforeTurnEnd.TryGetValue(item, out var value) || value <= 0)
			{
				continue;
			}
			BurnPower? burnPower = CreaturePowerAccess.Find<BurnPower>(item);
			int num = ((burnPower != null) ? ((PowerModel)burnPower).Amount : 0);
			int num2 = value - num;
			if (num2 <= 0)
			{
				continue;
			}
			decimal num3 = Math.Floor((decimal)item.CurrentHp * (((RelicModel)this).DynamicVars["CurrentHpPercent"].BaseValue / 100m) * (decimal)num2);
			if (!(num3 <= 0m))
			{
				if (!flashed)
				{
					((RelicModel)this).Flash();
					flashed = true;
				}
				await CreatureCmd.Damage(choiceContext, item, num3, (ValueProp)6, ((RelicModel)this).Owner.Creature, (CardModel)null);
			}
		}
		_burnBeforeTurnEnd.Clear();
	}

	public override Task AfterCombatEnd(CombatRoom room)
	{
		_burnBeforeTurnEnd.Clear();
		return Task.CompletedTask;
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
}
