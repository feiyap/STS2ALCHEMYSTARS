using System.Collections.Generic;
using System.Linq;
using MegaCrit.Sts2.Core.Entities.Ancients;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Patching.Models;

namespace AlchemyStars.Patches;

/// <summary>
/// 先古对话 visit 索引若出现空洞，原版 GetValidDialogues 会返回空集，
/// NEventRoom.SetupLayout 对 NextItem 结果解引用会 NRE 并卡死事件房。
/// 此处在空结果时回退到可重复对话（再不行则任意对话）。
/// </summary>
public sealed class AncientDialogueEmptyFallbackPatch : IPatchMethod
{
    public static string PatchId => "alchemy_stars_ancient_dialogue_empty_fallback";

    public static string Description =>
        "Fallback when AncientDialogueSet.GetValidDialogues returns empty to avoid SetupLayout NRE";

    public static bool IsCritical => false;

    public static ModPatchTarget[] GetTargets() =>
    [
        new(
            typeof(AncientDialogueSet),
            nameof(AncientDialogueSet.GetValidDialogues),
            [typeof(ModelId), typeof(int), typeof(int), typeof(bool)]),
    ];

    public static void Postfix(
        AncientDialogueSet __instance,
        ref IEnumerable<AncientDialogue> __result)
    {
        // 已有可选对话则无需干预。
        if (__result.Any())
            return;

        var all = __instance.GetAllDialogues().ToList();
        if (all.Count == 0)
            return;

        var repeating = all.Where(static d => d.IsRepeating).ToList();
        __result = repeating.Count > 0 ? repeating : all;
        Entry.Logger.Warn(
            "[AncientDialogue] GetValidDialogues 为空，已回退到备用对话，避免事件房空引用卡死。");
    }
}
