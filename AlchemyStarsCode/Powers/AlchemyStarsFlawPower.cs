using System.Collections.Generic;
using System.Threading.Tasks;
using AlchemyStars.Cards;
using AlchemyStars.Mechanics;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace AlchemyStars.Powers;

/// <summary>
/// ????????????????????????????/// </summary>
[RegisterPower]
public sealed class AlchemyStarsFlawPower : ModPowerTemplate
{
    private const decimal GoldPerBreak = 5m;

    public override PowerType Type => PowerType.Debuff;

    public override PowerStackType StackType => PowerStackType.Counter;

    protected override IEnumerable<string> RegisteredKeywordIds => ["flaw"];

    public override async Task AfterDamageReceived(
        PlayerChoiceContext choiceContext,
        Creature target,
        DamageResult result,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource)
    {
        if (target != Owner || Amount <= 0 || result.TotalDamage <= 0 || dealer == null)
            return;

        if (!props.IsPoweredAttack())
            return;

        if (dealer.Side != Owner.Side || ReferenceEquals(dealer, Owner))
            return;

        var sourcePlayer = cardSource?.Owner ?? dealer.Player;
        if (sourcePlayer == null)
            return;

        Flash();
        await PowerCmd.Decrement(this);

        using (LightMechanicDamageContext.Use(LightElement.Thunder))
        {
            await CreatureCmd.Damage(
                choiceContext,
                Owner,
                1m,
                ValueProp.Unblockable | ValueProp.Unpowered,
                cardSource,
                null);
        }

        var combatState = Owner.CombatState;
        if (combatState == null)
            return;

        foreach (var player in combatState.RunState.Players)
            await PlayerCmd.GainGold(GoldPerBreak, player);
    }
}
