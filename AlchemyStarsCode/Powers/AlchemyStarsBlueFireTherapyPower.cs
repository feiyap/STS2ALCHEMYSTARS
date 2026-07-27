using System.Threading.Tasks;
using AlchemyStars.Mechanics;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace AlchemyStars.Powers;

/// <summary>
/// 蓝火疗心：限时内损失生命时消耗火光能，将一半损失转化为覆甲。
/// Amount = 剩余回合数。
/// </summary>
[RegisterPower]
public sealed class AlchemyStarsBlueFireTherapyPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterDamageReceived(
        PlayerChoiceContext choiceContext,
        Creature? dealer,
        DamageResult result,
        ValueProp props,
        Creature target,
        CardModel? cardSource)
    {
        if (target != Owner || Amount <= 0 || result.UnblockedDamage <= 0)
            return;

        var player = Owner.Player;
        if (player == null || !LightMechanic.HasFireLightEnergy(player))
            return;

        if (!LightMechanic.TryConsumeLightEnergy(player, [LightElement.Fire]))
            return;

        Flash();
        var plating = (int)System.Math.Ceiling(result.UnblockedDamage * 0.5m);
        if (plating > 0)
        {
            await PowerCmd.Apply<PlatingPower>(
                choiceContext,
                Owner,
                plating,
                Owner,
                cardSource);
        }
    }

    public override async Task AfterPlayerTurnStart(
        PlayerChoiceContext choiceContext,
        MegaCrit.Sts2.Core.Entities.Players.Player player)
    {
        if (player.Creature != Owner || Amount <= 0)
            return;

        await PowerCmd.Decrement(this);
    }
}
