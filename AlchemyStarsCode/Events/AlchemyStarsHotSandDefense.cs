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
/// 白夜极光事件：热砂攻防战。
/// </summary>
[RegisterSharedEvent]
public sealed class AlchemyStarsHotSandDefense : ModEventTemplate
{
    public override bool IsShared => true;

    public override bool IsAllowed(IRunState runState) =>
        runState.Players.Any(p => p.GetRelic<AlchemyStarsKeyOfManyDoors>() != null);

    public override EventAssetProfile AssetProfile => new(
        InitialPortraitPath: $"{Entry.ResPath}/images/events/AlchemyStarsHotSandDefense.png");

    protected override IReadOnlyList<EventOption> GenerateInitialOptions() =>
    [
        new EventOption(this, Ke, InitialOptionKey("KE")),
        new EventOption(this, Rektone, InitialOptionKey("REKTONE")),
        new EventOption(this, Hard, InitialOptionKey("HARD"))
    ];

    private async Task Ke()
    {
        await AlchemyStarsEventHelpers.AddUpgradedCardToDeck<AlchemyStarsKe>(Owner!);
        SetEventFinished(PageDescription("KE"));
    }
    private async Task Rektone()
    {
        await AlchemyStarsEventHelpers.AddUpgradedCardToDeck<AlchemyStarsRektone>(Owner!);
        SetEventFinished(PageDescription("REKTONE"));
    }
    private async Task Hard()
    {
        await AlchemyStarsEventHelpers.EnchantFromDeck<Imbued>(Owner!);
        await AlchemyStarsEventHelpers.EnchantFromDeck<Glam>(Owner!);
        SetEventFinished(PageDescription("HARD"));
    }
}
