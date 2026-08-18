using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AlchemyStars.Mechanics;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace AlchemyStars.Powers;

/// <summary>
/// 易位王影：为队友承担未被格挡的生命伤害，并将自身森属性格伤害加成转移给对方。回合开始时移除。
/// </summary>
[RegisterPower]
public sealed class AlchemyStarsDisplacementKingShadowPower : ModPowerTemplate
{
    private Creature? _protectedAlly;
    private Player? _protectedAllyPlayer;
    private bool _isUpgraded;
    private bool _shareForestBonusWithAllAllies;

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;

    internal void Configure(Creature protectedAlly, bool isUpgraded = false)
    {
        _protectedAlly = protectedAlly;
        _protectedAllyPlayer = protectedAlly.Player;
        _isUpgraded = isUpgraded;
    }

    public override async Task BeforeSideTurnStart(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        IReadOnlyList<Creature> participants,
        ICombatState combatState)
    {
        if (side != CombatSide.Player || !participants.Contains(Owner))
            return;

        // 在格挡清空前判定升级；有格挡则本回合全体队友分享加成，下个回合开始再移除。
        if (_isUpgraded && !_shareForestBonusWithAllAllies && Owner.Block > 0)
        {
            _shareForestBonusWithAllAllies = true;
            return;
        }

        Flash();
        await PowerCmd.Remove(this);
    }

    public override Creature ModifyUnblockedDamageTarget(
        Creature target,
        decimal amount,
        ValueProp props,
        Creature? dealer)
    {
        if (amount <= 0m || Owner.IsDead || target == Owner || dealer == Owner)
            return target;

        if (!IsProtectedTarget(target))
            return target;

        Flash();
        return Owner;
    }

    public override decimal ModifyDamageMultiplicative(
        Creature? target,
        decimal amount,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource,
        CardPlay? cardPlay)
    {
        if (dealer == null || !IsForestOutgoingDamage())
            return 1m;

        var ownerPlayer = Owner.Player;
        if (ownerPlayer == null || !LightMechanic.HasMechanicRelic(ownerPlayer))
            return 1m;

        var forestMultiplier = GetOwnerForestCellMultiplier(ownerPlayer);
        if (forestMultiplier <= 1m)
            return 1m;

        if (IsBonusRecipient(dealer))
            return forestMultiplier;

        if (dealer == Owner)
            return 1m / forestMultiplier;

        return 1m;
    }

    private bool IsProtectedTarget(Creature target)
    {
        if (_protectedAlly != null && target == _protectedAlly)
            return true;

        return _protectedAllyPlayer != null &&
               target.Player == _protectedAllyPlayer &&
               target != Owner;
    }

    private bool IsBonusRecipient(Creature dealer)
    {
        if (dealer == Owner)
            return false;

        if (_shareForestBonusWithAllAllies &&
            dealer.IsPlayer &&
            dealer.Side == Owner.Side)
        {
            return true;
        }

        if (_protectedAlly != null && dealer == _protectedAlly)
            return true;

        return _protectedAllyPlayer != null && dealer.Player == _protectedAllyPlayer;
    }

    private static bool IsForestOutgoingDamage()
    {
        if (LightMechanicDamageContext.IsFireAndThunder)
            return false;

        var element = LightMechanicDamageContext.CurrentElement;
        return element is LightElement.Forest or LightElement.Prismatic;
    }

    private static decimal GetOwnerForestCellMultiplier(Player ownerPlayer)
    {
        var count = LightMechanic.CountForestAttributeCells(ownerPlayer);
        if (count <= 0)
            return 1m;

        return 1m + count * 0.04m;
    }
}
