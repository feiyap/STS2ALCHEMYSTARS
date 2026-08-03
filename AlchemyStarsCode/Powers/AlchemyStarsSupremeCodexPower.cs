using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Rooms;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace AlchemyStars.Powers;

/// <summary>
/// 至高宝典：本场战斗结束时随机升级 1 张牌。
/// </summary>
[RegisterPower]
public sealed class AlchemyStarsSupremeCodexPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;

    public override async Task AfterCombatVictory(CombatRoom room)
    {
        var player = Owner.Player;
        if (player == null)
            return;

        var upgradable = player.Deck.Cards.Where(card => card.IsUpgradable).ToList();
        if (upgradable.Count == 0)
            return;

        Flash();
        var pick = upgradable[player.RunState.Rng.Niche.NextInt(upgradable.Count)];
        CardCmd.Upgrade(pick);
    }
}
