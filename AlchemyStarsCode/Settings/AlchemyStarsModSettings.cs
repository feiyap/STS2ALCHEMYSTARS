using STS2RitsuLib;
using STS2RitsuLib.Settings;
using STS2RitsuLib.Utils.Persistence;

namespace AlchemyStars.Settings;

/// <summary>祭剑座语音语言。默认中文。</summary>
public enum AlchemyStarsVoiceLanguage
{
    Chinese = 0,
    Japanese = 1,
}

/// <summary>模组全局选项。</summary>
public sealed class AlchemyStarsModSettings
{
    public AlchemyStarsVoiceLanguage VoiceLanguage { get; set; } = AlchemyStarsVoiceLanguage.Chinese;
}

/// <summary>模组选项的存档与设置页。</summary>
public static class AlchemyStarsModSettingsStore
{
    public const string Key = "settings";
    public const string FileName = "settings.json";

    public static void Register()
    {
        using (RitsuLibFramework.BeginModDataRegistration(Entry.ModId))
        {
            RitsuLibFramework.GetDataStore(Entry.ModId).Register<AlchemyStarsModSettings>(
                Key,
                FileName,
                SaveScope.Global,
                () => new(),
                autoCreateIfMissing: true);
        }

        RitsuLibFramework.RegisterModSettings(Entry.ModId, page => page
            .WithTitle(ModSettingsText.Literal("祭剑座语音"))
            .WithModDisplayName(ModSettingsText.Literal("空裔"))
            .AddSection("voice", section => section
                .WithTitle(ModSettingsText.Literal("语音"))
                .AddEnumChoice(
                    "voice_language",
                    ModSettingsText.Literal("语音语言"),
                    new ModSettingsValueBinding<AlchemyStarsModSettings, AlchemyStarsVoiceLanguage>(
                        Entry.ModId,
                        Key,
                        SaveScope.Global,
                        settings => settings.VoiceLanguage,
                        (settings, value) => settings.VoiceLanguage = value),
                    value => ModSettingsText.Literal(value == AlchemyStarsVoiceLanguage.Japanese ? "日文" : "中文"),
                    ModSettingsText.Literal("祭剑座语音使用的语言。"),
                    ModSettingsChoicePresentation.Dropdown)));
    }

    public static AlchemyStarsVoiceLanguage GetVoiceLanguage()
    {
        try
        {
            return RitsuLibFramework.GetDataStore(Entry.ModId)
                .Get<AlchemyStarsModSettings>(Key)
                .VoiceLanguage;
        }
        catch (Exception)
        {
            return AlchemyStarsVoiceLanguage.Chinese;
        }
    }
}
