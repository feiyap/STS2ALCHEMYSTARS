using System.Threading.Tasks;
using AlchemyStars.Mechanics;
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
/// 破绽：被施加者的队友攻击命中时破碎，以施加者为来源造成 1 点雷伤并全员获金。
/// </summary>
[RegisterPower]
public sealed class AlchemyStarsFlawPower : ModPowerTemplate
{
    private const decimal GoldPerBreak = 5m;

    private bool _isBreaking;

    public override PowerType Type => PowerType.Debuff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterDamageReceived(
        PlayerChoiceContext choiceContext,
        Creature target,
        DamageResult result,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource)
    {
        if (_isBreaking || target != Owner || Amount <= 0 || result.TotalDamage <= 0 || dealer == null)
            return;

        if (!props.IsPoweredAttack())
            return;

        var applier = Applier;
        if (applier == null || !IsTeammateHit(applier, dealer))
            return;

        var applierPlayer = applier.Player;
        if (applierPlayer == null)
            return;

        _isBreaking = true;
        try
        {
            Flash();
            await PowerCmd.Decrement(this);

            await LightMechanic.DealElementalAttackDamage(
                choiceContext,
                applierPlayer,
                card: null,
                Owner,
                1m,
                LightElement.Thunder);

            var combatState = Owner.CombatState;
            if (combatState == null)
                return;

            foreach (var player in combatState.RunState.Players)
                await PlayerCmd.GainGold(GoldPerBreak, player);
        }
        finally
        {
            _isBreaking = false;
        }
    }

    /// <summary>
    /// 队友：与施加者同阵营的其他玩家，不含施加者本人。
    /// </summary>
    private static bool IsTeammateHit(Creature applier, Creature dealer)
    {
        if (!dealer.IsPlayer || ReferenceEquals(dealer, applier))
            return false;

        return dealer.Side == applier.Side;
    }
}
