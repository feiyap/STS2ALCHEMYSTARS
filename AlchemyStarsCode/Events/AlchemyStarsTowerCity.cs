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
using System.Linq;
using MegaCrit.Sts2.Core.Entities.Players;

namespace AlchemyStars.Events;

/// <summary>
/// 白夜极光事件：高塔之城。
/// </summary>
[RegisterSharedEvent]
public sealed class AlchemyStarsTowerCity : ModEventTemplate
{
    public override bool IsShared => true;

    public override bool IsAllowed(IRunState runState) =>
        ((AlchemyStarsEventHelpers.IsAct2(runState) || AlchemyStarsEventHelpers.IsAct3(runState)));

    public override EventAssetProfile AssetProfile => new(
        InitialPortraitPath: $"{Entry.ResPath}/images/events/AlchemyStarsTowerCity.png");

    protected override IReadOnlyList<EventOption> GenerateInitialOptions() =>
    [
        new EventOption(this, Fear, InitialOptionKey("FEAR")),
        new EventOption(this, Doubt, InitialOptionKey("DOUBT")),
        new EventOption(this, Deny, InitialOptionKey("DENY"))
    ];

    private async Task Fear()
    {
        await CardPileCmd.AddCurseToDeck<Regret>(Owner!);
        await RelicCmd.Obtain<AlchemyStarsLawString>(Owner!);
        SetEventFinished(PageDescription("FEAR"));
    }
    private async Task Doubt()
    {
        await AlchemyStarsEventHelpers.RemoveFromDeck(Owner!, 2);
        await CardPileCmd.AddCurseToDeck<Doubt>(Owner!);
        SetEventFinished(PageDescription("DOUBT"));
    }
    private async Task Deny()
    {
        await AlchemyStarsEventHelpers.EnchantFromDeck<PerfectFit>(Owner!);
        await CardPileCmd.AddCurseToDeck<Guilty>(Owner!);
        SetEventFinished(PageDescription("DENY"));
    }
}
