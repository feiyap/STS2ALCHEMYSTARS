using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;
using STS2RitsuLib.Scaffolding.Content;

namespace AlchemyStars.Events;

/// <summary>
/// 启迪者：仅作为涅奥后事件内续页的本地化/视觉模板，不进入地图先古池。
/// </summary>
public sealed class AlchemyStarsEnlightener : ModAncientEventTemplate
{
    public const string EventEntry = "ALCHEMY_STARS_ENLIGHTENER";

    public override LocString InitialDescription =>
        L10NLookup($"{EventEntry}.pages.INITIAL.description");

    public override bool IsValidForAct(ActModel act) => false;

    public override bool IsAllowed(IRunState runState) => false;

    public override IEnumerable<EventOption> AllPossibleOptions => [];

    protected override IReadOnlyList<EventOption> GenerateInitialOptions() => [];
}
