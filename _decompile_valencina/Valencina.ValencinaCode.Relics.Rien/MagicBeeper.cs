using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Map;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves.Runs;
using Valencina.ValencinaCode.Compat;

namespace Valencina.ValencinaCode.Relics.Rien;

public sealed class MagicBeeper : RienRelic
{
	private const string CombatsKey = "Combats";

	private const string StrengthKey = "Strength";

	private const string DexterityKey = "Dexterity";

	private int _markedActIndex = -1;

	private bool _currentCombatMarked;

	private bool _rewardsAddedThisCombat;

	protected override IEnumerable<DynamicVar> CanonicalVars => (IEnumerable<DynamicVar>)(object)new DynamicVar[3]
	{
		new DynamicVar("Combats", 12m),
		(DynamicVar)new PowerVar<StrengthPower>("Strength", 3m),
		(DynamicVar)new PowerVar<DexterityPower>("Dexterity", 3m)
	};

	[SavedProperty]
	public int MarkedActIndex
	{
		get
		{
			return _markedActIndex;
		}
		set
		{
			((AbstractModel)this).AssertMutable();
			_markedActIndex = value;
		}
	}

	[SavedProperty]
	private int[] MarkedCoordCols { get; set; } = Array.Empty<int>();

	[SavedProperty]
	private int[] MarkedCoordRows { get; set; } = Array.Empty<int>();

	[SavedProperty]
	private bool MarkedCoordsSet { get; set; }

	public override Task AfterObtained()
	{
		MarkedActIndex = ((RelicModel)this).Owner.RunState.CurrentActIndex;
		AddMarkedRooms(((RelicModel)this).Owner.RunState.Map);
		return Task.CompletedTask;
	}

	public override ActMap ModifyGeneratedMapLate(IRunState runState, ActMap map, int actIndex)
	{
		return AddMarkedRooms(map);
	}

	private ActMap AddMarkedRooms(ActMap map)
	{
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Expected O, but got Unknown
		//IL_01d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01de: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ed: Unknown result type (might be due to invalid IL or missing references)
		if (((RelicModel)this).Owner.RunState.CurrentActIndex != MarkedActIndex)
		{
			return map;
		}
		List<MapCoord> markedCoords = GetMarkedCoords();
		bool flag = markedCoords == null;
		if (markedCoords != null)
		{
			flag = !markedCoords.TrueForAll(delegate(MapCoord coord)
			{
				//IL_0006: Unknown result type (might be due to invalid IL or missing references)
				//IL_0016: Unknown result type (might be due to invalid IL or missing references)
				//IL_0021: Unknown result type (might be due to invalid IL or missing references)
				//IL_0027: Invalid comparison between Unknown and I4
				//IL_002a: Unknown result type (might be due to invalid IL or missing references)
				//IL_0030: Invalid comparison between Unknown and I4
				//IL_0033: Unknown result type (might be due to invalid IL or missing references)
				//IL_0039: Invalid comparison between Unknown and I4
				if (!map.HasPoint(coord))
				{
					return false;
				}
				MapPoint point2 = map.GetPoint(coord);
				return point2 != null && ((int)point2.PointType == 5 || (int)point2.PointType == 6 || (int)point2.PointType == 7);
			});
		}
		if (flag)
		{
			Rng val = new Rng((uint)((int)((RelicModel)this).Owner.RunState.Rng.Seed + (int)((RelicModel)this).Owner.NetId + StringHelper.GetDeterministicHashCode("ValencinaMagicBeeper")), 0);
			List<MapPoint> list = (from val2 in map.GetAllMapPoints()
				where ((int)val2.PointType == 5 || (int)val2.PointType == 6) && !val2.Quests.Any((AbstractModel q) => q is MagicBeeper)
				select val2).ToList();
			ListExtensions.UnstableShuffle<MapPoint>(list, val);
			List<MapPoint> marked = list.Take(((RelicModel)this).DynamicVars["Combats"].IntValue).ToList();
			marked.AddRange(from val2 in map.GetAllMapPoints()
				where (int)val2.PointType == 7 && !val2.Quests.Any((AbstractModel q) => q is MagicBeeper) && !marked.Contains(val2)
				select val2);
			MarkedCoordCols = new int[marked.Count];
			MarkedCoordRows = new int[marked.Count];
			for (int num = 0; num < marked.Count; num++)
			{
				MarkedCoordCols[num] = marked[num].coord.col;
				MarkedCoordRows[num] = marked[num].coord.row;
				marked[num].AddQuest((AbstractModel)(object)this);
			}
			MarkedCoordsSet = true;
		}
		else if (markedCoords != null)
		{
			foreach (MapCoord item in markedCoords)
			{
				if (map.HasPoint(item))
				{
					MapPoint point = map.GetPoint(item);
					if (point != null)
					{
						point.AddQuest((AbstractModel)(object)this);
					}
				}
			}
		}
		return map;
	}

	public override async Task BeforeCombatStart()
	{
		_currentCombatMarked = false;
		_rewardsAddedThisCombat = false;
		if (!IsCurrentMapPointMarked())
		{
			return;
		}
		_currentCombatMarked = true;
		((RelicModel)this).Flash();
		foreach (Player player in ((IPlayerCollection)((RelicModel)this).Owner.RunState).Players)
		{
			Creature creature = player.Creature;
			if (creature.IsAlive)
			{
				await CompatPowerCmd.Apply<StrengthPower>((PlayerChoiceContext)new BlockingPlayerChoiceContext(), creature, ((RelicModel)this).DynamicVars["Strength"].BaseValue, ((RelicModel)this).Owner.Creature, (CardModel?)null, silent: false);
				await CompatPowerCmd.Apply<DexterityPower>((PlayerChoiceContext)new BlockingPlayerChoiceContext(), creature, ((RelicModel)this).DynamicVars["Dexterity"].BaseValue, ((RelicModel)this).Owner.Creature, (CardModel?)null, silent: false);
			}
		}
	}

	public override bool TryModifyRewards(Player player, List<Reward> rewards, AbstractRoom? room)
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Expected O, but got Unknown
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Expected O, but got Unknown
		if (player != ((RelicModel)this).Owner || _rewardsAddedThisCombat)
		{
			return false;
		}
		if (!_currentCombatMarked && !IsCurrentMapPointMarked())
		{
			return false;
		}
		if (room == null || !RoomTypeExtensions.IsCombatRoom(room.RoomType))
		{
			return false;
		}
		((RelicModel)this).Flash();
		rewards.Add((Reward)new GoldReward(25, player, false));
		rewards.Add((Reward)new CardReward(CardCreationOptions.ForRoom(player, room.RoomType), 3, player, (PlayerChoiceSynchronizer)null));
		_rewardsAddedThisCombat = true;
		return true;
	}

	private bool IsCurrentMapPointMarked()
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		List<MapCoord> markedCoords = GetMarkedCoords();
		if (markedCoords != null && ((RelicModel)this).Owner.RunState.CurrentMapPoint != null)
		{
			return markedCoords.Contains(((RelicModel)this).Owner.RunState.CurrentMapPoint.coord);
		}
		return false;
	}

	public List<MapCoord>? GetMarkedCoords()
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		if (!MarkedCoordsSet || MarkedCoordCols.Length != MarkedCoordRows.Length)
		{
			return null;
		}
		List<MapCoord> list = new List<MapCoord>();
		for (int i = 0; i < MarkedCoordCols.Length; i++)
		{
			list.Add(new MapCoord
			{
				col = MarkedCoordCols[i],
				row = MarkedCoordRows[i]
			});
		}
		return list;
	}
}
