using System.Linq;
using AlchemyStars.Cards;
using AlchemyStars.Characters;
using AlchemyStars.Mechanics;
using AlchemyStars.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Saves.Runs;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;

namespace AlchemyStars.Relics.Ancients;

/// <summary>
/// 稍微拨动那汪流动：改写四属性格的被动结算。
/// </summary>
[RegisterRelic(typeof(AlchemyStarsRelicPool))]
public sealed class AlchemyStarsNudgeTheFlow : AlchemyStarsAncientRelicBase
{
    private bool _waterBurstUsed;

    [SavedProperty]
    public bool WaterBurstUsedThisCombat
    {
        get => _waterBurstUsed;
        set
        {
            AssertMutable();
            _waterBurstUsed = value;
        }
    }

    public override Task BeforeCombatStart()
    {
        WaterBurstUsedThisCombat = false;
        return Task.CompletedTask;
    }

    public static bool IsActive(Player player) =>
        player.GetRelic<AlchemyStarsNudgeTheFlow>() != null;

    public static bool AnyActive(Creature? creatureInCombat)
    {
        var players = creatureInCombat?.CombatState?.Players;
        return players != null && players.Any(IsActive);
    }

    /// <summary>
    /// 森：仅按森属性手牌提供格挡，每张 2 点。
    /// </summary>
    public static async Task ResolveForestBlockOverride(Player player)
    {
        var state = LightMechanic.GetActiveState(player);
        if (state == null || state.GetEffectiveCount(LightElement.Forest) / 4 <= 0)
            return;

        var forestCards = PileType.Hand.GetPile(player).Cards.Count(AlchemyStarsCardHelpers.HasForestKeyword);
        var block = forestCards * 2;
        if (block > 0)
            await CreatureCmd.GainBlock(player.Creature, new BlockVar(block, ValueProp.Move), null);
    }

    /// <summary>
    /// 水：首次满 4 格水属性时回复 20% 最大生命，此后本场不再回血。
    /// </summary>
    public static async Task ResolveWaterOverride(Player player)
    {
        var relic = player.GetRelic<AlchemyStarsNudgeTheFlow>();
        if (relic == null)
            return;

        var state = LightMechanic.GetActiveState(player);
        if (state == null || relic.WaterBurstUsedThisCombat)
            return;

        if (state.GetEffectiveCount(LightElement.Water) < 4)
            return;

        relic.Flash();
        relic.WaterBurstUsedThisCombat = true;
        var heal = Math.Max(1m, Math.Floor(player.Creature.MaxHp * 0.2m));
        await CreatureCmd.Heal(player.Creature, heal);
    }

    public static decimal ModifyScorchDamage(Creature victim, decimal damage) =>
        AnyActive(victim) ? damage * 2m : damage;

    public static void CapScorchStacks(AlchemyStarsScorchPower power)
    {
        if (!AnyActive(power.Owner) || power.Amount <= 14)
            return;

        power.Amount = 14;
    }

    public static async Task AfterParalysisApplied(Creature target, Player source)
    {
        if (!IsActive(source))
            return;

        var paralysis = target.GetPower<AlchemyStarsParalysisPower>();
        if (paralysis == null || paralysis.Amount < 30)
            return;

        source.GetRelic<AlchemyStarsNudgeTheFlow>()?.Flash();
        await PowerCmd.Remove<AlchemyStarsParalysisPower>(target);
        await CreatureCmd.Stun(target);
    }
}
