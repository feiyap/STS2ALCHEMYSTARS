using Godot;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Nodes.Screens.Shops;
using MegaCrit.Sts2.Core.Random;
using STS2RitsuLib.Patching.Models;

namespace AlchemyStars.Patches;

/// <summary>
/// 商店角色场景会被包进 NMerchantCharacter，游戏只给第一个子节点播 Spine。
/// 空裔商人场景的 Spine 在内层，这里改到真正的 SpineSprite 上播放。
/// </summary>
public sealed class MerchantSpinePlaybackPatch : IPatchMethod
{
    public static string PatchId => "alchemy_stars_merchant_spine_playback";

    public static string Description => "Play nested merchant Spine animations";

    public static bool IsCritical => false;

    public static ModPatchTarget[] GetTargets() =>
    [
        new(typeof(NMerchantCharacter), nameof(NMerchantCharacter.PlayAnimation)),
    ];

    public static bool Prefix(NMerchantCharacter __instance, string anim, bool loop)
    {
        if (__instance.GetChildCount() == 0)
            return true;

        // 第一个子节点已经是 Spine 时，交给原版播放。
        var first = __instance.GetChild(0);
        if (first.GetClass() == MegaSprite.spineClassName)
            return true;

        var spine = FindSpine(first);
        if (spine == null)
            return true;

        var state = new MegaSprite(spine).GetAnimationState();
        state.SetAnimation(anim, loop);
        if (loop)
        {
            var current = state.GetCurrent(0);
            if (current != null)
                current.SetTrackTime(current.GetAnimationEnd() * Rng.Chaotic.NextFloat());
        }

        return false;
    }

    private static Node? FindSpine(Node node)
    {
        if (node.GetClass() == MegaSprite.spineClassName)
            return node;

        foreach (var child in node.GetChildren())
        {
            var found = FindSpine(child);
            if (found != null)
                return found;
        }

        return null;
    }
}
