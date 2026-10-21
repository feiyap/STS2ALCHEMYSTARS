using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace AlchemyStars.Mechanics;

/// <summary>
/// 把同一次打出里的多段攻击收成一次攻击。
/// 活力、巨大化会绑定在第一条 AttackCommand 上，并在该命令结束时消耗。
/// 薇丝、剑雨这类多段是每击各开一条命令，于是只有第一下吃到加成。
/// 这里先开一个不结算伤害的攻击上下文占住绑定，整段打出结束后再结束它。
/// </summary>
internal static class CardPlayAttackGroup
{
    private static CardPlay? _play;
    private static AttackContext? _context;

    public static async Task EnsureAsync(PlayerChoiceContext choiceContext, CardPlay? cardPlay)
    {
        if (cardPlay?.Card.Owner?.Creature.CombatState == null)
            return;

        if (ReferenceEquals(_play, cardPlay))
            return;

        await FinishAsync();

        _play = cardPlay;
        _context = await AttackCommand.CreateContextAsync(
            cardPlay.Card.Owner.Creature.CombatState,
            choiceContext,
            cardPlay);
    }

    public static async Task FinishIfMatchAsync(CardPlay cardPlay)
    {
        if (ReferenceEquals(_play, cardPlay))
            await FinishAsync();
    }

    public static async Task FinishIfOwnerDiedAsync(Creature creature)
    {
        if (_play?.Card.Owner?.Creature == creature)
            await FinishAsync();
    }

    public static async Task FinishAsync()
    {
        var context = _context;
        _context = null;
        _play = null;
        if (context != null)
            await context.DisposeAsync();
    }
}
