using System.Linq;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using AlchemyStars.Powers;

namespace AlchemyStars.Mechanics;

/// <summary>
/// 光能/属性格机制的静态入口�?
/// </summary>
public static class LightMechanic
{
    public static bool HasMechanicRelic(Player player) =>
        player.GetRelic<Relics.AlchemyStarsLumenRelic>() != null ||
        player.GetRelic<Relics.AlchemyStarsLumenRelicUpgraded>() != null;

    public static int GetSlotLimit(Player player)
    {
        if (player.GetRelic<Relics.AlchemyStarsLumenRelicUpgraded>() != null)
            return 8;

        if (player.GetRelic<Relics.AlchemyStarsLumenRelic>() != null)
            return 4;

        return 0;
    }

    public static void InitializeForCombat(Player player)
    {
        var slotLimit = GetSlotLimit(player);
        if (slotLimit <= 0)
            return;

        var state = LightMechanicCombatState.Get(player);
        state.Configure(slotLimit);
        state.GrantStartingLightEnergy();
        LightMechanicUiBootstrap.RefreshForPlayer(player);
    }

    /// <summary>
    /// 在转色栏生成属性格；成功时刷新战斗 UI�?
    /// </summary>
    public static bool TryAddAttributeCell(
        Player player,
        LightElement element,
        AttributeCellKind kind = AttributeCellKind.Normal)
    {
        var state = GetActiveState(player);
        if (state == null)
            return false;

        if (kind == AttributeCellKind.Enhanced &&
            AlchemyStarsForestState.TryAbsorbEnhancedCellsForWordAbsolute(player, 1) > 0)
            return true;

        var overflow = state.AddAttributeCell(element, kind);
        if (overflow.Count > 0)
            NotifyAttributeCellsRemoved(player, overflow);

        LightMechanicUiBootstrap.RefreshForPlayer(player);

        if (element == LightElement.Forest && kind != AttributeCellKind.Enhanced)
            AlchemyStarsForestState.NotifyForestCellProduced(player);

        return true;
    }

    public static bool TryGrantLightEnergy(Player player, LightElement element)
    {
        var state = GetActiveState(player);
        if (state == null)
            return false;

        state.LightEnergy.Enqueue(element);
        LightMechanicUiBootstrap.RefreshForPlayer(player);
        return true;
    }

    public static bool TryGrantRandomBaseLightEnergy(Player player)
    {
        var elements = LightElementExtensions.BaseElements;
        var index = player.RunState.Rng.Niche.NextInt(elements.Length);
        return TryGrantLightEnergy(player, elements[index]);
    }

    public static void TryGrantLightEnergyMany(Player player, LightElement element, int amount)
    {
        for (var i = 0; i < amount; i++)
            TryGrantLightEnergy(player, element);
    }

    /// <summary>
    /// 随机转化非雷属性格为雷属性格；成功时刷新 UI�?
    /// </summary>
    public static int TryConvertRandomNonThunderCells(Player player, int maxCount) =>
        TryConvertRandomNonElementCells(player, LightElement.Thunder, maxCount);

    /// <summary>
    /// 随机转化非目标属性格为目标属性格；成功时刷新 UI�?
    /// </summary>
    public static int TryConvertRandomNonElementCells(Player player, LightElement targetElement, int maxCount)
    {
        var state = GetActiveState(player);
        if (state == null || maxCount <= 0)
            return 0;

        var converted = state.ConvertRandomNonElementCells(
            targetElement,
            maxCount,
            player.RunState.Rng.Niche);

        if (converted > 0)
            LightMechanicUiBootstrap.RefreshForPlayer(player);

        return converted;
    }

    /// <summary>
    /// 随机转化非森属性格为森属性格�?
    /// </summary>
    public static int TryConvertRandomNonForestCells(Player player, int maxCount) =>
        TryConvertRandomNonElementCells(player, LightElement.Forest, maxCount);

    /// <summary>
    /// 随机转化非水属性格为水属性格�?
    /// </summary>
    public static int TryConvertRandomNonWaterCells(Player player, int maxCount) =>
        TryConvertRandomNonElementCells(player, LightElement.Water, maxCount);

    public static bool HasWaterLightEnergy(Player player) =>
        HasLightEnergy(player, [LightElement.Water]);

    public static int CountWaterLightEnergy(Player player)
    {
        var state = GetActiveState(player);
        if (state == null)
            return 0;

        return state.LightEnergy.Items.Count(item =>
            LightElementExtensions.Matches(LightElement.Water, item));
    }

    /// <summary>
    /// 消耗全部水属性光能，并在转色栏为每个被消耗的点生成对应属性格�?
    /// </summary>
    public static int ConsumeAllWaterLightEnergy(Player player)
    {
        var state = GetActiveState(player);
        if (state == null)
            return 0;

        var waterEnergy = state.LightEnergy.Items
            .Where(item => LightElementExtensions.Matches(LightElement.Water, item))
            .ToList();
        if (waterEnergy.Count == 0)
            return 0;

        var remaining = state.LightEnergy.Items
            .Where(item => !LightElementExtensions.Matches(LightElement.Water, item))
            .ToList();
        state.LightEnergy.ReplaceAll(remaining);

        foreach (var element in waterEnergy)
            state.AddAttributeCell(element);

        LightMechanicUiBootstrap.RefreshForPlayer(player);
        NotifyLightEnergyConsumed(player, waterEnergy);
        return waterEnergy.Count;
    }

    /// <summary>
    /// 消耗全部光能（不生成属性格）。
    /// </summary>
    public static int ConsumeAllLightEnergy(Player player)
    {
        var state = GetActiveState(player);
        if (state == null)
            return 0;

        var consumed = state.LightEnergy.Items.ToList();
        if (consumed.Count == 0)
            return 0;

        state.LightEnergy.ReplaceAll([]);
        LightMechanicUiBootstrap.RefreshForPlayer(player);
        NotifyLightEnergyConsumed(player, consumed);
        return consumed.Count;
    }

    public static int CountWaterDarkCells(Player player)
    {
        var state = GetActiveState(player);
        if (state == null)
            return 0;

        return state.AttributeCells.Items.Count(cell =>
            cell.Element == LightElement.Water && cell.Kind == AttributeCellKind.Dark);
    }

    public static int CountEffectiveWaterCells(Player player)
    {
        var state = GetActiveState(player);
        if (state == null)
            return 0;

        return state.GetEffectiveCount(LightElement.Water);
    }

    /// <summary>
    /// 随机�?1 个水属性格转为水属性深色格�?
    /// </summary>
    public static bool TryConvertRandomWaterCellToDark(Player player)
    {
        var state = GetActiveState(player);
        if (state == null)
            return false;

        var converted = state.ConvertRandomNormalCellToDark(
            LightElement.Water,
            player.RunState.Rng.Niche,
            1);

        if (converted > 0)
            LightMechanicUiBootstrap.RefreshForPlayer(player);

        return converted > 0;
    }

    /// <summary>
    /// 重置所有非雷属性格，并按概率生成雷属性格或棱镜格�?
    /// </summary>
    /// <param name="normalChancePercent">中概率：普通雷属性格（与 prism 概率累加，总和不超�?100）�?/param>
    /// <param name="prismChancePercent">小概率：雷属性棱镜格�?/param>
    public static void ResetNonThunderCells(Player player, int normalChancePercent = 50, int prismChancePercent = 25)
    {
        var state = GetActiveState(player);
        if (state == null)
            return;

        state.ResetNonElementCells(
            LightElement.Thunder,
            player.RunState.Rng.Niche,
            normalChancePercent,
            prismChancePercent);
        LightMechanicUiBootstrap.RefreshForPlayer(player);
    }

    /// <summary>
    /// 重置所有非森属性格，大概率出现森格，中概率出现强化格�?
    /// </summary>
    public static int ResetNonForestCells(
        Player player,
        int normalChancePercent = 60,
        int enhancedChancePercent = 30,
        string? enhancedCardTypeName = null)
    {
        var state = GetActiveState(player);
        if (state == null)
            return 0;

        var reset = state.ResetNonElementCellsWithEnhanced(
            LightElement.Forest,
            player.RunState.Rng.Niche,
            normalChancePercent,
            enhancedChancePercent,
            enhancedCardTypeName);
        LightMechanicUiBootstrap.RefreshForPlayer(player);
        return reset;
    }

    /// <summary>
    /// 重置转色栏所有格子属性，大概率出现强化格�?
    /// </summary>
    public static int ResetAllCellsWithEnhanced(
        Player player,
        LightElement targetElement,
        int normalChancePercent = 60,
        int enhancedChancePercent = 30,
        string? enhancedCardTypeName = null)
    {
        var state = GetActiveState(player);
        if (state == null)
            return 0;

        var reset = state.ResetAllCellsWithEnhanced(
            targetElement,
            player.RunState.Rng.Niche,
            normalChancePercent,
            enhancedChancePercent,
            enhancedCardTypeName);
        LightMechanicUiBootstrap.RefreshForPlayer(player);
        return reset;
    }

    /// <summary>
    /// 将所有属性格转为森属性格，大概率出现强化格�?
    /// </summary>
    public static int ConvertAllCellsToForestWithEnhanced(
        Player player,
        int normalChancePercent = 60,
        int enhancedChancePercent = 30,
        string? enhancedCardTypeName = null)
    {
        var state = GetActiveState(player);
        if (state == null)
            return 0;

        var converted = state.ConvertAllCellsToElementWithEnhanced(
            LightElement.Forest,
            player.RunState.Rng.Niche,
            normalChancePercent,
            enhancedChancePercent,
            enhancedCardTypeName);
        LightMechanicUiBootstrap.RefreshForPlayer(player);
        return converted;
    }

    public static int CountForestAttributeCells(Player player)
    {
        var state = GetActiveState(player);
        if (state == null)
            return 0;

        return state.GetEffectiveCount(LightElement.Forest);
    }

    public static int CountForestEnhancedCells(Player player, string? cardTypeName = null)
    {
        var state = GetActiveState(player);
        if (state == null)
            return 0;

        return state.CountEnhancedCells(LightElement.Forest, cardTypeName);
    }

    public static int CountEffectiveForestCellsForDamage(Player player)
    {
        var state = GetActiveState(player);
        if (state == null)
            return 0;

        return state.CountForestCellsWeightedByEnhanced();
    }

    public static bool TryEnhanceRandomCell(
        Player player,
        LightElement element,
        string? enhancedCardTypeName = null)
    {
        var state = GetActiveState(player);
        if (state == null)
            return false;

        if (AlchemyStarsForestState.TryAbsorbEnhancedCellsForWordAbsolute(player, 1) > 0)
            return true;

        var enhanced = state.TryEnhanceRandomCell(
            element,
            player.RunState.Rng.Niche,
            enhancedCardTypeName);
        if (enhanced)
        {
            LightMechanicUiBootstrap.RefreshForPlayer(player);
            NotifyForestEnhancedCellGained(player, 1);
        }

        return enhanced;
    }

    /// <summary>
    /// 通知场上能力：玩家获得了森属性强化格（用于言绝等效果）�?
    /// </summary>
    public static void NotifyForestEnhancedCellGained(Player player, int count)
    {
        if (count <= 0)
            return;

        var creature = player.Creature;
        var power = creature.GetPower<AlchemyStarsWordAbsolutePower>();
        power?.NotifyEnhancedCellsGained(count);
    }

    public static int ConsumeAllForestEnhancedCells(Player player, string? cardTypeName = null)
    {
        var state = GetActiveState(player);
        if (state == null)
            return 0;

        var consumed = state.ConsumeAllEnhancedCells(LightElement.Forest, cardTypeName);
        if (consumed > 0)
            LightMechanicUiBootstrap.RefreshForPlayer(player);

        return consumed;
    }

    public static int ConvertAllCellsToForestEnhanced(Player player)
    {
        var state = GetActiveState(player);
        if (state == null)
            return 0;

        var converted = state.ConvertAllCellsToEnhanced(LightElement.Forest);
        LightMechanicUiBootstrap.RefreshForPlayer(player);
        return converted;
    }

    public static int CountWaterAttributeCells(Player player)
    {
        var state = GetActiveState(player);
        if (state == null)
            return 0;

        return state.AttributeCells.Items.Count(cell =>
            cell.Element is LightElement.Water or LightElement.Prismatic);
    }

    /// <summary>
    /// 转色栏中火或水（含万色）属性格数量。
    /// </summary>
    public static int CountFireAndWaterAttributeCells(Player player)
    {
        var state = GetActiveState(player);
        if (state == null)
            return 0;

        return state.AttributeCells.Items.Count(cell =>
            cell.Element is LightElement.Fire or LightElement.Water or LightElement.Prismatic);
    }

    /// <summary>
    /// 玩家拥有光能遗物时，确保战斗状态已创建并完成栏位配置�?
    /// </summary>
    internal static LightMechanicCombatState? GetActiveState(Player player)
    {
        if (!HasMechanicRelic(player))
            return null;

        var slotLimit = GetSlotLimit(player);
        if (slotLimit <= 0)
            return null;

        var state = LightMechanicCombatState.Get(player);
        if (state.LightEnergy.MaxSlots != slotLimit || state.AttributeCells.MaxSlots != slotLimit)
            state.Configure(slotLimit);

        return state;
    }

    /// <summary>
    /// 尝试消耗光能点；成功时每个被消耗的点在转色栏生成对应属性格�?
    /// </summary>
    public static bool HasLightEnergy(Player player, IReadOnlyList<LightElement> cost)
    {
        var state = GetActiveState(player);
        if (state == null)
            return false;

        var working = state.LightEnergy.Items.ToList();
        foreach (var required in cost)
        {
            var index = working.FindIndex(item => LightElementExtensions.Matches(required, item));
            if (index < 0)
                return false;

            working.RemoveAt(index);
        }

        return true;
    }

    public static bool HasThunderLightEnergy(Player player) =>
        HasLightEnergy(player, [LightElement.Thunder]);

    public static bool HasFireLightEnergy(Player player) =>
        HasLightEnergy(player, [LightElement.Fire]);

    public static bool HasFireLightEnergyCount(Player player, int count)
    {
        var state = GetActiveState(player);
        if (state == null || count <= 0)
            return false;

        var fireCount = state.LightEnergy.Items.Count(item =>
            LightElementExtensions.Matches(LightElement.Fire, item));
        return fireCount >= count;
    }

    public static int CountFireLightEnergy(Player player)
    {
        var state = GetActiveState(player);
        if (state == null)
            return 0;

        return state.LightEnergy.Items.Count(item =>
            LightElementExtensions.Matches(LightElement.Fire, item));
    }

    public static int CountFireAttributeCells(Player player)
    {
        var state = GetActiveState(player);
        if (state == null)
            return 0;

        return state.AttributeCells.Items.Count(cell =>
            cell.Element is LightElement.Fire or LightElement.Prismatic);
    }

    public static int CountEffectiveFireCells(Player player)
    {
        var state = GetActiveState(player);
        if (state == null)
            return 0;

        return state.GetEffectiveCount(LightElement.Fire);
    }

    /// <summary>
    /// 随机转化非火属性格为火属性格。
    /// </summary>
    public static int TryConvertRandomNonFireCells(Player player, int maxCount) =>
        TryConvertRandomNonElementCells(player, LightElement.Fire, maxCount);

    /// <summary>
    /// 重置所有非火属性格，大概率出现火属性格。
    /// </summary>
    public static void ResetNonFireCells(Player player, int normalChancePercent = 65, int prismChancePercent = 10)
    {
        var state = GetActiveState(player);
        if (state == null)
            return;

        state.ResetNonElementCells(
            LightElement.Fire,
            player.RunState.Rng.Niche,
            normalChancePercent,
            prismChancePercent);
        LightMechanicUiBootstrap.RefreshForPlayer(player);
    }

    /// <summary>
    /// 以随机属性填满转色栏至上限；可中概率出现深色格。
    /// </summary>
    public static void FillAttributeBarWithRandomElements(
        Player player,
        int darkChancePercent = 0,
        bool darkOnly = false)
    {
        var state = GetActiveState(player);
        if (state == null)
            return;

        state.FillAttributeBarWithRandomElements(
            player.RunState.Rng.Niche,
            darkChancePercent,
            darkOnly);
        LightMechanicUiBootstrap.RefreshForPlayer(player);
    }

    public static bool HasForestLightEnergy(Player player) =>
        HasLightEnergy(player, [LightElement.Forest]);

    public static bool HasForestLightEnergyCount(Player player, int count)
    {
        var state = GetActiveState(player);
        if (state == null || count <= 0)
            return false;

        var forestCount = state.LightEnergy.Items.Count(item =>
            LightElementExtensions.Matches(LightElement.Forest, item));
        return forestCount >= count;
    }

    public static int CountForestLightEnergy(Player player)
    {
        var state = GetActiveState(player);
        if (state == null)
            return 0;

        return state.LightEnergy.Items.Count(item =>
            LightElementExtensions.Matches(LightElement.Forest, item));
    }

    public static bool HasThunderLightEnergyCount(Player player, int count)
    {
        var state = GetActiveState(player);
        if (state == null || count <= 0)
            return false;

        var thunderCount = state.LightEnergy.Items.Count(item =>
            LightElementExtensions.Matches(LightElement.Thunder, item));
        return thunderCount >= count;
    }

    public static int CountThunderLightEnergy(Player player)
    {
        var state = GetActiveState(player);
        if (state == null)
            return 0;

        return state.LightEnergy.Items.Count(item =>
            LightElementExtensions.Matches(LightElement.Thunder, item));
    }

    public static void RecordThunderDamageDealt(Player player, int hits = 1)
    {
        var state = GetActiveState(player);
        if (state == null || hits <= 0)
            return;

        state.ThunderDamageDealtThisTurn += hits;
    }

    public static int GetThunderDamageDealtThisTurn(Player player) =>
        GetActiveState(player)?.ThunderDamageDealtThisTurn ?? 0;

    public static void ResetTurnCounters(Player player)
    {
        var state = GetActiveState(player);
        state?.ResetTurnCounters();
    }

    /// <summary>
    /// 如意神雷：全格转雷，已有雷格加强为深色格�?
    /// </summary>
    public static int TryConvertAllCellsAuspiciousThunder(Player player)
    {
        var state = GetActiveState(player);
        if (state == null)
            return 0;

        var converted = state.ConvertAllCellsToThunderWithDarkUpgrade();
        if (converted > 0)
            LightMechanicUiBootstrap.RefreshForPlayer(player);

        return converted;
    }

    /// <summary>
    /// 将所有非目标属性格转为目标属性格，小概率生成深色格�?
    /// </summary>
    public static int TryConvertAllCellsToElement(
        Player player,
        LightElement targetElement,
        int darkChancePercent = 15)
    {
        var state = GetActiveState(player);
        if (state == null)
            return 0;

        var converted = state.ConvertAllCellsToElement(
            targetElement,
            player.RunState.Rng.Niche,
            darkChancePercent);

        if (converted > 0)
            LightMechanicUiBootstrap.RefreshForPlayer(player);

        return converted;
    }

    /// <summary>
    /// 随机转化属性格为雷格，已是雷格时小概率变为深色格�?
    /// </summary>
    public static (int Converted, int DarkCreated) TryConvertRandomThunderCellsWithDark(
        Player player,
        int maxCount,
        int darkChancePercent = 25)
    {
        var state = GetActiveState(player);
        if (state == null || maxCount <= 0)
            return (0, 0);

        var result = state.ConvertRandomCellsToThunderWithDark(
            maxCount,
            player.RunState.Rng.Niche,
            darkChancePercent);

        if (result.Converted > 0)
            LightMechanicUiBootstrap.RefreshForPlayer(player);

        return result;
    }

    public static bool TryConsumeLightEnergy(Player player, IReadOnlyList<LightElement> cost)
    {
        var state = GetActiveState(player);
        if (state == null || !state.LightEnergy.TryConsumeManyFromFront(cost, out var consumed))
            return false;

        foreach (var element in consumed)
        {
            state.AddAttributeCell(element);
            if (element == LightElement.Forest)
                AlchemyStarsForestState.NotifyForestCellProduced(player);
        }

        LightMechanicUiBootstrap.RefreshForPlayer(player);
        NotifyLightEnergyConsumed(player, consumed);
        return true;
    }

    /// <summary>
    /// 通知能力：玩家消耗了光能（用于觉醒形态、凌空等效果）。
    /// </summary>
    public static void NotifyLightEnergyConsumed(Player player, IReadOnlyList<LightElement> consumed)
    {
        if (consumed.Count == 0)
            return;

        var state = GetActiveState(player);
        var differedCount = 0;

        foreach (var element in consumed)
        {
            if (state != null)
            {
                if (state.HasLastConsumedLightElement && state.LastConsumedLightElement != element)
                    differedCount++;

                state.HasLastConsumedLightElement = true;
                state.LastConsumedLightElement = element;
            }
        }

        player.Creature.GetPower<AlchemyStarsAwakeningFormPower>()
            ?.NotifyLightEnergyConsumed(consumed.Count);

        player.Creature.GetPower<AlchemyStarsAuroraMomentPower>()
            ?.NotifyLightEnergyConsumed(consumed.Count);

        if (differedCount > 0)
        {
            player.Creature.GetPower<AlchemyStarsSoaringPower>()
                ?.NotifyAttributeDiffered(differedCount);
        }
    }

    /// <summary>
    /// 属性格因溢出或其他原因被移出转色栏时通知。
    /// </summary>
    public static void NotifyAttributeCellsRemoved(Player player, IReadOnlyList<AttributeCell> removed)
    {
        if (removed.Count == 0)
            return;

        var fireRemoved = removed.Count(cell =>
            cell.Element is LightElement.Fire or LightElement.Prismatic);
        if (fireRemoved <= 0)
            return;

        player.Creature.GetPower<AlchemyStarsBloomingBegoniaPower>()
            ?.NotifyFireCellsRemoved(fireRemoved);
    }

    /// <summary>
    /// 将转色栏重置为满栏，并保证出现四种不同基础属性。
    /// </summary>
    public static int ResetAttributeBarWithFourDistinct(
        Player player,
        bool allowSpecialCells = false)
    {
        var state = GetActiveState(player);
        if (state == null)
            return 0;

        var reset = state.ResetWithFourDistinctAttributes(
            player.RunState.Rng.Niche,
            allowSpecialCells);
        if (reset > 0)
            LightMechanicUiBootstrap.RefreshForPlayer(player);

        return reset;
    }

    /// <summary>
    /// 转色栏当前属性格数量（不含有效倍率加权）。
    /// </summary>
    public static int CountAttributeCells(Player player)
    {
        var state = GetActiveState(player);
        return state?.AttributeCells.Items.Count ?? 0;
    }

    /// <summary>
    /// 造成属性伤害并触发对应属性的特别机制�?
    /// </summary>
    public static async Task DealElementalAttackDamage(
        PlayerChoiceContext choiceContext,
        Player player,
        CardModel? card,
        Creature target,
        decimal baseDamage,
        LightElement element,
        CardPlay? cardPlay = null)
    {
        using (LightMechanicDamageContext.Use(element))
        {
            if (card != null)
            {
                await DamageCmd.Attack(baseDamage)
                    .FromCard(card, cardPlay)
                    .Targeting(target)
                    .Execute(choiceContext);
            }
            else
            {
                await CreatureCmd.Damage(
                    choiceContext,
                    target,
                    baseDamage,
                    ValueProp.Unblockable | ValueProp.Unpowered,
                    null,
                    null);
            }
        }

        if (element == LightElement.Thunder)
            RecordThunderDamageDealt(player);

        await ApplyElementalHitEffects(choiceContext, player, target, element, card);
    }

    /// <summary>
    /// 贯穿之星：无视格挡、滑溜、缓冲、覆甲与难以杀灭后造成雷属性伤害�?
    /// </summary>
    public static async Task DealPenetratingElementalAttackDamage(
        PlayerChoiceContext choiceContext,
        Player player,
        CardModel? card,
        Creature target,
        decimal baseDamage,
        LightElement element,
        CardPlay? cardPlay = null)
    {
        await ClearPenetratingDefenses(choiceContext, target);

        using (LightMechanicDamageContext.Use(element))
        {
            if (card != null)
            {
                await DamageCmd.Attack(baseDamage)
                    .FromCard(card, cardPlay)
                    .Targeting(target)
                    .Execute(choiceContext);
            }
            else
            {
                await CreatureCmd.Damage(
                    choiceContext,
                    target,
                    baseDamage,
                    ValueProp.Unblockable | ValueProp.Unpowered,
                    null,
                    null);
            }
        }

        if (element == LightElement.Thunder)
            RecordThunderDamageDealt(player);

        await ApplyElementalHitEffects(choiceContext, player, target, element, card);
    }

    public static async Task ApplyElementalHitEffects(
        PlayerChoiceContext choiceContext,
        Player player,
        Creature target,
        LightElement element,
        CardModel? cardSource)
    {
        var state = GetActiveState(player);
        if (state == null)
            return;

        var elementsToProc = element == LightElement.Prismatic
            ? new[] { LightElement.Fire, LightElement.Thunder }
            : element is LightElement.Fire or LightElement.Thunder
                ? new[] { element }
                : Array.Empty<LightElement>();

        foreach (var procElement in elementsToProc)
        {
            var count = state.GetEffectiveCount(procElement);
            var stacks = count / 4;
            if (stacks <= 0)
                continue;

            if (procElement == LightElement.Fire)
            {
                await PowerCmd.Apply<AlchemyStarsScorchPower>(
                    choiceContext,
                    target,
                    stacks,
                    player.Creature,
                    cardSource);
            }
            else if (procElement == LightElement.Thunder)
            {
                await PowerCmd.Apply<AlchemyStarsParalysisPower>(
                    choiceContext,
                    target,
                    stacks,
                    player.Creature,
                    cardSource);
            }
        }
    }

    public static decimal GetOutgoingDamageMultiplier(Player player, LightElement? element)
    {
        var state = GetActiveState(player);
        if (state == null)
            return 1m;

        var multiplier = 1m;

        if (element != null)
        {
            var elements = element == LightElement.Prismatic
                ? LightElementExtensions.BaseElements
                : new[] { element.Value };

            foreach (var current in elements)
            {
                var count = state.GetEffectiveCount(current);
                if (count > 0)
                    multiplier *= 1m + count * 0.04m;
            }
        }

        if (state.RainbowActive)
        {
            var bonus = state.GetTotalEffectiveCellCount() * 0.08m;
            if (state.RainbowDoubled)
                bonus *= 2m;

            multiplier *= 1m + bonus;
        }

        if (element is LightElement.Fire or LightElement.Prismatic)
        {
            var ignition = player.Creature.GetPower<AlchemyStarsIgnitionPower>();
            if (ignition is { Amount: > 0 })
                multiplier *= 1m + AlchemyStarsIgnitionPower.GetBonusRate(player);

            if (player.Creature.GetPowerAmount<AlchemyStarsFireDoubleDamagePower>() > 0)
                multiplier *= 2m;
        }

        return multiplier;
    }

    private static async Task ClearPenetratingDefenses(
        PlayerChoiceContext choiceContext,
        Creature target)
    {
        if (target.Block > 0)
            await CreatureCmd.LoseBlock(choiceContext, target, target.Block, null);

        if (target.HasPower<SlipperyPower>())
            await PowerCmd.Remove<SlipperyPower>(target);

        if (target.HasPower<BufferPower>())
            await PowerCmd.Remove<BufferPower>(target);

        if (target.HasPower<PlatingPower>())
            await PowerCmd.Remove<PlatingPower>(target);

        if (target.HasPower<HardToKillPower>())
            await PowerCmd.Remove<HardToKillPower>(target);
    }

    /// <summary>
    /// 清空转色栏；若清空前仅有�?火属性格则返�?true�?
    /// </summary>
    public static bool TryExhaustAllAttributeCellsOnlyThunderAndFire(Player player)
    {
        var state = GetActiveState(player);
        if (state == null)
            return false;

        var cells = state.AttributeCells.Items;
        if (cells.Count == 0)
            return false;

        var onlyThunderAndFire = cells.All(cell =>
            cell.Element is LightElement.Thunder or LightElement.Fire);

        state.AttributeCells.Clear();
        state.UpdateRainbowState();
        LightMechanicUiBootstrap.RefreshForPlayer(player);
        return onlyThunderAndFire;
    }

    public static async Task ResolvePlayerTurnEnd(
        PlayerChoiceContext choiceContext,
        Player player)
    {
        var state = GetActiveState(player);
        if (state == null)
            return;

        var waterProcs = state.GetEffectiveCount(LightElement.Water) / 4;
        for (var i = 0; i < waterProcs; i++)
            await CreatureCmd.Heal(player.Creature, 1m);

        var forestProcs = state.GetEffectiveCount(LightElement.Forest) / 4;
        if (forestProcs > 0)
        {
            var handSize = player.PlayerCombatState?.Hand?.Cards.Count ?? 0;
            var block = handSize * forestProcs;
            if (block > 0)
                await CreatureCmd.GainBlock(player.Creature, new BlockVar(block, ValueProp.Move), null);
        }

        if (state.RainbowActive)
        {
            var slotLimit = state.AttributeCells.MaxSlots;
            var damage = slotLimit * 5m;
            var enemies = player.Creature.CombatState!.HittableEnemies.ToList();
            foreach (var enemy in enemies)
            {
                await CreatureCmd.Damage(
                    choiceContext,
                    enemy,
                    damage,
                    ValueProp.Unblockable | ValueProp.Unpowered,
                    null,
                    null);
            }

            state.AttributeCells.Clear();
            state.RainbowActive = false;
            state.RainbowDoubled = false;
        }
    }
}
