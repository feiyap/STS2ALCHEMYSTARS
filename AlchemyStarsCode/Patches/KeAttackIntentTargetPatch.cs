using AlchemyStars.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Patching.Models;

namespace AlchemyStars.Patches;

/// <summary>
/// 勇士之锋·克只能以攻击意图的敌人为目标。
/// </summary>
public sealed class KeAttackIntentTargetPatch : IPatchMethod
{
    public static string PatchId => "alchemy_stars_ke_attack_intent_target";

    public static string Description => "Restrict AlchemyStarsKe targeting to enemies intending to attack";

    public static bool IsCritical => false;

    public static ModPatchTarget[] GetTargets() =>
    [
        new(typeof(CardModel), nameof(CardModel.IsValidTarget)),
    ];

    public static void Postfix(CardModel __instance, Creature? target, ref bool __result)
    {
        if (!__result || __instance is not AlchemyStarsKe)
            return;

        if (target?.IsPlayer == true)
        {
            __result = false;
            return;
        }

        if (target?.Monster?.IntendsToAttack != true)
            __result = false;
    }
}
