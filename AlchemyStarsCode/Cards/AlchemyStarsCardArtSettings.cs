using STS2RitsuLib;
using STS2RitsuLib.Data;
using STS2RitsuLib.Utils.Persistence;

namespace AlchemyStars.Cards;

/// <summary>
/// 跨局保存的卡牌异画选择。键为卡牌类型名，值为异画 id；缺省表示使用默认卡图。
/// </summary>
public sealed class AlchemyStarsCardArtSettings
{
    public Dictionary<string, string> SelectedAlternateArt { get; set; } = new(StringComparer.Ordinal);
}

/// <summary>
/// 全局存档中的异画选择读写。
/// </summary>
public static class AlchemyStarsCardArtSettingsStore
{
    public const string Key = "card_art";
    public const string FileName = "card_art.json";

    public static void Register()
    {
        using (RitsuLibFramework.BeginModDataRegistration(Entry.ModId))
        {
            RitsuLibFramework.GetDataStore(Entry.ModId).Register<AlchemyStarsCardArtSettings>(
                Key,
                FileName,
                SaveScope.Global,
                () => new(),
                autoCreateIfMissing: true);
        }
    }

    public static string? GetSelectedAlternateArtId(string cardTypeName)
    {
        if (string.IsNullOrWhiteSpace(cardTypeName))
            return null;

        try
        {
            var settings = RitsuLibFramework.GetDataStore(Entry.ModId).Get<AlchemyStarsCardArtSettings>(Key);
            if (!settings.SelectedAlternateArt.TryGetValue(cardTypeName, out var id) ||
                string.IsNullOrWhiteSpace(id))
                return null;

            foreach (var art in AlchemyStarsCardArt.GetAlternateArts(cardTypeName))
            {
                if (string.Equals(art.Id, id, StringComparison.Ordinal))
                    return id;
            }

            return null;
        }
        catch (Exception)
        {
            return null;
        }
    }

    public static void SetSelectedAlternateArtId(string cardTypeName, string? alternateArtId)
    {
        var store = RitsuLibFramework.GetDataStore(Entry.ModId);
        store.Modify<AlchemyStarsCardArtSettings>(Key, settings =>
        {
            if (string.IsNullOrWhiteSpace(alternateArtId))
                settings.SelectedAlternateArt.Remove(cardTypeName);
            else
                settings.SelectedAlternateArt[cardTypeName] = alternateArtId;
        });
        store.Save(Key);
    }
}
