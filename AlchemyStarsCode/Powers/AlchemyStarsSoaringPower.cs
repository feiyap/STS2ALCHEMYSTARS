using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace AlchemyStars.Powers;

/// <summary>
/// 凌空：每当消耗的光能属性与上次不同时，抽牌；升级后额外获得能量。
/// </summary>
[RegisterPower]
public sealed class AlchemyStarsSoaringPower : ModPowerTemplate
{
    private int _pendingDraws;
    private int _pendingEnergy;
    private bool _alsoGainEnergy;

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new EnergyVar(1)
    ];

    /// <summary>升级后属性切换时还会获得能量。</summary>
    public void Configure(bool alsoGainEnergy) => _alsoGainEnergy = alsoGainEnergy;

    internal void NotifyAttributeDiffered(int count)
    {
        if (count <= 0)
            return;

        _pendingDraws += count;
        if (_alsoGainEnergy)
            _pendingEnergy += count;
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

        if (_pendingDraws > 0)
        {
            var draws = _pendingDraws;
            _pendingDraws = 0;
            Flash();
            await CardPileCmd.Draw(choiceContext, draws, player);
        }

        if (_pendingEnergy > 0)
        {
            var energy = _pendingEnergy;
            _pendingEnergy = 0;
            Flash();
            await PlayerCmd.GainEnergy(energy, player);
        }
    }
}
