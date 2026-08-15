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
    /// 随机转化非雷属性格为雷属性格；成功时刷新 UI。
    /// </summary>
    public static int TryConvertRandomNonThunderCells(Player player, int maxCount) =>
        TryConvertRandomNonElementCells(player, LightElement.Thunder, maxCount);

    /// <summary>
    /// 随机消耗最多 maxCount 点非雷光能（含万色），每点生成 1 雷属性格；返回成功生成的格数。
    /// </summary>
    public static int TryConvertRandomNonThunderLightEnergyToThunderCells(Player player, int maxCount)
    {
        var state = GetActiveState(player);
        if (state == null || maxCount <= 0)
            return 0;

        var energy = state.LightEnergy.Items.ToList();
        var candidateIndices = new List<int>();
        for (var i = 0; i < energy.Count; i++)
        {
            if (energy[i] != LightElement.Thunder)
                candidateIndices.Add(i);
        }

        if (candidateIndices.Count == 0)
            return 0;

        var rng = player.RunState.Rng.Niche;
        var take = Math.Min(maxCount, candidateIndices.Count);
        var pickedIndices = new List<int>(take);
        for (var n = 0; n < take; n++)
        {
            var pick = rng.NextInt(candidateIndices.Count);
            pickedIndices.Add(candidateIndices[pick]);
            candidateIndices.RemoveAt(pick);
        }

        pickedIndices.Sort((a, b) => b.CompareTo(a));
        var consumed = new List<LightElement>(pickedIndices.Count);
        foreach (var index in pickedIndices)
        {
            consumed.Add(energy[index]);
            energy.RemoveAt(index);
        }

        state.LightEnergy.ReplaceAll(energy);

        var created = 0;
        foreach (var _ in consumed)
        {
            if (TryAddAttributeCell(player, LightElement.Thunder))
                created++;
        }

        NotifyLightEnergyConsumed(player, consumed);
        return created;
    }

    /// <summary>
    /// 生成若干雷属性格，按概率出现深色格；返回成功生成数量。
    /// </summary>
    public static int TryAddThunderCellsWithDarkChance(
        Player player,
        int count,
        int darkChancePercent = 15)
    {
        if (count <= 0)
            return 0;

        var rng = player.RunState.Rng.Niche;
        var created = 0;
        for (var i = 0; i < count; i++)
        {
            var kind = rng.NextInt(100) < darkChancePercent
                ? AttributeCellKind.Dark
                : AttributeCellKind.Normal;
            if (TryAddAttributeCell(player, LightElement.Thunder, kind))
                created++;
        }

        return created;
    }

    /// <summary>
    /// 生成若干雷属性格，按概率出现棱镜格；返回成功生成数量。
    /// </summary>
    public static int TryAddThunderCellsWithPrismChance(
        Player player,
        int count,
        int prismChancePercent = 50)
    {
        if (count <= 0)
            return 0;

        var rng = player.RunState.Rng.Niche;
        var created = 0;
        for (var i = 0; i < count; i++)
        {
            var kind = rng.NextInt(100) < prismChancePercent
                ? AttributeCellKind.Prism
                : AttributeCellKind.Normal;
            if (TryAddAttributeCell(player, LightElement.Thunder, kind))
                created++;
        }

        return created;
    }

    /// <summary>
    /// 生成若干雷属性深色格；返回成功生成数量。
    /// </summary>
    public static int TryAddDarkThunderCells(Player player, int count)
    {
        if (count <= 0)
            return 0;

        var created = 0;
        for (var i = 0; i < count; i++)
        {
            if (TryAddAttributeCell(player, LightElement.Thunder, AttributeCellKind.Dark))
                created++;
        }

        return created;
    }

    /// <summary>
    /// 统计转色栏中的雷属性格数量（含万色格）。
    /// </summary>
    public static int CountThunderAttributeCells(Player player)
    {
        var state = GetActiveState(player);
        if (state == null)
            return 0;

        return state.AttributeCells.Items.Count(cell =>
            cell.Element is LightElement.Thunder or LightElement.Prismatic);
    }

    /// <summary>
    /// 转色栏是否已满且全部为雷属性深色格。
    /// </summary>
    public static bool IsAllThunderDarkCells(Player player)
    {
        var state = GetActiveState(player);
        if (state == null)
            return false;

        var cells = state.AttributeCells.Items;
        var maxSlots = state.AttributeCells.MaxSlots;
        if (cells.Count < maxSlots || maxSlots <= 0)
            return false;

        return cells.All(cell =>
            cell.Element == LightElement.Thunder && cell.Kind == AttributeCellKind.Dark);
    }

    /// <summary>
    /// 添加雷属性格；栏满时将非雷深色格转为雷深色格。
    /// 若已全部为雷深色格则返回 AllDarkThunder=true（不添加）。
    /// </summary>
    public static (bool AllDarkThunder, int Changed) TryAddThunderCellsOrDarkWhenFull(
        Player player,
        int count)
    {
        var state = GetActiveState(player);
        if (state == null || count <= 0)
            return (false, 0);

        if (IsAllThunderDarkCells(player))
            return (true, 0);

        var changed = 0;
        var rng = player.RunState.Rng.Niche;
        for (var i = 0; i < count; i++)
        {
            if (state.AttributeCells.Items.Count < state.AttributeCells.MaxSlots)
            {
                if (TryAddAttributeCell(player, LightElement.Thunder))
                    changed++;
                continue;
            }

            var cells = state.AttributeCells.Items.ToList();
            var candidateIndices = cells
                .Select((cell, index) => (cell, index))
                .Where(pair => !(pair.cell.Element == LightElement.Thunder &&
                                 pair.cell.Kind == AttributeCellKind.Dark))
                .Select(pair => pair.index)
                .ToList();

            if (candidateIndices.Count == 0)
                break;

            var cellIndex = candidateIndices[rng.NextInt(candidateIndices.Count)];
            var original = cells[cellIndex];
            cells[cellIndex] = new AttributeCell(
                LightElement.Thunder,
                AttributeCellKind.Dark,
                original.EnhancedCardTypeName);
            state.AttributeCells.ReplaceAll(cells);
            state.UpdateRainbowState();
            LightMechanicUiBootstrap.RefreshForPlayer(player);
            changed++;
        }

        return (false, changed);
    }

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
        {
            LightMechanicUiBootstrap.RefreshForPlayer(player);
            if (targetElement == LightElement.Forest)
                AlchemyStarsForestState.NotifyForestCellProduced(player, converted);
        }

        return converted;
    }

    /// <summary>
    /// 随机转化非森属性格为森属性格。
    /// </summary>
    public static int TryConvertRandomNonForestCells(Player player, int maxCount) =>
        TryConvertRandomNonElementCells(player, LightElement.Forest, maxCount);

    /// <summary>
    /// 随机消耗最多 maxCount 点非森光能（含万色），每点生成 1 森属性格；返回成功生成的格数。
    /// </summary>
    public static int TryConvertRandomNonForestLightEnergyToForestCells(Player player, int maxCount)
    {
        var state = GetActiveState(player);
        if (state == null || maxCount <= 0)
            return 0;

        var energy = state.LightEnergy.Items.ToList();
        var candidateIndices = new List<int>();
        for (var i = 0; i < energy.Count; i++)
        {
            if (energy[i] != LightElement.Forest)
                candidateIndices.Add(i);
        }

        if (candidateIndices.Count == 0)
            return 0;

        var rng = player.RunState.Rng.Niche;
        var take = Math.Min(maxCount, candidateIndices.Count);
        var pickedIndices = new List<int>(take);
        for (var n = 0; n < take; n++)
        {
            var pick = rng.NextInt(candidateIndices.Count);
            pickedIndices.Add(candidateIndices[pick]);
            candidateIndices.RemoveAt(pick);
        }

        pickedIndices.Sort((a, b) => b.CompareTo(a));
        var consumed = new List<LightElement>(pickedIndices.Count);
        foreach (var index in pickedIndices)
        {
            consumed.Add(energy[index]);
            energy.RemoveAt(index);
        }

        state.LightEnergy.ReplaceAll(energy);

        var created = 0;
        foreach (var _ in consumed)
        {
            if (TryAddAttributeCell(player, LightElement.Forest))
                created++;
        }

        NotifyLightEnergyConsumed(player, consumed);
        return created;
    }

    /// <summary>
    /// 生成若干森属性格，按概率出现强化格；返回成功生成数量。
    /// </summary>
    public static int TryAddForestCellsWithEnhancedChance(
        Player player,
        int count,
        int enhancedChancePercent = 30)
    {
        if (count <= 0)
            return 0;

        var rng = player.RunState.Rng.Niche;
        var created = 0;
        for (var i = 0; i < count; i++)
        {
            var kind = rng.NextInt(100) < enhancedChancePercent
                ? AttributeCellKind.Enhanced
                : AttributeCellKind.Normal;
            if (TryAddAttributeCell(player, LightElement.Forest, kind))
                created++;
        }

        return created;
    }

    /// <summary>
    /// 未满则用随机属性格填满转色栏；已满则整栏重置。大概率森格、中概率强化格。
    /// 返回成功制造的森属性格数量。
    /// </summary>
    public static int FillOrResetAttributeBarBiasedForest(
        Player player,
        int forestChancePercent = 60,
        int enhancedChancePercent = 30)
    {
        var state = GetActiveState(player);
        if (state == null)
            return 0;

        var rng = player.RunState.Rng.Niche;
        var maxSlots = state.AttributeCells.MaxSlots;
        if (maxSlots <= 0)
            return 0;

        var current = state.AttributeCells.Items.ToList();
        var forestCreated = 0;

        AttributeCell RollBiasedCell()
        {
            var roll = rng.NextInt(100);
            if (roll < enhancedChancePercent)
                return new AttributeCell(LightElement.Forest, AttributeCellKind.Enhanced);

            if (roll < enhancedChancePercent + forestChancePercent)
                return new AttributeCell(LightElement.Forest);

            var elements = LightElementExtensions.BaseElements;
            return new AttributeCell(elements[rng.NextInt(elements.Length)]);
        }

        if (current.Count >= maxSlots)
        {
            var rebuilt = new List<AttributeCell>(maxSlots);
            for (var i = 0; i < maxSlots; i++)
            {
                var cell = RollBiasedCell();
                rebuilt.Add(cell);
                if (cell.Element == LightElement.Forest)
                    forestCreated++;
            }

            var removed = current;
            state.AttributeCells.ReplaceAll(rebuilt);
            NotifyAttributeCellsRemoved(player, removed);
        }
        else
        {
            var toAdd = maxSlots - current.Count;
            for (var i = 0; i < toAdd; i++)
            {
                var cell = RollBiasedCell();
                if (cell.Kind == AttributeCellKind.Enhanced &&
                    AlchemyStarsForestState.TryAbsorbEnhancedCellsForWordAbsolute(player, 1) > 0)
                {
                    if (cell.Element == LightElement.Forest)
                        forestCreated++;
                    continue;
                }

                var overflow = state.AddAttributeCell(cell);
                if (overflow.Count > 0)
                    NotifyAttributeCellsRemoved(player, overflow);

                if (cell.Element == LightElement.Forest)
                    forestCreated++;
            }
        }

        LightMechanicUiBootstrap.RefreshForPlayer(player);
        if (forestCreated > 0)
            AlchemyStarsForestState.NotifyForestCellProduced(player, forestCreated);

        return forestCreated;
    }

    /// <summary>
    /// 随机转化非水属性格为水属性格。
    /// </summary>
    public static int TryConvertRandomNonWaterCells(Player player, int maxCount) =>
        TryConvertRandomNonElementCells(player, LightElement.Water, maxCount);

    /// <summary>
    /// 随机消耗最多 maxCount 点非水光能（含万色），每点生成 1 水属性格；返回成功生成的格数。
    /// </summary>
    public static int TryConvertRandomNonWaterLightEnergyToWaterCells(Player player, int maxCount)
    {
        var state = GetActiveState(player);
        if (state == null || maxCount <= 0)
            return 0;

        var energy = state.LightEnergy.Items.ToList();
        var candidateIndices = new List<int>();
        for (var i = 0; i < energy.Count; i++)
        {
            if (energy[i] != LightElement.Water)
                candidateIndices.Add(i);
        }

        if (candidateIndices.Count == 0)
            return 0;

        var rng = player.RunState.Rng.Niche;
        var take = Math.Min(maxCount, candidateIndices.Count);
        var pickedIndices = new List<int>(take);
        for (var n = 0; n < take; n++)
        {
            var pick = rng.NextInt(candidateIndices.Count);
            pickedIndices.Add(candidateIndices[pick]);
            candidateIndices.RemoveAt(pick);
        }

        pickedIndices.Sort((a, b) => b.CompareTo(a));
        var consumed = new List<LightElement>(pickedIndices.Count);
        foreach (var index in pickedIndices)
        {
            consumed.Add(energy[index]);
            energy.RemoveAt(index);
        }

        state.LightEnergy.ReplaceAll(energy);

        var created = 0;
        foreach (var _ in consumed)
        {
            if (TryAddAttributeCell(player, LightElement.Water))
                created++;
        }

        NotifyLightEnergyConsumed(player, consumed);
        return created;
    }

    /// <summary>
    /// 生成若干水属性格，按概率出现深色格；返回成功生成数量。
    /// </summary>
    public static int TryAddWaterCellsWithDarkChance(
        Player player,
        int count,
        int darkChancePercent = 30)
    {
        if (count <= 0)
            return 0;

        var rng = player.RunState.Rng.Niche;
        var created = 0;
        for (var i = 0; i < count; i++)
        {
            var kind = rng.NextInt(100) < darkChancePercent
                ? AttributeCellKind.Dark
                : AttributeCellKind.Normal;
            if (TryAddAttributeCell(player, LightElement.Water, kind))
                created++;
        }

        return created;
    }

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

        var forestBefore = CountForestCells(state);
        var reset = state.ResetNonElementCellsWithEnhanced(
            LightElement.Forest,
            player.RunState.Rng.Niche,
            normalChancePercent,
            enhancedChancePercent,
            enhancedCardTypeName);
        LightMechanicUiBootstrap.RefreshForPlayer(player);

        var produced = CountForestCells(state) - forestBefore;
        if (produced > 0)
            AlchemyStarsForestState.NotifyForestCellProduced(player, produced);

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

        var forestBefore = targetElement == LightElement.Forest ? CountForestCells(state) : 0;
        var reset = state.ResetAllCellsWithEnhanced(
            targetElement,
            player.RunState.Rng.Niche,
            normalChancePercent,
            enhancedChancePercent,
            enhancedCardTypeName);
        LightMechanicUiBootstrap.RefreshForPlayer(player);

        if (targetElement == LightElement.Forest)
        {
            var produced = CountForestCells(state) - forestBefore;
            if (produced > 0)
                AlchemyStarsForestState.NotifyForestCellProduced(player, produced);
        }

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

        var forestBefore = CountForestCells(state);
        var converted = state.ConvertAllCellsToElementWithEnhanced(
            LightElement.Forest,
            player.RunState.Rng.Niche,
            normalChancePercent,
            enhancedChancePercent,
            enhancedCardTypeName);
        LightMechanicUiBootstrap.RefreshForPlayer(player);

        var produced = CountForestCells(state) - forestBefore;
        if (produced > 0)
            AlchemyStarsForestState.NotifyForestCellProduced(player, produced);

        return converted;
    }

    private static int CountForestCells(LightMechanicCombatState state) =>
        state.AttributeCells.Items.Count(cell => cell.Element == LightElement.Forest);

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
        string? enhancedCardTypeName = null,
        bool allowWordAbsoluteAbsorb = true)
    {
        var state = GetActiveState(player);
        if (state == null)
            return false;

        if (allowWordAbsoluteAbsorb &&
            AlchemyStarsForestState.TryAbsorbEnhancedCellsForWordAbsolute(player, 1) > 0)
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
    /// 转色栏中火或水（含万色）的有效格数；深色格权重为 2。
    /// </summary>
    public static int CountFireAndWaterAttributeCells(Player player)
    {
        var state = GetActiveState(player);
        if (state == null)
            return 0;

        var total = 0;
        foreach (var cell in state.AttributeCells.Items)
        {
            if (cell.Element is not (LightElement.Fire or LightElement.Water or LightElement.Prismatic))
                continue;

            // 万色格不享受深色格双倍权重。
            total += cell.Kind == AttributeCellKind.Dark && cell.Element != LightElement.Prismatic
                ? 2
                : 1;
        }

        return total;
    }

    /// <summary>
    /// 光能机制当前是否对玩家生效（有遗物且存活）。
    /// </summary>
    public static bool IsMechanicActive(Player player) =>
        HasMechanicRelic(player) && player.Creature is { IsDead: false };

    /// <summary>
    /// 死亡后静默清空光能与转色栏；不触发属性格移除相关效果。
    /// </summary>
    public static void ClearOnDeath(Player player)
    {
        if (!HasMechanicRelic(player))
            return;

        if (!LightMechanicCombatState.TryGet(player, out var state))
            return;

        state.Clear();
        LightMechanicUiBootstrap.RefreshForPlayer(player);
    }

    /// <summary>
    /// 玩家拥有光能遗物且存活时，确保战斗状态已创建并完成栏位配置。
    /// 死亡后返回 null，使属性格相关效果不再触发。
    /// </summary>
    internal static LightMechanicCombatState? GetActiveState(Player player)
    {
        if (!IsMechanicActive(player))
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

    /// <summary>
    /// 统计火/水光能数量（含万色，每点只计 1）。
    /// </summary>
    public static int CountFireAndWaterLightEnergy(Player player)
    {
        var state = GetActiveState(player);
        if (state == null)
            return 0;

        return state.LightEnergy.Items.Count(item =>
            item is LightElement.Fire or LightElement.Water or LightElement.Prismatic);
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
    /// 随机消耗最多 maxCount 点非火光能（含万色），每点生成 1 火属性格；返回成功生成的格数。
    /// </summary>
    public static int TryConvertRandomNonFireLightEnergyToFireCells(Player player, int maxCount)
    {
        var state = GetActiveState(player);
        if (state == null || maxCount <= 0)
            return 0;

        var energy = state.LightEnergy.Items.ToList();
        var candidateIndices = new List<int>();
        for (var i = 0; i < energy.Count; i++)
        {
            if (energy[i] != LightElement.Fire)
                candidateIndices.Add(i);
        }

        if (candidateIndices.Count == 0)
            return 0;

        var rng = player.RunState.Rng.Niche;
        var take = Math.Min(maxCount, candidateIndices.Count);
        var pickedIndices = new List<int>(take);
        for (var n = 0; n < take; n++)
        {
            var pick = rng.NextInt(candidateIndices.Count);
            pickedIndices.Add(candidateIndices[pick]);
            candidateIndices.RemoveAt(pick);
        }

        pickedIndices.Sort((a, b) => b.CompareTo(a));
        var consumed = new List<LightElement>(pickedIndices.Count);
        foreach (var index in pickedIndices)
        {
            consumed.Add(energy[index]);
            energy.RemoveAt(index);
        }

        state.LightEnergy.ReplaceAll(energy);

        var created = 0;
        foreach (var _ in consumed)
        {
            if (TryAddAttributeCell(player, LightElement.Fire))
                created++;
        }

        NotifyLightEnergyConsumed(player, consumed);
        return created;
    }

    /// <summary>
    /// 生成若干火属性格，按概率出现深色格；返回成功生成数量。
    /// </summary>
    public static int TryAddFireCellsWithDarkChance(
        Player player,
        int count,
        int darkChancePercent = 15)
    {
        if (count <= 0)
            return 0;

        var rng = player.RunState.Rng.Niche;
        var created = 0;
        for (var i = 0; i < count; i++)
        {
            var kind = rng.NextInt(100) < darkChancePercent
                ? AttributeCellKind.Dark
                : AttributeCellKind.Normal;
            if (TryAddAttributeCell(player, LightElement.Fire, kind))
                created++;
        }

        return created;
    }

    /// <summary>
    /// 重置所有非火属性格：60% 出火；未出火时再等概率出森/雷/水。
    /// </summary>
    public static void ResetNonFireCells(Player player, int fireChancePercent = 60)
    {
        var state = GetActiveState(player);
        if (state == null)
            return;

        state.ResetNonFireCellsWithRandomOther(
            player.RunState.Rng.Niche,
            fireChancePercent);
        LightMechanicUiBootstrap.RefreshForPlayer(player);
    }

    /// <summary>
    /// 重置全部光能与全部转色栏属性格，大概率出现火属性。
    /// </summary>
    public static void ResetAllLightEnergyAndAttributeCellsBiasedFire(
        Player player,
        int fireChancePercent = 60)
    {
        var state = GetActiveState(player);
        if (state == null)
            return;

        var rng = player.RunState.Rng.Niche;
        var otherElements = new[]
        {
            LightElement.Forest,
            LightElement.Thunder,
            LightElement.Water,
        };

        LightElement RollFireBiased()
        {
            if (rng.NextInt(100) < fireChancePercent)
                return LightElement.Fire;

            return otherElements[rng.NextInt(otherElements.Length)];
        }

        var energyCount = state.LightEnergy.Count;
        if (energyCount > 0)
        {
            var rebuiltEnergy = new List<LightElement>(energyCount);
            for (var i = 0; i < energyCount; i++)
                rebuiltEnergy.Add(RollFireBiased());

            state.LightEnergy.ReplaceAll(rebuiltEnergy);
        }

        var cells = state.AttributeCells.Items.ToList();
        if (cells.Count > 0)
        {
            var removed = cells;
            var rebuiltCells = new List<AttributeCell>(cells.Count);
            for (var i = 0; i < cells.Count; i++)
                rebuiltCells.Add(new AttributeCell(RollFireBiased()));

            state.AttributeCells.ReplaceAll(rebuiltCells);
            NotifyAttributeCellsRemoved(player, removed);
        }

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
    /// 造成同时视为火与雷的攻击伤害。
    /// </summary>
    public static async Task DealFireAndThunderAttackDamage(
        PlayerChoiceContext choiceContext,
        Player player,
        CardModel? card,
        Creature target,
        decimal baseDamage,
        CardPlay? cardPlay = null)
    {
        using (LightMechanicDamageContext.UseFireAndThunder())
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

        RecordThunderDamageDealt(player);
        await ApplyElementalHitEffects(choiceContext, player, target, LightElement.Prismatic, card);
    }

    /// <summary>
    /// 贯穿之星：无视格挡、滑溜、缓冲、覆甲与难以杀灭后造成雷属性伤害。
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

        // 按当前格子重算虹光，避免缓存的 RainbowActive 在格子变化后仍错误增伤。
        state.UpdateRainbowState();

        var multiplier = 1m;
        var isFireAndThunder = LightMechanicDamageContext.IsFireAndThunder;

        if (isFireAndThunder || element != null)
        {
            var elements = isFireAndThunder
                ? new[] { LightElement.Fire, LightElement.Thunder }
                : element == LightElement.Prismatic
                    ? LightElementExtensions.BaseElements
                    : new[] { element!.Value };

            foreach (var current in elements)
            {
                var count = state.GetEffectiveCount(current);
                if (count > 0)
                    multiplier *= 1m + count * 0.04m;
            }
        }

        if (state.RainbowActive)
        {
            var bonus = state.GetRainbowWeightedCellCount() * 0.08m;
            if (state.RainbowDoubled)
                bonus *= 2m;

            multiplier *= 1m + bonus;
        }

        if (isFireAndThunder || element is LightElement.Fire or LightElement.Prismatic)
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

        // 结算前按当前格子重算，避免缓存状态与「万色补缺口」规则不一致。
        state.UpdateRainbowState();
        if (!state.RainbowActive)
            return;

        // 虹光：先结算伤害，无论成败都必须清空转色栏并刷新 UI。
        var slotLimit = state.AttributeCells.MaxSlots;
        var damage = slotLimit * 5m;
        var enemies = player.Creature.CombatState?.HittableEnemies.ToList()
                      ?? [];

        try
        {
            foreach (var enemy in enemies)
            {
                if (enemy.IsDead)
                    continue;

                await CreatureCmd.Damage(
                    choiceContext,
                    enemy,
                    damage,
                    ValueProp.Unblockable | ValueProp.Unpowered,
                    player.Creature);
            }
        }
        finally
        {
            var removed = state.AttributeCells.Items.ToList();
            state.AttributeCells.Clear();
            state.RainbowActive = false;
            state.RainbowDoubled = false;

            if (removed.Count > 0)
                NotifyAttributeCellsRemoved(player, removed);

            LightMechanicUiBootstrap.RefreshForPlayer(player);
        }
    }
}
