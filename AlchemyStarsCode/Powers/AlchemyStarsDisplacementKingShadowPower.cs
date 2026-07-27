using System.Collections.Generic;
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
/// 易位王影：为队友承担伤害，并将自身森属性伤害加成转移给队友�?/// </summary>
[RegisterPower]
public sealed class AlchemyStarsDisplacementKingShadowPower : ModPowerTemplate
{
    private Creature? _protectedAlly;
    private bool _isUpgraded;
    private bool _shareForestBonusWithAllAllies;

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;

    protected override IEnumerable<string> RegisteredKeywordIds => ["displacement_king_shadow"];

    internal void Configure(Creature protectedAlly, bool isUpgraded = false)
    {
        _protectedAlly = protectedAlly;
        _isUpgraded = isUpgraded;
    }

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player.Creature != Owner)
            return;

        _shareForestBonusWithAllAllies = _isUpgraded && Owner.Block > 0;
        await Task.CompletedTask;
    }

    public override async Task AfterDamageReceived(
        PlayerChoiceContext choiceContext,
        Creature target,
        DamageResult result,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource)
    {
        if (_protectedAlly == null || target != _protectedAlly || result.UnblockedDamage <= 0m || Owner.IsDead)
            return;

        await CreatureCmd.Damage(
            choiceContext,
            Owner,
            (decimal)result.UnblockedDamage,
            props,
            dealer);

        await CreatureCmd.Heal(target, result.UnblockedDamage);
    }

    public override decimal ModifyDamageMultiplicative(
        Creature? target,
        decimal amount,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource,
        CardPlay? cardPlay)
    {
        if (!props.IsPoweredAttack() || dealer?.Player == null)
            return 1m;

        var player = Owner.Player;
        if (player == null || !LightMechanic.HasMechanicRelic(player))
            return 1m;

        var forestMultiplier = LightMechanic.GetOutgoingDamageMultiplier(player, LightElement.Forest);
        if (forestMultiplier <= 1m)
            return 1m;

        if (_shareForestBonusWithAllAllies &&
            dealer != Owner &&
            dealer.IsPlayer &&
            dealer.CombatState?.PlayerCreatures.Contains(dealer) == true)
        {
            return forestMultiplier;
        }

        if (dealer == _protectedAlly)
            return forestMultiplier;

        if (dealer == Owner && LightMechanicDamageContext.CurrentElement == LightElement.Forest)
            return 1m / forestMultiplier;

        return 1m;
    }
}
