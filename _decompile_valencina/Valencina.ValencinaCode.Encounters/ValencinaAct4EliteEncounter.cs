using System.Collections.Generic;
using System.Linq;
using Godot;
using MegaCrit.Sts2.Core.Entities.Encounters;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Rooms;
using STS2RitsuLib.Scaffolding.Content;
using Valencina.ValencinaCode.Acts;
using Valencina.ValencinaCode.Monsters;

namespace Valencina.ValencinaCode.Encounters;

public sealed class ValencinaAct4EliteEncounter : ModEncounterTemplate
{
	public override RoomType RoomType => (RoomType)2;

	public override IEnumerable<EncounterTag> Tags => new _003C_003Ez__ReadOnlySingleElementList<EncounterTag>((EncounterTag)10);

	public override IEnumerable<MonsterModel> AllPossibleMonsters => new _003C_003Ez__ReadOnlyArray<MonsterModel>((MonsterModel[])(object)new MonsterModel[3]
	{
		(MonsterModel)ModelDb.Monster<Act4EliteRodya>(),
		(MonsterModel)ModelDb.Monster<Act4EliteHeathcliff>(),
		(MonsterModel)ModelDb.Monster<Act4EliteGregor>()
	});

	public override IReadOnlyList<string> Slots => new _003C_003Ez__ReadOnlyArray<string>(new string[3] { "first", "second", "third" });

	public override EncounterAssetProfile AssetProfile => new EncounterAssetProfile("res://Valencina/scenes/encounters/act4_elite_background.tscn", (string)null, (string)null, (string)null, Act4EliteAssets.AllAssetPaths.ToArray(), (string[])null, (string)null, (string)null);

	protected override bool UseProgrammaticCombatBackground => true;

	protected override BackgroundAssets BuildProgrammaticCombatBackground(ActModel parentAct, Rng rng)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Expected O, but got Unknown
		return new BackgroundAssets("valencina-act4_elite_encounter", rng);
	}

	public override float GetCameraScaling()
	{
		return 0.9f;
	}

	public override Vector2 GetCameraOffset()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		return Vector2.Down * 50f;
	}

	public override bool IsValidForAct(ActModel act)
	{
		return act is ValencinaAct4;
	}

	protected override IReadOnlyList<(MonsterModel, string?)> GenerateMonsters()
	{
		return new _003C_003Ez__ReadOnlyArray<(MonsterModel, string)>(new(MonsterModel, string)[3]
		{
			(((MonsterModel)ModelDb.Monster<Act4EliteRodya>()).ToMutable(), "first"),
			(((MonsterModel)ModelDb.Monster<Act4EliteHeathcliff>()).ToMutable(), "second"),
			(((MonsterModel)ModelDb.Monster<Act4EliteGregor>()).ToMutable(), "third")
		});
	}
}
