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
using MegaCrit.Sts2.Core.ValueProps;
using Valencina.ValencinaCode.Systems;

namespace Valencina.ValencinaCode.Relics.Rien;

public sealed class Rainstorm : RienRelic
{
	private const string HpLossKey = "HpLoss";

	private const string TremorKey = "Tremor";

	public override bool HasUponPickupEffect => true;

	protected override IEnumerable<DynamicVar> CanonicalVars => (IEnumerable<DynamicVar>)(object)new DynamicVar[2]
	{
		(DynamicVar)new DamageVar("HpLoss", 20m, (ValueProp)6),
		new DynamicVar("Tremor", 6m)
	};

	public override async Task AfterObtained()
	{
		await CreatureCmd.Damage((PlayerChoiceContext)new ThrowingPlayerChoiceContext(), ((RelicModel)this).Owner.Creature, (DamageVar)((RelicModel)this).DynamicVars["HpLoss"], (Creature)null, (CardModel)null);
	}

	public async Task ValencinaAfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		if (side != ((RelicModel)this).Owner.Creature.Side)
		{
			return;
		}
		((RelicModel)this).Flash();
		foreach (Creature item in combatState.HittableEnemies.OrderBy(StableCreatureKey))
		{
			await StatusSystem.ApplyTremorAsync(item, ((RelicModel)this).DynamicVars["Tremor"].IntValue, null, allowStarterRelicConversion: false);
		}
	}

	public override async Task BeforeSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		if (side != ((RelicModel)this).Owner.Creature.Side || ((RelicModel)this).Owner.Creature.CombatState == null)
		{
			return;
		}
		((RelicModel)this).Flash();
		foreach (Creature item in ((RelicModel)this).Owner.Creature.CombatState.HittableEnemies.OrderBy(StableCreatureKey))
		{
			await StatusSystem.DetonateTremorAsync(item, null, consumeStacks: false, choiceContext);
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
}
