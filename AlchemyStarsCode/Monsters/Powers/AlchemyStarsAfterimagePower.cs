using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using STS2RitsuLib.Interop.AutoRegistration;

namespace AlchemyStars.Monsters.Powers;

/// <summary>
/// 残影：玩家每次攻击时，随机丢弃 1 张手牌。
/// </summary>
[RegisterPower]
public sealed class AlchemyStarsAfterimagePower : AlchemyStarsSwordAltarPowerBase
{
    protected override string PlaceholderIcon => "AlchemyStarsAfterimagePower";

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;

    public override int DisplayAmount => 0;

    public override async Task AfterAttack(PlayerChoiceContext choiceContext, AttackCommand command)
    {
        var player = command.Attacker?.Player;
        if (player == null)
            return;

        var hand = PileType.Hand.GetPile(player).Cards.ToList();
        if (hand.Count == 0)
            return;

        var card = player.RunState.Rng.CombatCardSelection.NextItem(hand);
        if (card != null)
            await CardCmd.Discard(choiceContext, card);
    }
}
