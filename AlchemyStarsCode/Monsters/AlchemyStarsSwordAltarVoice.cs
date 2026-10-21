using System.Reflection;
using AlchemyStars.Settings;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Vfx;

namespace AlchemyStars.Monsters;

/// <summary>播放祭剑座语音。语言由模组选项决定，默认中文。</summary>
public static class AlchemyStarsSwordAltarVoice
{
    private const string SourceRoot = @"H:\STS2MOD\白夜极光\卡图\祭剑座语音&旧印系列能力图标";

    private static AudioStreamPlayer? _player;

    public static void Play(Creature speaker, string clipId)
    {
        if (NonInteractiveMode.IsActive || string.IsNullOrWhiteSpace(clipId) || speaker.IsDead)
            return;

        ShowLine(speaker, clipId);

        try
        {
            var path = ResolvePath(clipId);
            if (path == null)
            {
                Entry.Logger.Warn($"找不到祭剑座语音 {clipId}。");
                return;
            }

            var stream = AudioStreamWav.LoadFromFile(path);
            if (stream == null)
                return;

            if (Engine.GetMainLoop() is not SceneTree tree)
                return;

            StopCurrent();
            var player = new AudioStreamPlayer
            {
                Stream = stream,
                Bus = "Master",
            };
            player.Finished += () => Release(player);
            tree.Root.AddChild(player);
            _player = player;
            player.Play();
        }
        catch (Exception exception)
        {
            Entry.Logger.Warn($"播放祭剑座语音 {clipId} 失败：{exception.Message}");
        }
    }

    private static void ShowLine(Creature speaker, string clipId)
    {
        var entry = speaker.Monster?.Id.Entry;
        if (string.IsNullOrEmpty(entry))
            return;

        var line = MonsterModel.L10NMonsterLookup(entry + ".talk." + clipId);
        TalkCmd.Play(line, speaker, VfxColor.White);
    }

    private static void StopCurrent()
    {
        if (_player == null || !GodotObject.IsInstanceValid(_player))
        {
            _player = null;
            return;
        }

        var current = _player;
        _player = null;
        current.Stop();
        current.QueueFree();
    }

    private static void Release(AudioStreamPlayer player)
    {
        if (ReferenceEquals(_player, player))
            _player = null;

        if (GodotObject.IsInstanceValid(player))
            player.QueueFree();
    }

    private static string? ResolvePath(string clipId)
    {
        var language = AlchemyStarsModSettingsStore.GetVoiceLanguage();
        var bundledFolder = language == AlchemyStarsVoiceLanguage.Japanese ? "jpn" : "zhs";
        var sourceFolder = language == AlchemyStarsVoiceLanguage.Japanese ? "日语" : "中文";
        var fileName = clipId + ".wav";

        var assemblyDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        var candidates = new[]
        {
            Path.Combine(SourceRoot, sourceFolder, fileName),
            assemblyDir == null ? null : Path.Combine(assemblyDir, "audio", "sword_altar", bundledFolder, fileName),
            ProjectSettings.GlobalizePath($"{Entry.ResPath}/audio/sword_altar/{bundledFolder}/{fileName}"),
        };

        foreach (var candidate in candidates)
        {
            if (!string.IsNullOrWhiteSpace(candidate) && File.Exists(candidate))
                return candidate;
        }

        return null;
    }
}
