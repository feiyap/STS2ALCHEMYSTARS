using System;
using AlchemyStars.Cards;
using AlchemyStars.Encounters;
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
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using System.Linq;
using MegaCrit.Sts2.Core.Entities.Players;

namespace AlchemyStars.Events;

/// <summary>
/// 白夜极光事件：寂静之陵。第 3 幕事件池可见。
/// </summary>
[RegisterSharedEvent]
public sealed class AlchemyStarsSilentMausoleum : ModEventTemplate
{
    private bool _pendingStarCrest;

    public override bool IsShared => true;

    public override bool IsAllowed(IRunState runState) =>
        AlchemyStarsEventHelpers.IsAct3(runState);

    public override EventAssetProfile AssetProfile => new(
        InitialPortraitPath: $"{Entry.ResPath}/images/events/AlchemyStarsSilentMausoleum.png");

    protected override IReadOnlyList<EventOption> GenerateInitialOptions() =>
    [
        new EventOption(this, Boss, InitialOptionKey("BOSS"),
            HoverTipFactory.FromRelic<AlchemyStarsStarCrest>()),
        new EventOption(this, Seals, InitialOptionKey("SEALS"),
            HoverTipFactory.FromCardWithCardHoverTips<AlchemyStarsOldSealFrost>(upgrade: true)
                .Concat(HoverTipFactory.FromCardWithCardHoverTips<AlchemyStarsOldSealGuard>(upgrade: true))
                .Concat(HoverTipFactory.FromCardWithCardHoverTips<AlchemyStarsOldSealRhyme>(upgrade: true))
                .Concat(HoverTipFactory.FromCardWithCardHoverTips<AlchemyStarsOldSealRequiem>(upgrade: true))),
        new EventOption(this, Free, InitialOptionKey("FREE"),
            HoverTipFactory.FromEnchantment<RoyallyApproved>()),
        new EventOption(this, Story, InitialOptionKey("STORY"),
            HoverTipFactory.FromEnchantment<SoulsPower>())
    ];

    public override async Task Resume(AbstractRoom room)
    {
        if (!_pendingStarCrest)
        {
            SetEventFinished(PageDescription("BOSS"));
            return;
        }
        _pendingStarCrest = false;
        await RelicCmd.Obtain<AlchemyStarsStarCrest>(Owner!);
        SetEventFinished(PageDescription("BOSS"));
    }

    private Task Boss()
    {
        _pendingStarCrest = true;
        EnterCombatWithoutExitingEvent<AlchemyStarsSwordAltarEncounter>(
            Array.Empty<Reward>(),
            shouldResumeAfterCombat: true);
        return Task.CompletedTask;
    }
    private async Task Seals()
    {
        await AlchemyStarsEventHelpers.PickOneUpgradedFromCandidates(Owner!,
        [
            ModelDb.Card<AlchemyStarsOldSealFrost>(),
            ModelDb.Card<AlchemyStarsOldSealGuard>(),
            ModelDb.Card<AlchemyStarsOldSealRhyme>(),
            ModelDb.Card<AlchemyStarsOldSealRequiem>(),
        ]);
        SetEventFinished(PageDescription("SEALS"));
    }
    private async Task Free()
    {
        await AlchemyStarsEventHelpers.EnchantFromDeck<RoyallyApproved>(Owner!);
        SetEventFinished(PageDescription("FREE"));
    }
    private async Task Story()
    {
        await AlchemyStarsEventHelpers.EnchantFromDeck<SoulsPower>(Owner!);
        SetEventFinished(PageDescription("STORY"));
    }
}
