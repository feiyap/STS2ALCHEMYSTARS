using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Rooms;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace AlchemyStars.Powers;

/// <summary>
/// 至高宝典：本场战斗结束时随机升级 Amount 张牌。
/// </summary>
[RegisterPower]
public sealed class AlchemyStarsSupremeCodexPower : AlchemyStarsPowerBase
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    /// <summary>
    /// 必须用 AfterCombatEnd：引擎会在 AfterCombatVictory 前清掉能力。
    /// </summary>
    public override Task AfterCombatEnd(CombatRoom room)
    {
        var player = Owner.Player;
        if (player == null || Amount <= 0)
            return Task.CompletedTask;

        var upgradeCount = (int)Amount;
        Flash();

        for (var i = 0; i < upgradeCount; i++)
        {
            var upgradable = PileType.Deck.GetPile(player).Cards
                .Where(card => card.IsUpgradable)
                .ToList();
            if (upgradable.Count == 0)
                break;

            var pick = player.RunState.Rng.CombatCardSelection.NextItem(upgradable);
            if (pick == null)
                break;

            CardCmd.Upgrade(pick);
        }

        return Task.CompletedTask;
    }
}
