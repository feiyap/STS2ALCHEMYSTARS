using AlchemyStars.Characters;
using AlchemyStars.Mechanics;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Saves.Runs;
using STS2RitsuLib.Interop.AutoRegistration;

namespace AlchemyStars.Relics.Character;

/// <summary>
/// 虹光宣告：每隔 3 个回合，消耗全部光能并转化为等量万色格。
/// </summary>
[RegisterRelic(typeof(AlchemyStarsRelicPool))]
public sealed class AlchemyStarsRainbowProclamation : AlchemyStarsCharacterRelicBase
{
    private const int Interval = 3;

    private int _turnCounter;

    public override RelicRarity Rarity => RelicRarity.Uncommon;

    public override bool ShowCounter => true;

    public override int DisplayAmount
    {
        get
        {
            var mod = _turnCounter % Interval;
            return mod == 0 ? 0 : Interval - mod;
        }
    }

    [SavedProperty]
    public int TurnCounter
    {
        get => _turnCounter;
        set
        {
            AssertMutable();
            _turnCounter = value;
            InvokeDisplayAmountChanged();
        }
    }

    public override Task BeforeCombatStart()
    {
        TurnCounter = 0;
        return Task.CompletedTask;
    }

    public override Task AfterCombatEnd(CombatRoom _)
    {
        TurnCounter = 0;
        return Task.CompletedTask;
    }

    public override Task AfterPlayerTurnStartEarly(PlayerChoiceContext choiceContext, Player player)
    {
        if (player != Owner || Owner == null || !LightMechanic.IsMechanicActive(Owner))
            return Task.CompletedTask;

        TurnCounter++;
        if (TurnCounter % Interval != 0)
            return Task.CompletedTask;

        var state = LightMechanic.GetActiveState(Owner);
        if (state == null)
            return Task.CompletedTask;

        var count = state.LightEnergy.Items.Count;
        if (count <= 0)
            return Task.CompletedTask;

        Flash();
        // 静默消耗，避免与光珀互相触发。
        LightMechanic.ConsumeAllLightEnergy(Owner, notify: false);
        for (var i = 0; i < count; i++)
            LightMechanic.TryAddAttributeCell(Owner, LightElement.Prismatic);

        return Task.CompletedTask;
    }
}
