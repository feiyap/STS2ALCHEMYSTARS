using System;
using AlchemyStars.Cards;
using AlchemyStars.Enchantments;
using AlchemyStars.Relics.Events;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Helpers;
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
        new EventOption(this, Danger, InitialOptionKey("DANGER"),
            HoverTipFactory.FromEnchantment<Momentum>()),
        new EventOption(this, Ease, InitialOptionKey("EASE"),
            HoverTipFactory.FromEnchantment<Vigorous>(DynamicVars["Vigor"].IntValue))
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
    private Task Danger()
    {
        var owner = Owner!;
        // 治疗若挂起，不能挡住进战。非战斗治疗在调用时就会把血加上。
        TaskHelper.RunSafely(CreatureCmd.Heal(owner.Creature, DynamicVars.Heal.IntValue));

        EncounterModel? fight = null;
        try
        {
            // 共享事件的 Rng 种子一致，联机双方会进入同一场战斗。
            fight = AlchemyStarsEventHelpers.PickRandomElite(owner, Rng);
        }
        catch (Exception ex)
        {
            AlchemyStars.Entry.Logger.Error($"[TideFestival] 抽取精英失败: {ex}");
        }

        if (fight == null)
        {
            try
            {
                fight = owner.RunState.Act.PullNextEncounter(RoomType.Monster).ToMutable();
                AlchemyStars.Entry.Logger.Warn("[TideFestival] 精英池为空，改为进入普通战斗。");
            }
            catch (Exception ex)
            {
                AlchemyStars.Entry.Logger.Error($"[TideFestival] 无法取得战斗遭遇: {ex}");
            }
        }

        if (fight == null)
            return FinishDangerWithoutFight();

        _pendingMomentum = true;
        EnterCombatWithoutExitingEvent(fight, Array.Empty<Reward>(), shouldResumeAfterCombat: true);
        return Task.CompletedTask;
    }

    private async Task FinishDangerWithoutFight()
    {
        await AlchemyStarsEventHelpers.EnchantFromDeck<Momentum>(Owner!, amount: 1m, pickCount: 2);
        SetEventFinished(PageDescription("DANGER"));
    }
    private async Task Ease()
    {
        await AlchemyStarsEventHelpers.EnchantFromDeck<Vigorous>(
            Owner!, amount: DynamicVars["Vigor"].BaseValue);
        SetEventFinished(PageDescription("EASE"));
    }
}
