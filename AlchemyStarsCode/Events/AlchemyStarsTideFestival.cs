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
/// 白夜极光事件：潮汐祭。
/// </summary>
[RegisterSharedEvent]
public sealed class AlchemyStarsTideFestival : ModEventTemplate
{
    private bool _pendingMomentum;

    public override bool IsShared => true;

    public override bool IsAllowed(IRunState runState) =>
        (AlchemyStarsEventHelpers.IsAct2(runState));

    public override EventAssetProfile AssetProfile => new(
        InitialPortraitPath: $"{Entry.ResPath}/images/events/AlchemyStarsTideFestival.png");

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new HealVar(12m),
        new IntVar("Vigor", 4m)
    ];

    protected override IReadOnlyList<EventOption> GenerateInitialOptions() =>
    [
        new EventOption(this, Play, InitialOptionKey("PLAY")),
        new EventOption(this, Danger, InitialOptionKey("DANGER")),
        new EventOption(this, Ease, InitialOptionKey("EASE"))
    ];

    public override async Task Resume(AbstractRoom room)
    {
        if (!_pendingMomentum)
        {
            SetEventFinished(PageDescription("DANGER"));
            return;
        }
        _pendingMomentum = false;
        await AlchemyStarsEventHelpers.EnchantFromDeck<Momentum>(Owner!, amount: 1m, pickCount: 2);
        SetEventFinished(PageDescription("DANGER"));
    }

    private async Task Play()
    {
        await AlchemyStarsEventHelpers.RemoveEtherealAndExhaust(Owner!);
        SetEventFinished(PageDescription("PLAY"));
    }
    private async Task Danger()
    {
        await CreatureCmd.Heal(Owner!.Creature, DynamicVars.Heal.IntValue);
        var elite = AlchemyStarsEventHelpers.PickRandomElite(Owner!);
        if (elite == null)
        {
            await AlchemyStarsEventHelpers.EnchantFromDeck<Momentum>(Owner!, amount: 1m, pickCount: 2);
            SetEventFinished(PageDescription("DANGER"));
            return;
        }
        _pendingMomentum = true;
        EnterCombatWithoutExitingEvent(elite, Array.Empty<Reward>(), shouldResumeAfterCombat: true);
    }
    private async Task Ease()
    {
        await AlchemyStarsEventHelpers.EnchantFromDeck<Vigorous>(
            Owner!, amount: DynamicVars["Vigor"].BaseValue);
        SetEventFinished(PageDescription("EASE"));
    }
}
