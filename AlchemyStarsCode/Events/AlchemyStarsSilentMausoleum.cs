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
/// 白夜极光事件：寂静之陵。
/// </summary>
[RegisterSharedEvent]
public sealed class AlchemyStarsSilentMausoleum : ModEventTemplate
{
    private bool _pendingStarCrest;

    public override bool IsShared => true;

    public override bool IsAllowed(IRunState runState) =>
        (AlchemyStarsEventHelpers.IsAct3(runState));

    public override EventAssetProfile AssetProfile => new(
        InitialPortraitPath: $"{Entry.ResPath}/images/events/AlchemyStarsSilentMausoleum.png");

    protected override IReadOnlyList<EventOption> GenerateInitialOptions() =>
    [
        new EventOption(this, Boss, InitialOptionKey("BOSS")),
        new EventOption(this, Seals, InitialOptionKey("SEALS")),
        new EventOption(this, Free, InitialOptionKey("FREE")),
        new EventOption(this, Story, InitialOptionKey("STORY"))
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

    private async Task Boss()
    {
        var elite = AlchemyStarsEventHelpers.PickRandomElite(Owner!);
        if (elite == null)
        {
            await RelicCmd.Obtain<AlchemyStarsStarCrest>(Owner!);
            SetEventFinished(PageDescription("BOSS"));
            return;
        }
        // 祭剑座 Boss 未实装前，暂以精英战 + 星辰纹章占位。
        _pendingStarCrest = true;
        EnterCombatWithoutExitingEvent(elite, Array.Empty<Reward>(), shouldResumeAfterCombat: true);
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
