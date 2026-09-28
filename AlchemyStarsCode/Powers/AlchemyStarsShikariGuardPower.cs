using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace AlchemyStars.Powers;

/// <summary>
/// 默陵之卫：每回合第一次攻击获得能量；每次攻击按份数增加收割意识，每份上限 20 层。
/// </summary>
[RegisterPower]
public sealed class AlchemyStarsShikariGuardPower : AlchemyStarsPowerBase
{
    private const decimal HarvestCapPerCopy = 20m;

    private readonly List<(decimal Increment, decimal Contributed)> _copies = [];
    private bool _gainedEnergyThisTurn;

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public void AddCopy(decimal harvestIncrement) =>
        _copies.Add((harvestIncrement, 0m));

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player.Creature != Owner)
            return;

        _gainedEnergyThisTurn = false;
        await Task.CompletedTask;
    }

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner != Owner.Player || cardPlay.Card.Type != CardType.Attack)
            return;

        var player = Owner.Player;
        if (player == null)
            return;

        if (!_gainedEnergyThisTurn)
        {
            _gainedEnergyThisTurn = true;
            Flash();
            await PlayerCmd.GainEnergy(1, player);
        }

        var harvest = TakeHarvestGain();
        if (harvest <= 0m)
            return;

        Flash();
        await PowerCmd.Apply<AlchemyStarsHarvestConsciousnessPower>(
            choiceContext,
            Owner,
            harvest,
            Owner,
            cardPlay.Card);
    }

    private decimal TakeHarvestGain()
    {
        var total = 0m;
        for (var i = 0; i < _copies.Count; i++)
        {
            var remaining = HarvestCapPerCopy - _copies[i].Contributed;
            if (remaining <= 0m || _copies[i].Increment <= 0m)
                continue;

            var gain = decimal.Min(_copies[i].Increment, remaining);
            _copies[i] = (_copies[i].Increment, _copies[i].Contributed + gain);
            total += gain;
        }

        return total;
    }
}
