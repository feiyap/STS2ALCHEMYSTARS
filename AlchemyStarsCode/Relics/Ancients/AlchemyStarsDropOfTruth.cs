using AlchemyStars.Characters;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Saves.Runs;
using STS2RitsuLib.Interop.AutoRegistration;

namespace AlchemyStars.Relics.Ancients;

/// <summary>
/// 真理之滴：仿蜥蜴尾巴，以约 50% 生命复活一次；拾起时加入笨拙。
/// </summary>
[RegisterRelic(typeof(AlchemyStarsRelicPool))]
public sealed class AlchemyStarsDropOfTruth : AlchemyStarsAncientRelicBase
{
    private bool _wasUsed;

    public override bool HasUponPickupEffect => true;

    public override bool IsUsedUp => _wasUsed;

    [SavedProperty]
    public bool WasUsed
    {
        get => _wasUsed;
        set
        {
            AssertMutable();
            _wasUsed = value;
            if (IsUsedUp)
                Status = RelicStatus.Disabled;
        }
    }

    public override async Task AfterObtained()
    {
        if (Owner == null)
            return;

        await CardPileCmd.AddCurseToDeck<Clumsy>(Owner);
        Flash();
    }

    public override bool ShouldDieLate(Creature creature)
    {
        if (Owner == null || creature != Owner.Creature)
            return true;

        return WasUsed;
    }

    public override async Task AfterPreventingDeath(Creature creature)
    {
        Flash();
        WasUsed = true;
        var amount = Math.Max(1m, creature.MaxHp * 0.5m);
        await CreatureCmd.Heal(creature, amount);
    }
}
