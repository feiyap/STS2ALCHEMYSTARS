using System.Threading.Tasks;
using AlchemyStars.Mechanics;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace AlchemyStars.Powers;

/// <summary>
/// 闪电机蜂：每回合开始时消耗 1 层，获得 1 点雷属性光能并抽 1 张牌。
/// </summary>
[RegisterPower]
public sealed class AlchemyStarsLightningBeePower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player.Creature != Owner || Amount <= 0)
            return;

        Flash();
        await PowerCmd.Decrement(this);
        LightMechanic.TryGrantLightEnergy(player, LightElement.Thunder);
        await CardPileCmd.Draw(choiceContext, 1, player);
    }
}
