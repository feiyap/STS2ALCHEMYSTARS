using System;
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
/// 白夜极光事件：少女与遗迹。
/// </summary>
[RegisterActEvent(typeof(Hive))]
public sealed class AlchemyStarsGirlAndRuins : ModEventTemplate
{
    public override EventAssetProfile AssetProfile => new(
        InitialPortraitPath: $"{Entry.ResPath}/images/events/AlchemyStarsGirlAndRuins.png");

    protected override IReadOnlyList<EventOption> GenerateInitialOptions() =>
    [
        new EventOption(this, Dress, InitialOptionKey("DRESS")),
        new EventOption(this, Adventure, InitialOptionKey("ADVENTURE")),
        new EventOption(this, Farewell, InitialOptionKey("FAREWELL"))
    ];

    private async Task Dress()
    {
        await CardPileCmd.AddCurseToDeck<Writhe>(Owner!);
        await AlchemyStarsEventHelpers.GainCharacterRelic(Owner!);
        SetEventFinished(PageDescription("DRESS"));
    }
    private async Task Adventure()
    {
        await AlchemyStarsEventHelpers.PickCardsFromCharacterPool(Owner!, 10, 2);
        SetEventFinished(PageDescription("ADVENTURE"));
    }
    private async Task Farewell()
    {
        await AlchemyStarsEventHelpers.UpgradeFromDeck(Owner!, 1);
        SetEventFinished(PageDescription("FAREWELL"));
    }
}
