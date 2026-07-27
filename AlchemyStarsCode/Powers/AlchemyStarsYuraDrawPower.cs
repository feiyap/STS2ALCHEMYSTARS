using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace AlchemyStars.Powers;

/// <summary>
/// 缘木求叶：下个回合开始时抽牌�?/// </summary>
[RegisterPower]
public sealed class AlchemyStarsYuraDrawPower : ModPowerTemplate
{
    private int _drawCount = 1;

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;

    public void Configure(int drawCount) => _drawCount = drawCount;

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player.Creature != Owner || _drawCount <= 0)
            return;

        await CardPileCmd.Draw(choiceContext, _drawCount, player);
        await PowerCmd.Remove(this);
    }
}
