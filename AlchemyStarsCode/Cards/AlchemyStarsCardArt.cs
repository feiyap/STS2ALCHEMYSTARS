using System.Collections.Concurrent;
using Godot;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Cards;
using STS2RitsuLib.Scaffolding.Content.Patches;
using STS2RitsuLib.Utils;

namespace AlchemyStars.Cards;

/// <summary>
/// 按强化状态解析卡图：强化前用 <c>Foo.png</c>，强化后优先 <c>Foo+.png</c>，没有则共用强化前。
/// </summary>
public static class AlchemyStarsCardArt
{
    private static readonly ConcurrentDictionary<string, bool> ExistsCache = new(StringComparer.Ordinal);

    /// <summary>
    /// 卡牌声明了本 Mod 卡图路径时，按强化状态选择实际使用的路径。
    /// </summary>
    public static bool TryResolvePortraitPath(CardModel card, string currentPath, out string path)
    {
        path = currentPath;
        if (!TryGetBasePortraitPath(card, out var basePath))
            return false;

        if (!ShouldUseUpgradedArt(card))
        {
            path = basePath;
            return !string.Equals(currentPath, basePath, StringComparison.Ordinal);
        }

        if (TryGetExistingUpgradedPortraitPath(card, basePath, out var upgradedPath))
        {
            path = upgradedPath;
            return !string.Equals(currentPath, upgradedPath, StringComparison.Ordinal);
        }

        path = basePath;
        return !string.Equals(currentPath, basePath, StringComparison.Ordinal);
    }

    /// <summary>
    /// 若存在强化后卡图，将其加入预加载列表。
    /// </summary>
    public static IEnumerable<string> AppendUpgradedPortraitPath(CardModel card, IEnumerable<string> current)
    {
        if (!TryGetBasePortraitPath(card, out var basePath) ||
            !TryGetExistingUpgradedPortraitPath(card, basePath, out var upgradedPath))
            return current;

        var paths = current as ICollection<string> ?? current.ToArray();
        foreach (var existing in paths)
        {
            if (string.Equals(existing, upgradedPath, StringComparison.Ordinal))
                return paths;
        }

        return paths.Append(upgradedPath);
    }

    public static void ReloadIfValid(NCard card)
    {
        if (!GodotObject.IsInstanceValid(card) || !card.IsNodeReady())
            return;

        card.Call(NCard.MethodName.Reload);
    }

    private static bool ShouldUseUpgradedArt(CardModel card) => card.IsUpgraded;

    private static bool TryGetBasePortraitPath(CardModel card, out string basePath)
    {
        basePath = "";
        if (card is not IModCardAssetOverrides overrides)
            return false;

        if (card is IUpgradedCardArt custom &&
            !string.IsNullOrWhiteSpace(custom.BasePortraitPath))
        {
            basePath = custom.BasePortraitPath;
            return true;
        }

        var declared = overrides.CustomPortraitPath;
        if (string.IsNullOrWhiteSpace(declared) ||
            !declared.StartsWith(Entry.ResPath, StringComparison.Ordinal))
            return false;

        basePath = declared;
        return true;
    }

    private static bool TryGetExistingUpgradedPortraitPath(CardModel card, string basePath, out string upgradedPath)
    {
        foreach (var candidate in EnumerateUpgradedPortraitCandidates(card, basePath))
        {
            if (!ResourceExists(candidate))
                continue;

            upgradedPath = candidate;
            return true;
        }

        upgradedPath = "";
        return false;
    }

    private static IEnumerable<string> EnumerateUpgradedPortraitCandidates(CardModel card, string basePath)
    {
        if (card is IUpgradedCardArt custom &&
            !string.IsNullOrWhiteSpace(custom.UpgradedPortraitPath))
            yield return custom.UpgradedPortraitPath;

        var derived = ToUpgradedPortraitPath(basePath);
        if (!string.IsNullOrWhiteSpace(derived) &&
            !string.Equals(derived, basePath, StringComparison.Ordinal))
            yield return derived;
    }

    /// <summary>
    /// <c>res://AlchemyStars/images/cards/Foo.png</c> → <c>res://AlchemyStars/images/cards/Foo+.png</c>
    /// </summary>
    public static string ToUpgradedPortraitPath(string basePath)
    {
        var slash = basePath.LastIndexOf('/');
        var fileStart = slash >= 0 ? slash + 1 : 0;
        var fileName = basePath[fileStart..];
        var directory = slash >= 0 ? basePath[..fileStart] : "";

        var dot = fileName.LastIndexOf('.');
        if (dot <= 0)
            return basePath + "+";

        var stem = fileName[..dot];
        if (stem.EndsWith('+'))
            return basePath;

        return directory + stem + "+" + fileName[dot..];
    }

    private static bool ResourceExists(string path) =>
        ExistsCache.GetOrAdd(path, static candidate => GodotResourcePath.ResourceExists(candidate));
}

/// <summary>
/// 可选：单独指定强化前 / 强化后卡图。未实现时按 <see cref="AlchemyStarsCardArt"/> 约定从 <c>PortraitPath</c> 推导。
/// </summary>
public interface IUpgradedCardArt
{
    string? BasePortraitPath => null;

    string? UpgradedPortraitPath => null;
}
