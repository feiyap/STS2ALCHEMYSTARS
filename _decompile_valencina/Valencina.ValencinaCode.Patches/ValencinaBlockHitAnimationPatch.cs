using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.ValueProps;
using Valencina.ValencinaCode.Character;
using Valencina.ValencinaCode.Powers;

namespace Valencina.ValencinaCode.Patches;

[HarmonyPatch]
internal static class ValencinaBlockHitAnimationPatch
{
	private static MethodBase TargetMethod()
	{
		return AccessTools.Method(typeof(CreatureCmd), "Damage", new Type[6]
		{
			typeof(PlayerChoiceContext),
			typeof(IEnumerable<Creature>),
			typeof(decimal),
			typeof(ValueProp),
			typeof(Creature),
			typeof(CardModel)
		}, (Type[])null) ?? throw new MissingMethodException(typeof(CreatureCmd).FullName, "Damage");
	}

	private static void Postfix(ref Task<IEnumerable<DamageResult>> __result)
	{
		__result = PlayBlockHitAfterDamage(__result);
	}

	private static async Task<IEnumerable<DamageResult>> PlayBlockHitAfterDamage(Task<IEnumerable<DamageResult>> resultTask)
	{
		IEnumerable<DamageResult> enumerable = await resultTask;
		foreach (DamageResult item in enumerable)
		{
			Player player = item.Receiver.Player;
			if (!(((player != null) ? player.Character : null) is Valencina.ValencinaCode.Character.Valencina))
			{
				continue;
			}
			NCombatRoom instance = NCombatRoom.Instance;
			NCreature val = ((instance != null) ? instance.GetCreatureNode(item.Receiver) : null);
			if (val != null && !InstantForesightPower.WasPreventedByPrecognition(item) && !item.WasTargetKilled && !item.Receiver.IsDead && item.Receiver.CurrentHp > 0)
			{
				if (item.UnblockedDamage > 0)
				{
					ValencinaAnimation.PlayDamageFrame(val);
				}
				else if (item.WasFullyBlocked)
				{
					ValencinaAnimation.PlayOn(val, "block_hit");
				}
			}
		}
		return enumerable;
	}
}
