using System.Linq;
using AlchemyStars.Cards;
using AlchemyStars.Characters;
using AlchemyStars.Keywords;
using AlchemyStars.Mechanics;
using AlchemyStars.Powers;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Saves.Runs;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;

namespace AlchemyStars.Relics.Ancients;

/// <summary>
/// 故我自在莫比乌斯：按属性连击组合触发效果；连击中断则本场战斗失效。
/// </summary>
[RegisterRelic(typeof(AlchemyStarsRelicPool))]
public sealed class AlchemyStarsMobiusOfSelf : AlchemyStarsAncientRelicBase
{
    private static readonly string[] Recipes =
    [
        "FFFF", "WWWW", "TTTT", "SSSS",
        "SFT", "FWT", "TSW", "PPP",
    ];

    /// <summary>连招细则放到悬停提示栏（同先天枷锁的光能/属性格词条）。</summary>
    protected override IEnumerable<string> RegisteredKeywordIds =>
    [
        AlchemyStarsKeywordIds.MobiusCombo,
    ];

    private bool _broken;
    private string _combo = "";

    public override bool IsUsedUp => _broken;

    /// <summary>本场战斗是否已因断连击失效（跨战斗会重置）。</summary>
    [SavedProperty]
    public bool Broken
    {
        get => _broken;
        set
        {
            AssertMutable();
            _broken = value;
            Status = value ? RelicStatus.Disabled : RelicStatus.Active;
        }
    }

    [SavedProperty]
    public string Combo
    {
        get => _combo;
        set
        {
            AssertMutable();
            _combo = value ?? "";
        }
    }

    public override Task BeforeCombatStart()
    {
        Broken = false;
        Combo = "";
        return Task.CompletedTask;
    }

    public override Task AfterCombatEnd(CombatRoom _)
    {
        Broken = false;
        Combo = "";
        return Task.CompletedTask;
    }

    /// <summary>
    /// 连招不能跨回合：玩家回合开始时清空（不因此失效）。
    /// </summary>
    public override Task AfterPlayerTurnStartEarly(PlayerChoiceContext choiceContext, Player player)
    {
        if (player == Owner && !Broken)
            Combo = "";
        return Task.CompletedTask;
    }

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (Broken || Owner == null || cardPlay.Card.Owner != Owner)
            return;

        var token = ResolveToken(cardPlay.Card);
        if (token == null)
        {
            BreakCombo();
            return;
        }

        var candidate = TrimToValidPrefix(Combo + token);
        if (candidate == null)
        {
            BreakCombo();
            return;
        }

        Combo = candidate;
        Flash();
        await TryTriggerRecipes(choiceContext, Combo);
    }

    private void BreakCombo()
    {
        if (Broken)
            return;
        Flash();
        Broken = true;
        Combo = "";
    }

    private async Task TryTriggerRecipes(PlayerChoiceContext choiceContext, string combo)
    {
        if (Owner == null)
            return;

        if (combo.EndsWith("FFFF", StringComparison.Ordinal))
        {
            await PowerCmd.Apply<StrengthPower>(choiceContext, Owner.Creature, 1m, Owner.Creature, null);
            Combo = "";
            return;
        }

        if (combo.EndsWith("WWWW", StringComparison.Ordinal))
        {
            await CreatureCmd.Heal(Owner.Creature, 2m);
            Combo = "";
            return;
        }

        if (combo.EndsWith("TTTT", StringComparison.Ordinal))
        {
            await PowerCmd.Apply<AlchemyStarsMobiusExtraHitPower>(choiceContext, Owner.Creature, 2m, Owner.Creature, null);
            Combo = "";
            return;
        }

        if (combo.EndsWith("SSSS", StringComparison.Ordinal))
        {
            await PowerCmd.Apply<DexterityPower>(choiceContext, Owner.Creature, 1m, Owner.Creature, null);
            Combo = "";
            return;
        }

        if (combo.EndsWith("SFT", StringComparison.Ordinal))
        {
            foreach (var enemy in Owner.Creature.CombatState?.HittableEnemies.ToList() ?? [])
            {
                if (enemy.IsDead) continue;
                await PowerCmd.Apply<AlchemyStarsScorchPower>(choiceContext, enemy, 3m, Owner.Creature, null);
                await PowerCmd.Apply<StrengthPower>(choiceContext, enemy, -1m, Owner.Creature, null);
            }

            Combo = "";
            return;
        }

        if (combo.EndsWith("FWT", StringComparison.Ordinal))
        {
            foreach (var enemy in Owner.Creature.CombatState?.HittableEnemies.ToList() ?? [])
            {
                if (enemy.IsDead) continue;
                await PowerCmd.Apply<SlowPower>(choiceContext, enemy, 1m, Owner.Creature, null);
                await PowerCmd.Apply<StrengthPower>(choiceContext, enemy, -1m, Owner.Creature, null);
            }

            Combo = "";
            return;
        }

        if (combo.EndsWith("TSW", StringComparison.Ordinal))
        {
            foreach (var enemy in Owner.Creature.CombatState?.HittableEnemies.ToList() ?? [])
            {
                if (enemy.IsDead) continue;
                await CreatureCmd.Stun(enemy);
                await PowerCmd.Apply<StrengthPower>(choiceContext, enemy, -1m, Owner.Creature, null);
            }

            Combo = "";
            return;
        }

        if (combo.EndsWith("PPP", StringComparison.Ordinal))
        {
            foreach (var enemy in Owner.Creature.CombatState?.HittableEnemies.ToList() ?? [])
            {
                if (enemy.IsDead) continue;
                using (LightMechanicDamageContext.Use(LightElement.Prismatic))
                {
                    await CreatureCmd.Damage(
                        choiceContext,
                        enemy,
                        40m,
                        ValueProp.Unblockable | ValueProp.Unpowered,
                        Owner.Creature);
                }
            }

            Combo = "";
        }
    }

    private static string? ResolveToken(CardModel card)
    {
        var prism = ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.Prismatic);
        if (card.Keywords.Contains(prism))
            return "P";
        if (AlchemyStarsCardHelpers.HasFireKeyword(card))
            return "F";
        if (AlchemyStarsCardHelpers.HasWaterKeyword(card))
            return "W";
        if (AlchemyStarsCardHelpers.HasThunderKeyword(card))
            return "T";
        if (AlchemyStarsCardHelpers.HasForestKeyword(card))
            return "S";
        return null;
    }

    private static string? TrimToValidPrefix(string combo)
    {
        for (var start = 0; start < combo.Length; start++)
        {
            var suffix = combo[start..];
            if (Recipes.Any(r => r.StartsWith(suffix, StringComparison.Ordinal)))
                return suffix;
        }

        return null;
    }
}
