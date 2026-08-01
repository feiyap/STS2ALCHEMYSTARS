using System.Collections.Generic;
using System.Linq;
using Godot;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Rooms;
using STS2RitsuLib.Scaffolding.Content;
using Valencina.ValencinaCode.Acts;
using Valencina.ValencinaCode.Monsters;

namespace Valencina.ValencinaCode.Encounters;

public sealed class UngezieferKaiserEncounter : ModEncounterTemplate
{
	public override RoomType RoomType => (RoomType)3;

	public override IEnumerable<MonsterModel> AllPossibleMonsters => new _003C_003Ez__ReadOnlySingleElementList<MonsterModel>((MonsterModel)(object)ModelDb.Monster<UngezieferKaiser>());

	public override IReadOnlyList<string> Slots => new _003C_003Ez__ReadOnlySingleElementList<string>("M");

	public override EncounterAssetProfile AssetProfile => new EncounterAssetProfile("res://Valencina/scenes/encounters/ungeziefer_kaiser_background.tscn", (string)null, (string)null, (string)null, UngezieferKaiserAssets.AllAssetPaths.ToArray(), new string[2] { "res://Valencina/images/ui/run_history/ungeziefer_kaiser_encounter.png", "res://Valencina/images/ui/run_history/ungeziefer_kaiser_encounter_outline.png" }, "res://Valencina/images/ui/run_history/ungeziefer_kaiser_encounter.png", "res://Valencina/images/ui/run_history/ungeziefer_kaiser_encounter_outline.png");

	protected override bool UseProgrammaticCombatBackground => true;

	public override IEnumerable<string> ExtraAssetPaths => ((EncounterModel)this).ExtraAssetPaths.Concat(UngezieferKaiserAssets.AllAssetPaths).Distinct();

	protected override BackgroundAssets BuildProgrammaticCombatBackground(ActModel parentAct, Rng rng)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Expected O, but got Unknown
		return new BackgroundAssets("valencina-ungeziefer_kaiser_encounter", rng);
	}

	public override float GetCameraScaling()
	{
		return 0.85f;
	}

	public override Vector2 GetCameraOffset()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		return Vector2.Down * 35f;
	}

	public override bool IsValidForAct(ActModel act)
	{
		return act is ValencinaAct4;
	}

	protected override IReadOnlyList<(MonsterModel, string?)> GenerateMonsters()
	{
		return new _003C_003Ez__ReadOnlySingleElementList<(MonsterModel, string)>((((MonsterModel)ModelDb.Monster<UngezieferKaiser>()).ToMutable(), "M"));
	}
}
