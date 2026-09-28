using System.Collections.Generic;
using System.Linq;
using System.Text;
using AlchemyStars.Characters;
using HarmonyLib;
using MegaCrit.Sts2.Core.Map;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.Saves.Runs;
using STS2RitsuLib.Interop.AutoRegistration;

namespace AlchemyStars.Relics.Ancients;

/// <summary>
/// 顺心罗盘：地图精英/Boss 旁显示遭遇名，并按坐标固定精英遭遇。
/// </summary>
[RegisterRelic(typeof(AlchemyStarsRelicPool))]
public sealed class AlchemyStarsSmoothCompass : AlchemyStarsAncientRelicBase
{
    private string _assignmentBlob = "";

    public override bool HasUponPickupEffect => true;

    /// <summary>
    /// 序列化：每行 <c>actIndex:col,row=EncounterEntry</c>。
    /// </summary>
    [SavedProperty]
    public string AssignmentBlob
    {
        get => _assignmentBlob;
        set
        {
            AssertMutable();
            _assignmentBlob = value ?? "";
        }
    }

    public override Task AfterObtained()
    {
        Flash();
        if (Owner?.RunState != null)
            EnsureAssignments(Owner.RunState);
        return Task.CompletedTask;
    }

    public static AlchemyStarsSmoothCompass? FindActive(IRunState? runState)
    {
        if (runState == null)
            return null;

        foreach (var player in runState.Players)
        {
            var relic = player.GetRelic<AlchemyStarsSmoothCompass>();
            if (relic != null)
                return relic;
        }

        return null;
    }

    public void EnsureAssignments(IRunState runState)
    {
        var map = runState.Map;
        var act = runState.Act;
        var roomSet = GetRoomSet(act);
        if (roomSet == null || map == null)
            return;

        var elitePoints = map.GetAllMapPoints()
            .Where(p => p.PointType == MapPointType.Elite)
            .OrderBy(p => p.coord.row)
            .ThenBy(p => p.coord.col)
            .ToList();

        var elites = roomSet.eliteEncounters;
        if (elites.Count == 0 || elitePoints.Count == 0)
            return;

        var dict = ParseAssignments();
        var actIndex = runState.CurrentActIndex;
        var start = Math.Max(0, roomSet.eliteEncountersVisited);
        var changed = false;

        for (var i = 0; i < elitePoints.Count; i++)
        {
            var key = MakeKey(actIndex, elitePoints[i].coord);
            if (dict.ContainsKey(key))
                continue;

            var encounter = elites[(start + i) % elites.Count];
            dict[key] = encounter.Id.ToString();
            changed = true;
        }

        if (changed)
            AssignmentBlob = SerializeAssignments(dict);
    }

    public bool TryGetEliteEncounter(IRunState runState, MapCoord coord, out EncounterModel? encounter)
    {
        encounter = null;
        EnsureAssignments(runState);
        var key = MakeKey(runState.CurrentActIndex, coord);
        if (!ParseAssignments().TryGetValue(key, out var idText))
            return false;

        try
        {
            encounter = SaveUtil.EncounterOrDeprecated(ModelId.Deserialize(idText));
        }
        catch
        {
            encounter = null;
        }

        return encounter != null && encounter.GetType().Name != "DeprecatedEncounter";
    }

    public string? GetLabelForPoint(IRunState runState, MapPoint point)
    {
        if (point.PointType == MapPointType.Boss)
        {
            var enc = point == runState.Map.SecondBossMapPoint
                ? runState.Act.SecondBossEncounter
                : runState.Act.BossEncounter;
            return enc?.Title.GetRawText();
        }

        if (point.PointType != MapPointType.Elite)
            return null;

        if (!TryGetEliteEncounter(runState, point.coord, out var elite) || elite == null)
            return null;

        return elite.Title.GetRawText();
    }

    private static string MakeKey(int actIndex, MapCoord coord) =>
        $"{actIndex}:{coord.col},{coord.row}";

    private Dictionary<string, string> ParseAssignments()
    {
        var dict = new Dictionary<string, string>(StringComparer.Ordinal);
        if (string.IsNullOrWhiteSpace(AssignmentBlob))
            return dict;

        foreach (var line in AssignmentBlob.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var eq = line.IndexOf('=');
            if (eq <= 0)
                continue;
            dict[line[..eq]] = line[(eq + 1)..];
        }

        return dict;
    }

    private static string SerializeAssignments(Dictionary<string, string> dict)
    {
        var sb = new StringBuilder();
        foreach (var (k, v) in dict.OrderBy(kv => kv.Key, StringComparer.Ordinal))
            sb.Append(k).Append('=').Append(v).Append('\n');
        return sb.ToString();
    }

    internal static RoomSet? GetRoomSet(ActModel act)
    {
        var field = AccessTools.Field(typeof(ActModel), "_rooms");
        return field?.GetValue(act) as RoomSet;
    }
}
