using System.Collections.Generic;
using Godot;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.Rooms;
using STS2RitsuLib.Scaffolding.Content;

namespace Valencina.ValencinaCode.Encounters;

public sealed class DuelEncounter : ModEncounterTemplate
{
	public override RoomType RoomType => (RoomType)2;

	public override bool ShouldGiveRewards => true;

	public override IEnumerable<MonsterModel> AllPossibleMonsters => new _003C_003Ez__ReadOnlySingleElementList<MonsterModel>((MonsterModel)(object)ModelDb.Monster<BowlbugRock>());

	public override IReadOnlyList<string> Slots => new _003C_003Ez__ReadOnlySingleElementList<string>("single");

	public override EncounterAssetProfile AssetProfile => new EncounterAssetProfile("res://Valencina/scenes/encounters/war_ambush_slots.tscn", (string)null, (string)null, (string)null, (string[])null, (string[])null, (string)null, (string)null);

	public override float GetCameraScaling()
	{
		return 0.95f;
	}

	public override Vector2 GetCameraOffset()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		return Vector2.Down * 35f;
	}

	public override bool IsValidForAct(ActModel act)
	{
		return false;
	}

	protected override IReadOnlyList<(MonsterModel, string?)> GenerateMonsters()
	{
		return new _003C_003Ez__ReadOnlySingleElementList<(MonsterModel, string)>((((MonsterModel)ModelDb.Monster<BowlbugRock>()).ToMutable(), "single"));
	}
}
