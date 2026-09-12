using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using Godot;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Cards;
using STS2RitsuLib.Scaffolding.Content.Patches;
using STS2RitsuLib.Utils;

namespace AlchemyStars.Cards;

/// <summary>
/// 一张已发现的异画：默认态与可选的强化态路径。
/// </summary>
public readonly record struct AlternateCardArt(string Id, string BasePath, string? UpgradedPath);

/// <summary>
/// 按强化状态与已选异画解析卡图。
/// 默认：<c>Foo.png</c> / <c>Foo+.png</c>；异画：<c>Foo.alt.鎏金秘影.png</c> / <c>Foo.alt.鎏金秘影+.png</c>。
/// </summary>
public static class AlchemyStarsCardArt
{
    public const string CardsDirectory = $"{Entry.ResPath}/images/cards";
    public const string AlternateMarker = ".alt.";

    private static readonly ConcurrentDictionary<string, bool> ExistsCache = new(StringComparer.Ordinal);
    private static readonly ConcurrentDictionary<string, AlternateCardArt[]> AlternateArtsCache = new(StringComparer.Ordinal);
    private static bool _cardImageListingFoundFiles;
    private static bool _loggedCardImageListing;
    private static readonly Regex AlternateFileRegex = new(
        @"^(?<type>.+)\.alt\.(?<id>.+?)(?<plus>\+)?\.png$",
        RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

    private static string? _inspectPreviewTypeName;
    private static string? _inspectPreviewAltId;

    /// <summary>
    /// 图鉴检视中的临时预览。<c>null</c> 表示跟已保存选择；空字符串表示强制默认卡图。
    /// </summary>
    public static void SetInspectPreview(string? cardTypeName, string? alternateArtId)
    {
        _inspectPreviewTypeName = cardTypeName;
        _inspectPreviewAltId = alternateArtId;
    }

    public static void ClearInspectPreview()
    {
        _inspectPreviewTypeName = null;
        _inspectPreviewAltId = null;
    }

    public static bool TryResolvePortraitPath(CardModel card, string currentPath, out string path)
    {
        path = currentPath;
        if (!TryGetDeclaredBasePortraitPath(card, out var declaredBase))
            return false;

        var effectiveBase = ResolveEffectiveBasePath(card, declaredBase);
        var resolved = ShouldUseUpgradedArt(card)
            ? ResolveUpgradedPath(card, effectiveBase)
            : effectiveBase;

        path = resolved;
        return !string.Equals(currentPath, resolved, StringComparison.Ordinal);
    }

    public static IEnumerable<string> AppendUpgradedPortraitPath(CardModel card, IEnumerable<string> current) =>
        AppendExtraPortraitPaths(card, current);

    public static IEnumerable<string> AppendExtraPortraitPaths(CardModel card, IEnumerable<string> current)
    {
        if (!TryGetDeclaredBasePortraitPath(card, out var declaredBase))
            return current;

        var extras = new List<string>();
        AddIfExisting(extras, ToUpgradedPortraitPath(declaredBase));
        foreach (var alt in GetAlternateArts(card))
        {
            AddIfExisting(extras, alt.BasePath);
            if (!string.IsNullOrWhiteSpace(alt.UpgradedPath))
                AddIfExisting(extras, alt.UpgradedPath);
        }

        if (extras.Count == 0)
            return current;

        var paths = current as ICollection<string> ?? current.ToArray();
        var merged = new List<string>(paths);
        foreach (var extra in extras)
        {
            if (merged.Any(existing => string.Equals(existing, extra, StringComparison.Ordinal)))
                continue;
            merged.Add(extra);
        }

        return merged;
    }

    public static IReadOnlyList<AlternateCardArt> GetAlternateArts(CardModel card) =>
        GetAlternateArts(card.GetType().Name);

    public static IReadOnlyList<AlternateCardArt> GetAlternateArts(string cardTypeName)
    {
        if (string.IsNullOrWhiteSpace(cardTypeName))
            return [];

        if (AlternateArtsCache.TryGetValue(cardTypeName, out var cached))
            return cached;

        var scanned = ScanAlternateArts(cardTypeName);
        // 目录还没扫到任何 png 时不缓存空结果，避免启动瞬间把「没有异画」写死。
        if (scanned.Length > 0 || _cardImageListingFoundFiles)
            AlternateArtsCache[cardTypeName] = scanned;
        return scanned;
    }

    public static bool HasAlternateArts(CardModel card) => GetAlternateArts(card).Count > 0;

    public static string GetCardTypeName(CardModel card) => card.GetType().Name;

    public static void ReloadIfValid(NCard card)
    {
        if (!GodotObject.IsInstanceValid(card) || !card.IsNodeReady())
            return;

        card.Call(NCard.MethodName.Reload);
    }

    public static void ReloadDisplayedCards(string cardTypeName)
    {
        if (Engine.GetMainLoop() is not SceneTree tree || tree.Root == null)
            return;

        ReloadDisplayedCards(tree.Root, cardTypeName);
    }

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

    public static bool ResourceExists(string path) =>
        ExistsCache.GetOrAdd(path, static candidate => GodotResourcePath.ResourceExists(candidate));

    private static void ReloadDisplayedCards(Node node, string cardTypeName)
    {
        if (node is NCard nCard &&
            nCard.Model != null &&
            string.Equals(nCard.Model.GetType().Name, cardTypeName, StringComparison.Ordinal))
            ReloadIfValid(nCard);

        foreach (var child in node.GetChildren())
            ReloadDisplayedCards(child, cardTypeName);
    }

    private static bool ShouldUseUpgradedArt(CardModel card) => card.IsUpgraded;

    private static bool TryGetDeclaredBasePortraitPath(CardModel card, out string basePath)
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

    private static string ResolveEffectiveBasePath(CardModel card, string declaredBase)
    {
        var typeName = GetCardTypeName(card);
        var altId = ResolveActiveAlternateArtId(typeName);
        if (string.IsNullOrWhiteSpace(altId))
            return declaredBase;

        foreach (var alt in GetAlternateArts(typeName))
        {
            if (string.Equals(alt.Id, altId, StringComparison.Ordinal))
                return alt.BasePath;
        }

        return declaredBase;
    }

    private static string? ResolveActiveAlternateArtId(string typeName)
    {
        if (string.Equals(_inspectPreviewTypeName, typeName, StringComparison.Ordinal))
            return _inspectPreviewAltId;

        return AlchemyStarsCardArtSettingsStore.GetSelectedAlternateArtId(typeName);
    }

    private static string ResolveUpgradedPath(CardModel card, string effectiveBase)
    {
        var typeName = GetCardTypeName(card);
        var altId = ResolveActiveAlternateArtId(typeName);
        if (!string.IsNullOrWhiteSpace(altId))
        {
            foreach (var alt in GetAlternateArts(typeName))
            {
                if (!string.Equals(alt.Id, altId, StringComparison.Ordinal))
                    continue;

                if (!string.IsNullOrWhiteSpace(alt.UpgradedPath) && ResourceExists(alt.UpgradedPath))
                    return alt.UpgradedPath;
                return alt.BasePath;
            }
        }

        if (card is IUpgradedCardArt custom &&
            !string.IsNullOrWhiteSpace(custom.UpgradedPortraitPath) &&
            ResourceExists(custom.UpgradedPortraitPath))
            return custom.UpgradedPortraitPath;

        var derived = ToUpgradedPortraitPath(effectiveBase);
        return ResourceExists(derived) ? derived : effectiveBase;
    }

    private static AlternateCardArt[] ScanAlternateArts(string cardTypeName)
    {
        var found = new Dictionary<string, (string? Base, string? Upgraded)>(StringComparer.Ordinal);
        foreach (var fileName in ListCardImageFiles())
        {
            var match = AlternateFileRegex.Match(fileName);
            if (!match.Success ||
                !string.Equals(match.Groups["type"].Value, cardTypeName, StringComparison.Ordinal))
                continue;

            var id = match.Groups["id"].Value;
            if (string.IsNullOrWhiteSpace(id))
                continue;

            var path = $"{CardsDirectory}/{fileName}";
            found.TryGetValue(id, out var pair);
            if (match.Groups["plus"].Success)
                pair.Upgraded = path;
            else
                pair.Base = path;
            found[id] = pair;
        }

        return found
            .Select(pair =>
            {
                var basePath = pair.Value.Base ?? pair.Value.Upgraded;
                return string.IsNullOrWhiteSpace(basePath)
                    ? (AlternateCardArt?)null
                    : new AlternateCardArt(pair.Key, basePath, pair.Value.Upgraded);
            })
            .Where(art => art != null)
            .Select(art => art!.Value)
            .OrderBy(art => art.Id, StringComparer.Ordinal)
            .ToArray();
    }

    private static string[] ListCardImageFiles()
    {
        var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        try
        {
            foreach (var entry in ResourceLoader.ListDirectory(CardsDirectory))
                TryAddPngFileName(names, entry);
        }
        catch (Exception ex)
        {
            Entry.Logger.Warn($"罗列卡图目录失败（ResourceLoader）：{ex.Message}");
        }

        try
        {
            foreach (var file in DirAccess.GetFilesAt(CardsDirectory))
                TryAddPngFileName(names, file);
        }
        catch (Exception ex)
        {
            Entry.Logger.Warn($"罗列卡图目录失败（DirAccess）：{ex.Message}");
        }

        try
        {
            var globalPath = ProjectSettings.GlobalizePath(CardsDirectory);
            if (Directory.Exists(globalPath))
            {
                foreach (var path in Directory.GetFiles(globalPath, "*.png"))
                    TryAddPngFileName(names, Path.GetFileName(path));
            }
        }
        catch (Exception ex)
        {
            Entry.Logger.Warn($"罗列卡图目录失败（文件系统）：{ex.Message}");
        }

        _cardImageListingFoundFiles = names.Count > 0;
        if (!_loggedCardImageListing)
        {
            _loggedCardImageListing = true;
            var altCount = names.Count(static name =>
                name.Contains(AlternateMarker, StringComparison.OrdinalIgnoreCase));
            Entry.Logger.Info($"卡图目录扫描到 {names.Count} 个 png，其中异画 {altCount} 个。");
        }

        return names.ToArray();
    }

    private static void TryAddPngFileName(HashSet<string> names, string? entry)
    {
        if (string.IsNullOrWhiteSpace(entry))
            return;

        var fileName = entry.Trim().Replace('\\', '/').TrimEnd('/');
        var slash = fileName.LastIndexOf('/');
        if (slash >= 0)
            fileName = fileName[(slash + 1)..];

        if (fileName.EndsWith(".png", StringComparison.OrdinalIgnoreCase) &&
            !fileName.EndsWith(".import", StringComparison.OrdinalIgnoreCase))
            names.Add(fileName);
    }

    private static void AddIfExisting(List<string> paths, string? path)
    {
        if (!string.IsNullOrWhiteSpace(path) && ResourceExists(path))
            paths.Add(path);
    }
}

/// <summary>
/// 可选：单独指定强化前 / 强化后默认卡图。未实现时按约定从 <c>PortraitPath</c> 推导。
/// </summary>
public interface IUpgradedCardArt
{
    string? BasePortraitPath => null;

    string? UpgradedPortraitPath => null;
}
