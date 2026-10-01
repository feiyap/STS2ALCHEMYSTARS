using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Acts;
using MegaCrit.Sts2.Core.Rooms;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace AlchemyStars.Encounters;

/// <summary>
/// 祭剑座遭遇。只由寂静之陵进入，不进入章节的普通怪或 Boss 池。
/// </summary>
[RegisterActEncounter(typeof(Glory))]
public sealed class AlchemyStarsSwordAltarEncounter : ModEncounterTemplate
{
    public override bool IsValidForAct(ActModel act) => false;

    public override RoomType RoomType => RoomType.Monster;

    public override IEnumerable<MonsterModel> AllPossibleMonsters =>
        [ModelDb.Monster<AlchemyStars.Monsters.AlchemyStarsSwordAltar>()];

    protected override IReadOnlyList<(MonsterModel, string?)> GenerateMonsters() =>
        [(ModelDb.Monster<AlchemyStars.Monsters.AlchemyStarsSwordAltar>().ToMutable(), null)];
}
