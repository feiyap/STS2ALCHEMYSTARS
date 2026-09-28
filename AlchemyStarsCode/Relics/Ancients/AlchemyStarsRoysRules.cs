using AlchemyStars.Characters;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Saves.Runs;
using STS2RitsuLib.Interop.AutoRegistration;

namespace AlchemyStars.Relics.Ancients;

/// <summary>
/// 罗伊的规矩：战斗开始获得再生，层数随胜利累积（初始 1）。
/// </summary>
[RegisterRelic(typeof(AlchemyStarsRelicPool))]
public sealed class AlchemyStarsRoysRules : AlchemyStarsAncientRelicBase
{
    private int _regenAmount = 1;

    public override bool ShowCounter => true;

    public override int DisplayAmount => RegenAmount;

    /// <summary>
    /// 当前再生层数，初始为 1，胜利后 +1。
    /// </summary>
    [SavedProperty]
    public int RegenAmount
    {
        get => _regenAmount;
        set
        {
            AssertMutable();
            _regenAmount = value;
            InvokeDisplayAmountChanged();
        }
    }

    public override async Task BeforeCombatStart()
    {
        if (Owner == null)
            return;

        Flash();
        await PowerCmd.Apply<RegenPower>(
            new ThrowingPlayerChoiceContext(),
            Owner.Creature,
            RegenAmount,
            Owner.Creature,
            null);
    }

    public override Task AfterCombatVictory(CombatRoom room)
    {
        RegenAmount++;
        Flash();
        return Task.CompletedTask;
    }
}
