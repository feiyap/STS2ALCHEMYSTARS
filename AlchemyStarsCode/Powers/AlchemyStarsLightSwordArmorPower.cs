using System.Threading.Tasks;
using AlchemyStars.Mechanics;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace AlchemyStars.Powers;

/// <summary>
/// 光之剑甲（挂在召唤物上）：消失时为卡牌召唤者生成 1 个森属性强化格。
/// </summary>
[RegisterPower]
public sealed class AlchemyStarsLightSwordArmorPower : AlchemyStarsPowerBase
{
    private Player? _rewardPlayer;

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;

    public override bool ShouldPlayVfx => false;

    public override bool ShouldPowerBeRemovedAfterOwnerDeath() => false;

    internal void Configure(Player rewardPlayer) => _rewardPlayer = rewardPlayer;

    public override Task AfterDeath(
        PlayerChoiceContext choiceContext,
        Creature creature,
        bool wasRemovalPrevented,
        float deathAnimLength)
    {
        if (creature != Owner || wasRemovalPrevented)
            return Task.CompletedTask;

        var rewardPlayer = _rewardPlayer ?? Owner.PetOwner;
        if (rewardPlayer != null && LightMechanic.HasMechanicRelic(rewardPlayer))
        {
            LightMechanic.TryAddAttributeCell(
                rewardPlayer,
                LightElement.Forest,
                AttributeCellKind.Enhanced);
        }

        return Task.CompletedTask;
    }
}
