using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Scaffolding.Content;
using Valencina.ValencinaCode.Compat;

namespace Valencina.ValencinaCode.Monsters;

public abstract class GCompanySoldier : ModMonsterTemplate
{
	private const int BasePlating = 10;

	public override async Task AfterAddedToRoom()
	{
		await _003C_003En__0();
		await CompatPowerCmd.Apply<PlatingPower>((PlayerChoiceContext)new BlockingPlayerChoiceContext(), ((MonsterModel)this).Creature, 10m, ((MonsterModel)this).Creature, (CardModel?)null, silent: false);
	}

	public override async Task AfterDeath(PlayerChoiceContext choiceContext, Creature creature, bool wasRemovalPrevented, float deathAnimLength)
	{
		if (creature == ((MonsterModel)this).Creature && !wasRemovalPrevented)
		{
			ICombatState combatState = creature.CombatState;
			((combatState != null) ? combatState.Enemies.Select((Creature enemy) => (enemy == null) ? null : enemy.Monster).OfType<GCompanyMinister>().FirstOrDefault() : null)?.NotifyTeammateDeath();
		}
		await _003C_003En__1(choiceContext, creature, wasRemovalPrevented, deathAnimLength);
	}

	[CompilerGenerated]
	[DebuggerHidden]
	private Task _003C_003En__0()
	{
		return ((MonsterModel)this).AfterAddedToRoom();
	}

	[CompilerGenerated]
	[DebuggerHidden]
	private Task _003C_003En__1(PlayerChoiceContext choiceContext, Creature creature, bool wasRemovalPrevented, float deathAnimLength)
	{
		return ((AbstractModel)this).AfterDeath(choiceContext, creature, wasRemovalPrevented, deathAnimLength);
	}
}
