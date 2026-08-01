using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Hooks;
using Valencina.ValencinaCode.Character;

namespace Valencina.ValencinaCode.Patches;

[HarmonyPatch(typeof(Hook), "BeforeCardPlayed")]
internal static class ValencinaSkillCardAnimationPatch
{
	private static void Prefix(CardPlay cardPlay)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Invalid comparison between Unknown and I4
		if ((int)cardPlay.Card.Type != 2)
		{
			return;
		}
		Player owner = cardPlay.Card.Owner;
		Creature val = ((owner != null) ? owner.Creature : null);
		if (val != null)
		{
			Player player = val.Player;
			if (((player != null) ? player.Character : null) is Valencina.ValencinaCode.Character.Valencina)
			{
				ValencinaAnimation.PlaySkill1FromCard(val);
			}
		}
	}
}
