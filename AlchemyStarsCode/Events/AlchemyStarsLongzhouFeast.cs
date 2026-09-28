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
/// 白夜极光事件：龙洲盛宴。
/// </summary>
[RegisterSharedEvent]
public sealed class AlchemyStarsLongzhouFeast : ModEventTemplate
{
    public override bool IsShared => true;

    public override bool IsAllowed(IRunState runState) =>
        ((AlchemyStarsEventHelpers.IsAct1(runState) || AlchemyStarsEventHelpers.IsAct2(runState))) && (runState.Players.Any(p => p.Gold >= 150));

    public override EventAssetProfile AssetProfile => new(
        InitialPortraitPath: $"{Entry.ResPath}/images/events/AlchemyStarsLongzhouFeast.png");

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new GoldVar(150),
        new HealVar(12m)
    ];

    protected override IReadOnlyList<EventOption> GenerateInitialOptions() =>
    [
        new EventOption(this, Proxy, InitialOptionKey("PROXY")),
        new EventOption(this, Self, InitialOptionKey("SELF")),
        new EventOption(this, Jiu, InitialOptionKey("JIU"))
    ];

    private async Task Proxy()
    {
        if (Owner!.Gold < DynamicVars.Gold.IntValue)
        {
            SetEventFinished(PageDescription("PROXY"));
            return;
        }
        await PlayerCmd.LoseGold(DynamicVars.Gold.IntValue, Owner, MegaCrit.Sts2.Core.Entities.Gold.GoldLossType.Spent);
        await AlchemyStarsEventHelpers.EnchantFromDeck<AlchemyStarsReplayEnchantment>(Owner!);
        SetEventFinished(PageDescription("PROXY"));
    }
    private async Task Self()
    {
        var elite = AlchemyStarsEventHelpers.PickRandomElite(Owner!);
        if (elite == null)
        {
            await AlchemyStarsEventHelpers.GainRandomRelic(Owner!, RelicRarity.Uncommon);
            SetEventFinished(PageDescription("SELF"));
            return;
        }
        EnterCombatWithoutExitingEvent(
            elite,
            [new RelicReward(RelicRarity.Uncommon, Owner!)],
            shouldResumeAfterCombat: false);
    }
    private async Task Jiu()
    {
        await CreatureCmd.Heal(Owner!.Creature, DynamicVars.Heal.IntValue);
        SetEventFinished(PageDescription("JIU"));
    }
}
