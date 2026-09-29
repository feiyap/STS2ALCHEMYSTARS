using AlchemyStars.Characters;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Saves.Runs;
using STS2RitsuLib.Interop.AutoRegistration;

namespace AlchemyStars.Relics.Ancients;

/// <summary>
/// 罗伊的规矩：战斗开始获得再生；每获胜 2 场后层数 +1（初始 1）。
/// </summary>
[RegisterRelic(typeof(AlchemyStarsRelicPool))]
public sealed class AlchemyStarsRoysRules : AlchemyStarsAncientRelicBase
{
    private int _regenAmount = 1;
    private int _victoryCount;

    public override bool ShowCounter => true;

    public override int DisplayAmount => RegenAmount;

    /// <summary>
    /// 当前再生层数，初始为 1；每 2 场胜利后 +1。
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

    /// <summary>
    /// 累计胜利场次，用于每 2 场提升一次再生。
    /// </summary>
    [SavedProperty]
    public int VictoryCount
    {
        get => _victoryCount;
        set
        {
            AssertMutable();
            _victoryCount = value;
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
        VictoryCount++;
        if (VictoryCount % 2 != 0)
            return Task.CompletedTask;

        RegenAmount++;
        Flash();
        return Task.CompletedTask;
    }
}
