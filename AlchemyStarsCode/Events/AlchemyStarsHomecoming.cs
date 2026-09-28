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
/// 白夜极光事件：归家。
/// </summary>
[RegisterActEvent(typeof(Overgrowth))]
[RegisterActEvent(typeof(Underdocks))]
public sealed class AlchemyStarsHomecoming : ModEventTemplate
{
    public override EventAssetProfile AssetProfile => new(
        InitialPortraitPath: $"{Entry.ResPath}/images/events/AlchemyStarsHomecoming.png");

    protected override IReadOnlyList<EventOption> GenerateInitialOptions() =>
    [
        new EventOption(this, Help, InitialOptionKey("HELP")),
        new EventOption(this, Leisure, InitialOptionKey("LEISURE")),
        new EventOption(this, Forget, InitialOptionKey("FORGET"))
    ];

    private async Task Help()
    {
        await AlchemyStarsEventHelpers.EnchantFromDeck<Sown>(Owner!);
        SetEventFinished(PageDescription("HELP"));
    }
    private async Task Leisure()
    {
        await AlchemyStarsEventHelpers.UpgradeRandomFromDeck(Owner!, 2);
        SetEventFinished(PageDescription("LEISURE"));
    }
    private async Task Forget()
    {
        await AlchemyStarsEventHelpers.RemoveFromDeck(Owner!, 1);
        SetEventFinished(PageDescription("FORGET"));
    }
}
