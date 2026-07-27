using System.Collections.Generic;
using System.Threading.Tasks;
using AlchemyStars.Mechanics;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace AlchemyStars.Powers;

/// <summary>
/// 默陵之卫：每回合第一次攻击获得森光能与收割意识�?/// </summary>
[RegisterPower]
public sealed class AlchemyStarsShikariGuardPower : ModPowerTemplate
{
    private bool _triggeredThisTurn;
    private decimal _harvestAmount = 1m;

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;

    public void Configure(decimal harvestAmount) => _harvestAmount = harvestAmount;

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player.Creature != Owner)
            return;

        _triggeredThisTurn = false;
        await Task.CompletedTask;
    }

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (_triggeredThisTurn || cardPlay.Card.Owner != Owner.Player)
            return;

        if (cardPlay.Card.Type != CardType.Attack)
            return;

        _triggeredThisTurn = true;
        var player = Owner.Player;
        if (player == null)
            return;

        LightMechanic.TryGrantLightEnergy(player, LightElement.Forest);
        await PowerCmd.Apply<AlchemyStarsHarvestConsciousnessPower>(
            choiceContext,
            Owner,
            _harvestAmount,
            Owner,
            cardPlay.Card);
    }
}
