using System;
using AlchemyStars.Cards;
using AlchemyStars.Enchantments;
using AlchemyStars.Relics.Ancients;
using AlchemyStars.Relics.Events;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Acts;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Enchantments;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using System.Linq;
using MegaCrit.Sts2.Core.Entities.Players;

namespace AlchemyStars.Events;

/// <summary>
/// 白夜极光事件：隙间旅人。
/// </summary>
[RegisterSharedEvent]
public sealed class AlchemyStarsGapTraveler : ModEventTemplate
{
    public override bool IsShared => true;

    public override bool IsAllowed(IRunState runState) =>
        runState.Players.Any(p => p.GetRelic<AlchemyStarsKeyOfManyDoors>() != null);

    public override EventAssetProfile AssetProfile => new(
        InitialPortraitPath: $"{Entry.ResPath}/images/events/AlchemyStarsGapTraveler.png");

    protected override IReadOnlyList<EventOption> GenerateInitialOptions() =>
    [
        new EventOption(this, Vic, InitialOptionKey("VIC")),
        new EventOption(this, William, InitialOptionKey("WILLIAM")),
        new EventOption(this, Gap, InitialOptionKey("GAP"))
    ];

    private async Task Vic()
    {
        await AlchemyStarsEventHelpers.AddUpgradedCardToDeck<AlchemyStarsVictoriaGraveSong>(Owner!);
        SetEventFinished(PageDescription("VIC"));
    }
    private async Task William()
    {
        // 可选任意数量变化：最多变换整副牌。
        var max = Math.Max(1, Owner!.Deck.Cards.Count);
        await AlchemyStarsEventHelpers.TransformFromDeck(Owner!, max);
        SetEventFinished(PageDescription("WILLIAM"));
    }
    private async Task Gap()
    {
        await AlchemyStarsEventHelpers.EnchantFromDeck<Imbued>(Owner!);
        await AlchemyStarsEventHelpers.EnchantFromDeck<Glam>(Owner!);
        SetEventFinished(PageDescription("GAP"));
    }
}
