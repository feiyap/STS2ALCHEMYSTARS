using AlchemyStars.Characters;
using Godot;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Nodes.RestSite;
using MegaCrit.Sts2.Core.Random;
using STS2RitsuLib.Patching.Models;

namespace AlchemyStars.Patches;

/// <summary>
/// 篝火只给休息点场景的直接子 Spine 播章节循环。
/// 薇丝和卡莲挂在角色骨骼下面，骨架里也只有 idle，这里单独循环播放。
/// </summary>
public sealed class RestSiteCompanionSpinePatch : IPatchMethod
{
    private const string IdleAnim = "idle";

    public static string PatchId => "alchemy_stars_rest_site_companion_spine";

    public static string Description => "Loop idle on nested rest site companion spines";

    public static bool IsCritical => false;

    public static ModPatchTarget[] GetTargets() =>
    [
        new(typeof(NRestSiteCharacter), "_Ready"),
    ];

    public static void Postfix(NRestSiteCharacter __instance)
    {
        if (__instance.Player?.Character is not AlchemyStarsCharacter)
            return;

        foreach (var child in __instance.GetChildren())
            PlayNestedIdle(__instance, child);
    }

    private static void PlayNestedIdle(NRestSiteCharacter host, Node node)
    {
        foreach (var child in node.GetChildren())
        {
            if (child.GetClass() == MegaSprite.spineClassName)
            {
                var sprite = new MegaSprite(child);
                host.RunWhenSpineReady(sprite, animState =>
                {
                    if (!sprite.HasAnimation(IdleAnim))
                        return;

                    animState.SetAnimation(IdleAnim, loop: true, trackId: 0);
                    using var entry = animState.GetCurrent(0);
                    entry?.SetTrackTime(entry.GetAnimationEnd() * Rng.Chaotic.NextFloat());
                });
            }

            PlayNestedIdle(host, child);
        }
    }
}
