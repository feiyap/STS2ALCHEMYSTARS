using System.Reflection;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using Valencina.ValencinaCode.Character;
using Valencina.ValencinaCode.Utils;

namespace Valencina.ValencinaCode.Patches;

[HarmonyPatch(typeof(AttackCommand), "Execute")]
internal static class ValencinaAttackCommandAnimationQueuePatch
{
	private static readonly FieldInfo? HitCountField = AccessTools.Field(typeof(AttackCommand), "_hitCount");

	private static readonly FieldInfo? PlayOnEveryHitField = AccessTools.Field(typeof(AttackCommand), "_playOnEveryHit");

	private static void Prefix(AttackCommand __instance)
	{
		Creature attacker = __instance.Attacker;
		object obj;
		if (attacker == null)
		{
			obj = null;
		}
		else
		{
			Player player = attacker.Player;
			obj = ((player != null) ? player.Character : null);
		}
		if (!(obj is Valencina.ValencinaCode.Character.Valencina))
		{
			return;
		}
		int hitCount = 1;
		bool playOnEveryHit = true;
		try
		{
			if (HitCountField?.GetValue(__instance) is int num)
			{
				hitCount = num;
			}
			if (PlayOnEveryHitField?.GetValue(__instance) is bool flag)
			{
				playOnEveryHit = flag;
			}
		}
		catch
		{
			hitCount = 1;
			playOnEveryHit = true;
		}
		bool num2 = ValencinaAnimation.HasQueuedDisposalAttack(attacker);
		if (!num2)
		{
			ValencinaAnimation.ClearPostDisposalAttackSuppression(attacker);
		}
		ValencinaAnimation.QueueNextAttackVariant(attacker, hitCount, playOnEveryHit);
		if (!num2)
		{
			ValencinaVoiceSfx.TryPlayAttackVoice(attacker);
		}
	}

	private static void Postfix(AttackCommand __instance, ref Task<AttackCommand> __result)
	{
		Creature attacker = __instance.Attacker;
		object obj;
		if (attacker == null)
		{
			obj = null;
		}
		else
		{
			Player player = attacker.Player;
			obj = ((player != null) ? player.Character : null);
		}
		if (obj is Valencina.ValencinaCode.Character.Valencina)
		{
			__result = ClearAttackCommandAnimationStateAfterAsync(__result, attacker);
		}
	}

	private static async Task<AttackCommand> ClearAttackCommandAnimationStateAfterAsync(Task<AttackCommand> task, Creature attacker)
	{
		try
		{
			return await task;
		}
		finally
		{
			ValencinaAnimation.ClearAttackCommandAnimationState(attacker);
		}
	}
}
