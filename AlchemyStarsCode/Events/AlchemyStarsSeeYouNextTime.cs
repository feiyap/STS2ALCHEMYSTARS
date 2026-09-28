using System;
using AlchemyStars.Cards;
using AlchemyStars.Enchantments;
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

namespace AlchemyStars.Events;

/// <summary>
/// 白夜极光事件：下次再见。
/// </summary>
[RegisterActEvent(typeof(Overgrowth))]
[RegisterActEvent(typeof(Underdocks))]
public sealed class AlchemyStarsSeeYouNextTime : ModEventTemplate
{
    public override EventAssetProfile AssetProfile => new(
        InitialPortraitPath: $"{Entry.ResPath}/images/events/AlchemyStarsSeeYouNextTime.png");

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new GoldVar(99),
        new IntVar("GoldMin", 99m),
        new IntVar("GoldMax", 200m)
    ];

    protected override IReadOnlyList<EventOption> GenerateInitialOptions() =>
    [
        new EventOption(this, Letters, InitialOptionKey("LETTERS")),
        new EventOption(this, Biyataman, InitialOptionKey("BIYATAMAN")),
        new EventOption(this, Prima, InitialOptionKey("PRIMA"))
    ];

    private async Task Letters()
    {
        await AlchemyStarsEventHelpers.GainRandomRelic(Owner!);
        SetEventFinished(PageDescription("LETTERS"));
    }
    private async Task Biyataman()
    {
        var gold = Rng.NextInt(
            DynamicVars["GoldMin"].IntValue,
            DynamicVars["GoldMax"].IntValue + 1);
        await PlayerCmd.GainGold(gold, Owner!);
        SetEventFinished(PageDescription("BIYATAMAN"));
    }
    private async Task Prima()
    {
        await AlchemyStarsEventHelpers.TransformFromDeck(Owner!, 1);
        await AlchemyStarsEventHelpers.RemoveFromDeck(
            Owner!, 1, c => c.Type == CardType.Curse);
        SetEventFinished(PageDescription("PRIMA"));
    }
}
