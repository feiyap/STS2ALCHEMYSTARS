using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace AlchemyStars.Powers;

/// <summary>
/// 冥河列车·卡戎：下次洗牌时，消耗一半抽牌堆（优先状态与诅咒）。
/// </summary>
[RegisterPower]
public sealed class AlchemyStarsDeadCelebrationPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;

    public override async Task AfterShuffle(PlayerChoiceContext choiceContext, Player shuffler)
    {
        if (shuffler.Creature != Owner)
            return;

        var combat = shuffler.PlayerCombatState;
        if (combat == null)
        {
            await PowerCmd.Remove(this);
            return;
        }

        var draw = combat.DrawPile.Cards.ToList();
        var removeCount = draw.Count / 2;
        if (removeCount <= 0)
        {
            await PowerCmd.Remove(this);
            return;
        }

        Flash();
        var prioritized = draw
            .OrderByDescending(card => card.Type is CardType.Status or CardType.Curse)
            .Take(removeCount)
            .ToList();

        foreach (var card in prioritized)
            await CardPileCmd.Add(card, PileType.Exhaust);

        await PowerCmd.Remove(this);
    }
}
