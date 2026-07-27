using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Models;

namespace AlchemyStars.Mechanics;

/// <summary>
/// 协调光能/属性格战斗钩子�?
/// </summary>
[RegisterSingleton]
public sealed class AlchemyStarsLightMechanicService : HookedSingletonModel
{
    public AlchemyStarsLightMechanicService() : base(HookType.Combat)
    {
    }

    public override async Task BeforeCombatStart()
    {
        var runState = CurrentRunState;
        if (runState == null)
        {
            await Task.CompletedTask;
            return;
        }

        foreach (var player in runState.Players)
        {
            if (!LightMechanic.HasMechanicRelic(player))
                continue;

            LightMechanicCombatState.Reset(player);
            LightMechanic.InitializeForCombat(player);
        }

        await Task.CompletedTask;
    }

    public override decimal ModifyDamageMultiplicative(
        Creature? target,
        decimal amount,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource,
        CardPlay? cardPlay)
    {
        if (dealer?.Player == null || !LightMechanic.HasMechanicRelic(dealer.Player))
            return 1m;

        var element = LightMechanicDamageContext.CurrentElement;
        var state = LightMechanic.GetActiveState(dealer.Player);
        if (element == null && (state == null || !state.RainbowActive))
            return 1m;

        return LightMechanic.GetOutgoingDamageMultiplier(dealer.Player, element);
    }

    public override async Task AfterSideTurnEnd(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        IEnumerable<Creature> participants)
    {
        if (side != CombatSide.Player)
            return;

        foreach (var creature in participants)
        {
            var player = creature.Player;
            if (player == null || !LightMechanic.HasMechanicRelic(player))
                continue;

            await LightMechanic.ResolvePlayerTurnEnd(choiceContext, player);
        }
    }

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (!LightMechanic.HasMechanicRelic(player))
            return;

        LightMechanic.ResetTurnCounters(player);
        AlchemyStarsForestState.TickTeaPartyCooldown(player);
        await Task.CompletedTask;
    }
}
