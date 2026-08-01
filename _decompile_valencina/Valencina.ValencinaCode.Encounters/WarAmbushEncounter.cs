using System.Collections.Generic;
using System.Linq;
using Godot;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Rooms;
using STS2RitsuLib.Scaffolding.Content;
using Valencina.ValencinaCode.Monsters;

namespace Valencina.ValencinaCode.Encounters;

public sealed class WarAmbushEncounter : ModEncounterTemplate
{
	public const string SlotScene = "res://Valencina/scenes/encounters/war_ambush_slots.tscn";

	public const string BackgroundTitle = "valencina-war_ambush_encounter";

	public const string BackgroundScene = "res://scenes/backgrounds/valencina-war_ambush_encounter/valencina-war_ambush_encounter_background.tscn";

	public const string BackgroundLayerA = "res://scenes/backgrounds/valencina-war_ambush_encounter/layers/valencina-war_ambush_encounter_bg_00_a.tscn";

	public const string BackgroundLayerB = "res://scenes/backgrounds/valencina-war_ambush_encounter/layers/valencina-war_ambush_encounter_bg_00_b.tscn";

	public const string BackgroundTextureA = "res://Valencina/images/monsters/war_ambush/Battle_smokewar.webp";

	public const string BackgroundTextureB = "res://Valencina/images/monsters/war_ambush/Battle_smokewar_v1.webp";

	private static readonly string[] BackgroundAssetPaths = new string[5] { "res://scenes/backgrounds/valencina-war_ambush_encounter/valencina-war_ambush_encounter_background.tscn", "res://scenes/backgrounds/valencina-war_ambush_encounter/layers/valencina-war_ambush_encounter_bg_00_a.tscn", "res://scenes/backgrounds/valencina-war_ambush_encounter/layers/valencina-war_ambush_encounter_bg_00_b.tscn", "res://Valencina/images/monsters/war_ambush/Battle_smokewar.webp", "res://Valencina/images/monsters/war_ambush/Battle_smokewar_v1.webp" };

	public override RoomType RoomType => (RoomType)1;

	public override bool ShouldGiveRewards => true;

	public override IReadOnlyList<string> Slots => new _003C_003Ez__ReadOnlyArray<string>(new string[14]
	{
		"wriggler1", "wriggler2", "wriggler3", "wriggler4", "odd", "even", "first", "middle", "last", "second",
		"third", "fourth", "hopper", "single"
	});

	public override IEnumerable<MonsterModel> AllPossibleMonsters => new _003C_003Ez__ReadOnlyArray<MonsterModel>((MonsterModel[])(object)new MonsterModel[14]
	{
		(MonsterModel)ModelDb.Monster<Wriggler>(),
		(MonsterModel)ModelDb.Monster<BowlbugEgg>(),
		(MonsterModel)ModelDb.Monster<BowlbugNectar>(),
		(MonsterModel)ModelDb.Monster<BowlbugRock>(),
		(MonsterModel)ModelDb.Monster<BowlbugSilk>(),
		(MonsterModel)ModelDb.Monster<Exoskeleton>(),
		(MonsterModel)ModelDb.Monster<Myte>(),
		(MonsterModel)ModelDb.Monster<ThievingHopper>(),
		(MonsterModel)ModelDb.Monster<ShrinkerBeetle>(),
		(MonsterModel)ModelDb.Monster<FuzzyWurmCrawler>(),
		(MonsterModel)ModelDb.Monster<GCompanySoldierOne>(),
		(MonsterModel)ModelDb.Monster<GCompanySoldierTwo>(),
		(MonsterModel)ModelDb.Monster<GCompanySoldierThree>(),
		(MonsterModel)ModelDb.Monster<GCompanyMinister>()
	});

	public override EncounterAssetProfile AssetProfile => new EncounterAssetProfile("res://Valencina/scenes/encounters/war_ambush_slots.tscn", (string)null, (string)null, (string)null, BackgroundAssetPaths.Concat(GCompanyAmbushAssets.AllAssetPaths).Distinct().ToArray(), (string[])null, (string)null, (string)null);

	protected override bool UseProgrammaticCombatBackground => true;

	protected override BackgroundAssets BuildProgrammaticCombatBackground(ActModel parentAct, Rng rng)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Expected O, but got Unknown
		return new BackgroundAssets("valencina-war_ambush_encounter", rng);
	}

	public override bool IsValidForAct(ActModel act)
	{
		return false;
	}

	public override float GetCameraScaling()
	{
		if (!IsMyteFormation())
		{
			return ((EncounterModel)this).GetCameraScaling();
		}
		return 0.9f;
	}

	public override Vector2 GetCameraOffset()
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		if (!IsMyteFormation())
		{
			return ((EncounterModel)this).GetCameraOffset();
		}
		return Vector2.Down * 50f;
	}

	protected override IReadOnlyList<(MonsterModel, string?)> GenerateMonsters()
	{
		return ((EncounterModel)this).Rng.NextInt(10) switch
		{
			0 => FourWrigglers(), 
			1 => WeakBowlbugs(), 
			2 => NormalBowlbugs(), 
			3 => Exoskeletons(3), 
			4 => Exoskeletons(4), 
			5 => new _003C_003Ez__ReadOnlyArray<(MonsterModel, string)>(new(MonsterModel, string)[2]
			{
				(((MonsterModel)ModelDb.Monster<Myte>()).ToMutable(), "first"),
				(((MonsterModel)ModelDb.Monster<Myte>()).ToMutable(), "second")
			}), 
			6 => new _003C_003Ez__ReadOnlySingleElementList<(MonsterModel, string)>((((MonsterModel)ModelDb.Monster<ThievingHopper>()).ToMutable(), "hopper")), 
			7 => new _003C_003Ez__ReadOnlySingleElementList<(MonsterModel, string)>((((MonsterModel)ModelDb.Monster<ShrinkerBeetle>()).ToMutable(), "single")), 
			8 => new _003C_003Ez__ReadOnlySingleElementList<(MonsterModel, string)>((((MonsterModel)ModelDb.Monster<FuzzyWurmCrawler>()).ToMutable(), "single")), 
			_ => GCompanySquad(), 
		};
	}

	private IReadOnlyList<(MonsterModel, string?)> GCompanySquad()
	{
		List<MonsterModel> list = new List<MonsterModel>
		{
			(MonsterModel)(object)ModelDb.Monster<GCompanySoldierOne>(),
			(MonsterModel)(object)ModelDb.Monster<GCompanySoldierTwo>(),
			(MonsterModel)(object)ModelDb.Monster<GCompanySoldierThree>()
		};
		MonsterModel val = list[((EncounterModel)this).Rng.NextInt(list.Count)];
		list.Remove(val);
		MonsterModel val2 = list[((EncounterModel)this).Rng.NextInt(list.Count)];
		return new _003C_003Ez__ReadOnlyArray<(MonsterModel, string)>(new(MonsterModel, string)[3]
		{
			(val.ToMutable(), "first"),
			(val2.ToMutable(), "middle"),
			(((MonsterModel)ModelDb.Monster<GCompanyMinister>()).ToMutable(), "last")
		});
	}

	private static IReadOnlyList<(MonsterModel, string?)> FourWrigglers()
	{
		return new _003C_003Ez__ReadOnlyArray<(MonsterModel, string)>(new(MonsterModel, string)[4]
		{
			(((MonsterModel)ModelDb.Monster<Wriggler>()).ToMutable(), "wriggler1"),
			(((MonsterModel)ModelDb.Monster<Wriggler>()).ToMutable(), "wriggler2"),
			(((MonsterModel)ModelDb.Monster<Wriggler>()).ToMutable(), "wriggler3"),
			(((MonsterModel)ModelDb.Monster<Wriggler>()).ToMutable(), "wriggler4")
		});
	}

	private IReadOnlyList<(MonsterModel, string?)> WeakBowlbugs()
	{
		MonsterModel item = ((((EncounterModel)this).Rng.NextInt(2) == 0) ? ((MonsterModel)ModelDb.Monster<BowlbugEgg>()).ToMutable() : ((MonsterModel)ModelDb.Monster<BowlbugNectar>()).ToMutable());
		return new _003C_003Ez__ReadOnlyArray<(MonsterModel, string)>(new(MonsterModel, string)[2]
		{
			(((MonsterModel)ModelDb.Monster<BowlbugRock>()).ToMutable(), "odd"),
			(item, "even")
		});
	}

	private IReadOnlyList<(MonsterModel, string?)> NormalBowlbugs()
	{
		List<MonsterModel> list = new List<MonsterModel>
		{
			(MonsterModel)(object)ModelDb.Monster<BowlbugEgg>(),
			(MonsterModel)(object)ModelDb.Monster<BowlbugSilk>(),
			(MonsterModel)(object)ModelDb.Monster<BowlbugNectar>()
		};
		int index = ((EncounterModel)this).Rng.NextInt(list.Count);
		MonsterModel item = list[index].ToMutable();
		list.RemoveAt(index);
		MonsterModel item2 = list[((EncounterModel)this).Rng.NextInt(list.Count)].ToMutable();
		return new _003C_003Ez__ReadOnlyArray<(MonsterModel, string)>(new(MonsterModel, string)[3]
		{
			(((MonsterModel)ModelDb.Monster<BowlbugRock>()).ToMutable(), "first"),
			(item, "middle"),
			(item2, "last")
		});
	}

	private static IReadOnlyList<(MonsterModel, string?)> Exoskeletons(int count)
	{
		string[] array = new string[4] { "first", "second", "third", "fourth" };
		List<(MonsterModel, string)> list = new List<(MonsterModel, string)>(count);
		for (int i = 0; i < count; i++)
		{
			list.Add((((MonsterModel)ModelDb.Monster<Exoskeleton>()).ToMutable(), array[i]));
		}
		return list;
	}

	private bool IsMyteFormation()
	{
		if (((EncounterModel)this).HaveMonstersBeenGenerated)
		{
			return ((EncounterModel)this).MonstersWithSlots.All<(MonsterModel, string)>(((MonsterModel, string) entry) => entry.Item1 is Myte);
		}
		return false;
	}
}
