using System.Collections.Generic;
using System.Threading.Tasks;
using AlchemyStars.Mechanics;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace AlchemyStars.Powers;

/// <summary>
/// 灼灼海棠：灼燃增幅额外提高 20%；回合开始生成火属性格；火格被移出时获能与灼燃。
/// </summary>
[RegisterPower]
public sealed class AlchemyStarsBloomingBegoniaPower : ModPowerTemplate
{
    private int _pendingEnergy;
    private int _pendingIgnition;

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new EnergyVar(1),
        new PowerVar<AlchemyStarsIgnitionPower>(1m)
    ];

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player.Creature != Owner || Amount <= 0)
            return;

        Flash();
        for (var i = 0; i < Amount; i++)
            LightMechanic.TryAddAttributeCell(player, LightElement.Fire);

        await FlushPending(choiceContext);
    }

    /// <summary>
    /// 火属性格被移出转色栏时：每格排队获得 1 能量与 1 层灼燃。
    /// </summary>
    internal void NotifyFireCellsRemoved(int count)
    {
        if (count <= 0)
            return;

        _pendingEnergy += count;
        _pendingIgnition += count;
    }

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await FlushPending(choiceContext);
    }

    private async Task FlushPending(PlayerChoiceContext choiceContext)
    {
        var player = Owner.Player;
        if (player == null)
            return;

        if (_pendingEnergy > 0)
        {
            var energy = _pendingEnergy;
            _pendingEnergy = 0;
            Flash();
            await PlayerCmd.GainEnergy(energy, player);
        }

        if (_pendingIgnition > 0)
        {
            var ignition = _pendingIgnition;
            _pendingIgnition = 0;
            await PowerCmd.Apply<AlchemyStarsIgnitionPower>(
                choiceContext,
                Owner,
                ignition,
                Owner,
                null);
        }
    }
}
