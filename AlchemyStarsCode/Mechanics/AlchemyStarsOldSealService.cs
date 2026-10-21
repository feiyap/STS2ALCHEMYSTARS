using AlchemyStars.Cards;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Models;
using STS2RitsuLib.Utils;

namespace AlchemyStars.Mechanics;

/// <summary>
/// 记录本场战斗玩家受伤次数，供戒卫座加算伤害。
/// </summary>
[RegisterSingleton]
public sealed class AlchemyStarsOldSealCombatService : HookedSingletonModel
{
    private static readonly AttachedState<Player, int> TimesHurt = new(_ => 0);

    public AlchemyStarsOldSealCombatService()
        : base(HookType.Combat)
    {
    }

    public static int GetTimesHurt(Player? player) =>
        player == null ? 0 : TimesHurt[player];

    public override Task BeforeCombatStart()
    {
        var run = CurrentRunState;
        if (run == null)
            return Task.CompletedTask;

        foreach (var player in run.Players)
            TimesHurt[player] = 0;

        return Task.CompletedTask;
    }

    public override Task AfterDamageReceived(
        PlayerChoiceContext choiceContext,
        Creature target,
        DamageResult result,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource)
    {
        var player = target.Player;
        if (player == null || result.UnblockedDamage <= 0)
            return Task.CompletedTask;

        TimesHurt[player]++;
        return Task.CompletedTask;
    }
}

/// <summary>
/// 阻止寂静猎兵·祭剑座以任何方式进入牌组。
/// </summary>
[RegisterSingleton]
public sealed class AlchemyStarsOldSealRunService : HookedSingletonModel
{
    public AlchemyStarsOldSealRunService()
        : base(HookType.Run)
    {
    }

    public override bool ShouldAddToDeck(CardModel card) =>
        card is not AlchemyStarsOldSealAltar;
}
