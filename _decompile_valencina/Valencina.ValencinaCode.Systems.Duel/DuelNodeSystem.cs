using System.Collections.Generic;
using System.Linq;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Map;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Runs;
using Valencina.ValencinaCode.Character;
using Valencina.ValencinaCode.Relics.Rien;

namespace Valencina.ValencinaCode.Systems.Duel;

internal static class DuelNodeSystem
{
	internal const int DuelNodesPerAct = 0;

	internal const int CardsPerTurnLimit = 6;

	internal const int StrengthPerTurn = 3;

	internal const int VictoryGold = 75;

	internal const string MapIconPath = "res://Valencina/images/ui/map/duel_node.svg";

	private const string RngKey = "ValencinaDuelNodes";

	internal static ActMap ApplyDuelNodes(IRunState runState, ActMap map, int actIndex)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Invalid comparison between Unknown and I4
		foreach (MapPoint duelPoint in GetDuelPoints(runState, map, actIndex))
		{
			MapPointType pointType = duelPoint.PointType;
			if (pointType - 7 > 1)
			{
				duelPoint.PointType = (MapPointType)1;
				duelPoint.CanBeModified = false;
			}
		}
		return map;
	}

	internal static bool IsDuelPoint(IRunState? runState, MapCoord? coord = null)
	{
		return false;
	}

	internal static IReadOnlyList<MapCoord> GetDuelCoords(IRunState runState, ActMap map, int actIndex)
	{
		return (from point in GetDuelPoints(runState, map, actIndex)
			select point.coord into coord
			orderby coord.row, coord.col
			select coord).ToArray();
	}

	internal static bool IsValencina(Player? player)
	{
		return ((player != null) ? player.Character : null) is Valencina.ValencinaCode.Character.Valencina;
	}

	internal static RelicModel? CreateValencinaAncientReward(Player player)
	{
		if (!IsValencina(player))
		{
			return null;
		}
		using (List<ExtraAncientPoolEntry>.Enumerator enumerator = ValencinaExtraAncientRelicPools.DrawOptions(player, player.RunState.CurrentActIndex, 1).GetEnumerator())
		{
			if (enumerator.MoveNext())
			{
				RelicModel obj = enumerator.Current.CreateCanonical().ToMutable();
				obj.Owner = player;
				return obj;
			}
		}
		return null;
	}

	private static IReadOnlyList<MapPoint> GetDuelPoints(IRunState runState, ActMap map, int actIndex)
	{
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Expected O, but got Unknown
		List<MapPoint> list = (from point in (from point in map.GetAllMapPoints()
				where point.coord.row > 1
				where point.coord.row < map.GetRowCount() - 1
				select point).Where(delegate(MapPoint point)
			{
				//IL_0001: Unknown result type (might be due to invalid IL or missing references)
				//IL_0006: Unknown result type (might be due to invalid IL or missing references)
				//IL_0007: Unknown result type (might be due to invalid IL or missing references)
				//IL_0009: Invalid comparison between Unknown and I4
				//IL_000b: Unknown result type (might be due to invalid IL or missing references)
				//IL_000d: Invalid comparison between Unknown and I4
				//IL_000f: Unknown result type (might be due to invalid IL or missing references)
				//IL_0011: Invalid comparison between Unknown and I4
				MapPointType pointType = point.PointType;
				return (int)pointType != 7 && (int)pointType != 8 && (int)pointType != 4;
			})
			orderby point.coord.row, point.coord.col
			select point).ToList();
		if (list.Count <= 0)
		{
			return list;
		}
		Rng val = new Rng((uint)((int)runState.Rng.Seed + (actIndex + 1) * 719 + StringHelper.GetDeterministicHashCode("ValencinaDuelNodes")), 0);
		ListExtensions.StableShuffle<MapPoint>(list, val);
		return (from point in list.Take(0)
			orderby point.coord.row, point.coord.col
			select point).ToArray();
	}
}
