using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Scaffolding.Content;
using Valencina.ValencinaCode.Compat;
using Valencina.ValencinaCode.Powers;

namespace Valencina.ValencinaCode.Relics;

internal static class ForesightEyeDestinedFutureHelper
{
	public static bool DidOwnerTakeEnemyTurnDamage(Creature? owner, Creature target, DamageResult result, Creature? dealer)
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		if (owner != null && target == owner && result.UnblockedDamage > 0)
		{
			ICombatState combatState = owner.CombatState;
			if (((combatState != null) ? new CombatSide?(combatState.CurrentSide) : ((CombatSide?)null)) != (CombatSide?)owner.Side)
			{
				return ((dealer != null) ? new CombatSide?(dealer.Side) : ((CombatSide?)null)) != (CombatSide?)owner.Side;
			}
		}
		return false;
	}

	public static async Task GrantDestinedFutureIfUntouchedAsync(PlayerChoiceContext choiceContext, ModRelicTemplate source, Creature? owner, CombatSide endedSide, bool tookDamageThisEnemyTurn)
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		if (!(owner == null || endedSide == owner.Side || tookDamageThisEnemyTurn) && owner.Player != null && owner.IsAlive && !owner.IsDead)
		{
			((RelicModel)source).Flash();
			await CommonActions.Apply<DestinedFuturePower>(choiceContext, owner, (CardModel?)null, 1m, silent: false);
		}
	}
}
