using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;

namespace Valencina.ValencinaCode.Relics.Rien;

public sealed class UnhatchedSpark : RienRelic
{
	private bool _triggeredThisCombat;

	private bool _protectionActive;

	public override Task BeforeCombatStart()
	{
		_triggeredThisCombat = false;
		_protectionActive = false;
		return Task.CompletedTask;
	}

	public override Task AfterCombatEnd(CombatRoom room)
	{
		_triggeredThisCombat = false;
		_protectionActive = false;
		return Task.CompletedTask;
	}

	public override Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
	{
		if (player == ((RelicModel)this).Owner)
		{
			_protectionActive = false;
		}
		return Task.CompletedTask;
	}

	public override bool ShouldDieLate(Creature creature)
	{
		Player owner = ((RelicModel)this).Owner;
		if (((owner != null) ? owner.Creature : null) == null || creature != ((RelicModel)this).Owner.Creature || ((RelicModel)this).Owner.Creature.CombatState == null)
		{
			return true;
		}
		if (_protectionActive)
		{
			return false;
		}
		return _triggeredThisCombat;
	}

	public override async Task AfterPreventingDeath(Creature creature)
	{
		Player owner = ((RelicModel)this).Owner;
		if (((owner != null) ? owner.Creature : null) != null && creature == ((RelicModel)this).Owner.Creature && ((RelicModel)this).Owner.Creature.CombatState != null && (!_triggeredThisCombat || _protectionActive))
		{
			if (!_triggeredThisCombat)
			{
				_triggeredThisCombat = true;
				_protectionActive = true;
				((RelicModel)this).Flash();
			}
			if (creature.CurrentHp < 1)
			{
				await CreatureCmd.Heal(creature, (decimal)(1 - creature.CurrentHp), true);
			}
		}
	}
}
