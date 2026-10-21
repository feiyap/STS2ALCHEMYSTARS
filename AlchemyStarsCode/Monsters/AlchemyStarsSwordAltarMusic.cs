using System.Reflection;
using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Nodes.Audio;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Saves;

namespace AlchemyStars.Monsters;

/// <summary>
/// 祭剑座事件战的专属 BGM。战斗开始时停掉章节音乐并循环播放，战斗结束后交还原版音乐。
/// </summary>
public static class AlchemyStarsSwordAltarMusic
{
    private const string FileName = "hush_watch_your_back.mp3";

    private static AudioStreamPlayer? _player;
    private static bool _active;

    public static void Start()
    {
        if (_active || NonInteractiveMode.IsActive)
            return;

        try
        {
            var path = ResolvePath();
            if (path == null)
            {
                Entry.Logger.Warn("找不到祭剑座战斗 BGM。");
                return;
            }

            var bytes = File.ReadAllBytes(path);
            if (bytes.Length == 0)
                return;

            if (Engine.GetMainLoop() is not SceneTree tree)
                return;

            StopFmodMusic();
            var stream = new AudioStreamMP3
            {
                Data = bytes,
                Loop = true,
            };
            var player = new AudioStreamPlayer
            {
                Stream = stream,
                Bus = "Master",
                VolumeDb = ResolveVolumeDb(),
            };
            tree.Root.AddChild(player);
            _player = player;
            player.Play();
            _active = true;

            if (CombatManager.Instance != null)
                CombatManager.Instance.CombatEnded += OnCombatEnded;
        }
        catch (Exception exception)
        {
            Entry.Logger.Warn($"播放祭剑座战斗 BGM 失败：{exception.Message}");
            StopPlayback();
        }
    }

    private static void OnCombatEnded(CombatRoom _) => Stop();

    public static void Stop()
    {
        if (!_active)
            return;

        _active = false;
        if (CombatManager.Instance != null)
            CombatManager.Instance.CombatEnded -= OnCombatEnded;

        StopPlayback();
        NRunMusicController.Instance?.StopCustomMusic();
    }

    private static void StopPlayback()
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

    /// <summary>只停掉正在播放的章节音乐，不卸载音效库。</summary>
    private static void StopFmodMusic()
    {
        var controller = NRunMusicController.Instance;
        if (controller == null)
            return;

        var proxy = typeof(NRunMusicController)
            .GetField("_proxy", BindingFlags.Instance | BindingFlags.NonPublic)
            ?.GetValue(controller) as GodotObject;
        proxy?.Call("stop_music");
    }

    private static float ResolveVolumeDb()
    {
        var settings = SaveManager.Instance?.SettingsSave;
        var master = settings?.VolumeMaster ?? 0.5f;
        var bgm = settings?.VolumeBgm ?? 0.5f;
        var linear = Mathf.Pow(master, 2f) * Mathf.Pow(bgm, 2f);
        return linear <= 0.0001f ? -80f : Mathf.LinearToDb(linear);
    }

    private static string? ResolvePath()
    {
        var assemblyDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        var candidates = new[]
        {
            assemblyDir == null ? null : Path.Combine(assemblyDir, "audio", "sword_altar", FileName),
            ProjectSettings.GlobalizePath($"{Entry.ResPath}/audio/sword_altar/{FileName}"),
        };

        foreach (var candidate in candidates)
        {
            if (!string.IsNullOrWhiteSpace(candidate) && File.Exists(candidate))
                return candidate;
        }

        return null;
    }
}
