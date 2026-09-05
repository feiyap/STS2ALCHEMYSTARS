using System.Collections.Concurrent;
using System.Reflection;
using AlchemyStars.Cards;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Cards;
using STS2RitsuLib.Patching.Models;

namespace AlchemyStars.Patches;

/// <summary>
/// 强化后若存在 <c>Foo+.png</c> 则改用该卡图，否则继续用强化前卡图。
/// </summary>
public sealed class UpgradedCardPortraitPathPatch : IPatchMethod
{
    public static string PatchId => "alchemy_stars_upgraded_card_portrait_path";

    public static string Description => "Use a separate portrait after upgrade when Foo+.png exists";

    public static bool IsCritical => false;

    public static ModPatchTarget[] GetTargets() =>
    [
        new(typeof(CardModel), "PortraitPath", MethodType.Getter),
    ];

    public static void Postfix(CardModel __instance, ref string __result)
    {
        if (AlchemyStarsCardArt.TryResolvePortraitPath(__instance, __result, out var path))
            __result = path;
    }
}

/// <summary>
/// 把强化后卡图纳入预加载，避免第一次强化时才读盘。
/// </summary>
public sealed class UpgradedCardAllPortraitPathsPatch : IPatchMethod
{
    public static string PatchId => "alchemy_stars_upgraded_card_all_portrait_paths";

    public static string Description => "Preload upgraded card portraits when Foo+.png exists";

    public static bool IsCritical => false;

    public static ModPatchTarget[] GetTargets() =>
    [
        new(typeof(CardModel), "AllPortraitPaths", MethodType.Getter),
    ];

    public static void Postfix(CardModel __instance, ref IEnumerable<string> __result)
    {
        __result = AlchemyStarsCardArt.AppendUpgradedPortraitPath(__instance, __result);
    }
}

/// <summary>
/// 同一张卡实例强化后刷新已显示的 <see cref="NCard"/>，否则卡图不会重载。
/// </summary>
public sealed class UpgradedCardArtReloadPatch : IPatchMethod
{
    public static string PatchId => "alchemy_stars_upgraded_card_art_reload";

    public static string Description => "Reload card visuals after upgrade so portrait can swap";

    public static bool IsCritical => false;

    private static readonly ConcurrentDictionary<NCard, Action> ReloadHandlers = new();

    public static ModPatchTarget[] GetTargets() =>
    [
        new(typeof(NCard), "SubscribeToModel", [typeof(CardModel)]),
        new(typeof(NCard), "UnsubscribeFromModel", [typeof(CardModel)]),
    ];

    public static void Postfix(NCard __instance, CardModel? model, MethodBase __originalMethod)
    {
        if (model == null)
            return;

        if (__originalMethod.Name == "SubscribeToModel")
            Attach(__instance, model);
        else
            Detach(__instance, model);
    }

    private static void Attach(NCard card, CardModel model)
    {
        Detach(card, model);
        Action handler = () => AlchemyStarsCardArt.ReloadIfValid(card);
        if (ReloadHandlers.TryAdd(card, handler))
            model.Upgraded += handler;
    }

    private static void Detach(NCard card, CardModel model)
    {
        if (!ReloadHandlers.TryRemove(card, out var handler))
            return;

        model.Upgraded -= handler;
    }
}
