using System;
using System.Linq;
using AlchemyStars.Cards;
using AlchemyStars.Enchantments;
using AlchemyStars.Relics.Events;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Acts;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Enchantments;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using STS2RitsuLib.Scaffolding.Content;
using MegaCrit.Sts2.Core.Entities.Players;

namespace AlchemyStars.Events;

/// <summary>
/// 白夜极光事件：热砂攻防战。仅由「万门之钥」进入，不进通用事件池，选项为单人选择。
/// </summary>
public sealed class AlchemyStarsHotSandDefense : ModEventTemplate
{
    public override bool IsShared => false;

    public override bool IsAllowed(IRunState runState) => false;

    public override EventAssetProfile AssetProfile => new(
        InitialPortraitPath: $"{Entry.ResPath}/images/events/AlchemyStarsHotSandDefense.png");

    protected override IReadOnlyList<EventOption> GenerateInitialOptions() =>
    [
        new EventOption(this, Ke, InitialOptionKey("KE"),
            HoverTipFactory.FromCardWithCardHoverTips<AlchemyStarsKe>(upgrade: true)),
        new EventOption(this, Rektone, InitialOptionKey("REKTONE"),
            HoverTipFactory.FromCardWithCardHoverTips<AlchemyStarsRektone>(upgrade: true)),
        new EventOption(this, Hard, InitialOptionKey("HARD"),
            HoverTipFactory.FromEnchantment<Imbued>().Concat(HoverTipFactory.FromEnchantment<Glam>()))
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
