using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Random;
using STS2RitsuLib.Utils;

namespace AlchemyStars.Mechanics;

/// <summary>
/// 单场战斗内的光能/转色栏状态�?
/// </summary>
public sealed class LightMechanicCombatState
{
    private static readonly AttachedState<Player, LightMechanicCombatState> Store =
        new(_ => new LightMechanicCombatState());

    public SlotQueue<LightElement> LightEnergy { get; private set; } = new(4);
    public SlotQueue<AttributeCell> AttributeCells { get; private set; } = new(4);

    public bool RainbowActive { get; set; }
    public bool RainbowDoubled { get; set; }

    /// <summary>本回合已造成的雷属性伤害次数（用于启明之光等卡牌）。</summary>
    public int ThunderDamageDealtThisTurn { get; set; }

    /// <summary>本场战斗中上一次消耗的光能属性。</summary>
    public LightElement LastConsumedLightElement { get; set; }

    /// <summary>是否已记录过本场战斗的光能消耗属性。</summary>
    public bool HasLastConsumedLightElement { get; set; }

    public static LightMechanicCombatState Get(Player player) => Store[player];

    public static bool TryGet(Player player, out LightMechanicCombatState state)
    {
        if (!Store.ContainsKey(player))
        {
            state = null!;
            return false;
        }

        state = Store[player];
        return true;
    }

    public static void Reset(Player player)
    {
        if (Store.ContainsKey(player))
            Store[player].Clear();
    }

    public void Configure(int slotLimit)
    {
        LightEnergy.SetMaxSlots(slotLimit);
        AttributeCells.SetMaxSlots(slotLimit);
        RainbowActive = false;
        RainbowDoubled = false;
    }

    public void Clear()
    {
        LightEnergy.Clear();
        AttributeCells.Clear();
        RainbowActive = false;
        RainbowDoubled = false;
        ThunderDamageDealtThisTurn = 0;
        HasLastConsumedLightElement = false;
        LastConsumedLightElement = default;
    }

    public void ResetTurnCounters()
    {
        ThunderDamageDealtThisTurn = 0;
    }

    public void GrantStartingLightEnergy()
    {
        foreach (var element in LightElementExtensions.BaseElements)
            LightEnergy.Enqueue(element);
    }

    public List<AttributeCell> AddAttributeCell(AttributeCell cell)
    {
        var overflow = AttributeCells.EnqueueReturningOverflow(cell);
        UpdateRainbowState();
        return overflow;
    }

    public List<AttributeCell> AddAttributeCell(LightElement element, AttributeCellKind kind = AttributeCellKind.Normal) =>
        AddAttributeCell(new AttributeCell(element, kind));

    /// <summary>
    /// 以随机属性填满转色栏至上限。
    /// </summary>
    public void FillAttributeBarWithRandomElements(Rng rng, int darkChancePercent, bool darkOnly)
    {
        var cells = new List<AttributeCell>(AttributeCells.MaxSlots);
        var elements = LightElementExtensions.BaseElements;
        for (var i = 0; i < AttributeCells.MaxSlots; i++)
        {
            var element = elements[rng.NextInt(elements.Length)];
            AttributeCellKind kind;
            if (darkOnly)
                kind = AttributeCellKind.Dark;
            else if (darkChancePercent > 0 && rng.NextInt(100) < darkChancePercent)
                kind = AttributeCellKind.Dark;
            else
                kind = AttributeCellKind.Normal;

            cells.Add(new AttributeCell(element, kind));
        }

        AttributeCells.ReplaceAll(cells);
        UpdateRainbowState();
    }

    /// <summary>
    /// 统计指定属性的有效格数：自身匹配，或邻接同属性棱镜格（邻格视为同属性）。
    /// 深色格权重为 2。
    /// </summary>
    public int GetEffectiveCount(LightElement element)
    {
        var cells = AttributeCells.Items;
        if (cells.Count == 0)
            return 0;

        var total = 0;
        for (var i = 0; i < cells.Count; i++)
        {
            // 万色格不享受深色格双倍权重。
            var weight = cells[i].Kind == AttributeCellKind.Dark &&
                         cells[i].Element != LightElement.Prismatic
                ? 2
                : 1;
            if (ElementMatchesTarget(cells[i].Element, element) ||
                IsAdjacentToMatchingPrism(cells, i, element))
            {
                total += weight;
            }
        }

        return total;
    }

    /// <summary>
    /// 虹光增伤用的加权格数：按实体格子计数（深色×2；万色固定×1，且只计一次）。
    /// 不可对四属性分别 <see cref="GetEffectiveCount"/> 再求和，否则万色会被重复计入 4 次。
    /// </summary>
    public int GetRainbowWeightedCellCount()
    {
        var total = 0;
        foreach (var cell in AttributeCells.Items)
        {
            if (cell.Element == LightElement.Prismatic)
                total += 1;
            else if (cell.Kind == AttributeCellKind.Dark)
                total += 2;
            else
                total += 1;
        }

        return total;
    }

    /// <summary>
    /// 统计万色格数量（每格权重 1，用于虹光补缺口）。
    /// </summary>
    public int CountPrismaticCells()
    {
        return AttributeCells.Items.Count(cell => cell.Element == LightElement.Prismatic);
    }

    /// <summary>
    /// 统计真实属性有效格数（不含万色；万色仅在虹光判定中补缺口）。
    /// </summary>
    public int GetRealEffectiveCount(LightElement element)
    {
        if (!element.IsBaseElement())
            return 0;

        var cells = AttributeCells.Items;
        if (cells.Count == 0)
            return 0;

        var total = 0;
        for (var i = 0; i < cells.Count; i++)
        {
            if (cells[i].Element == LightElement.Prismatic)
                continue;

            var weight = cells[i].Kind == AttributeCellKind.Dark ? 2 : 1;
            if (cells[i].Element == element ||
                IsAdjacentToMatchingPrism(cells, i, element))
            {
                total += weight;
            }
        }

        return total;
    }

    /// <summary>
    /// 虹光四色：真实属性齐全；缺口可由万色格按 1:1 补足。
    /// </summary>
    public bool HasAllBaseElements()
    {
        var missing = LightElementExtensions.BaseElements.Count(e => GetRealEffectiveCount(e) <= 0);
        return CountPrismaticCells() >= missing;
    }

    /// <summary>
    /// 虹光翻倍：每种真实属性有效格数达到 2；不足部分由万色格按 1:1 补足。
    /// </summary>
    public bool HasRainbowDoubleCondition()
    {
        var deficit = 0;
        foreach (var element in LightElementExtensions.BaseElements)
            deficit += Math.Max(0, 2 - GetRealEffectiveCount(element));

        return CountPrismaticCells() >= deficit;
    }

    public void UpdateRainbowState()
    {
        if (!HasAllBaseElements())
        {
            RainbowActive = false;
            RainbowDoubled = false;
            return;
        }

        RainbowActive = true;
        RainbowDoubled = HasRainbowDoubleCondition();
    }

    private static bool ElementMatchesTarget(LightElement cellElement, LightElement target) =>
        cellElement == target ||
        cellElement == LightElement.Prismatic ||
        target == LightElement.Prismatic;

    /// <summary>
    /// 棱镜格：相邻格子视为与棱镜同属性。
    /// </summary>
    private static bool IsAdjacentToMatchingPrism(
        IReadOnlyList<AttributeCell> cells,
        int index,
        LightElement element)
    {
        for (var offset = -1; offset <= 1; offset += 2)
        {
            var neighbor = index + offset;
            if (neighbor < 0 || neighbor >= cells.Count)
                continue;

            if (cells[neighbor].Kind != AttributeCellKind.Prism)
                continue;

            // 万色不与棱镜叠加以生效：万色棱镜格不向邻格传导属性。
            if (cells[neighbor].Element == LightElement.Prismatic)
                continue;

            if (ElementMatchesTarget(cells[neighbor].Element, element))
                return true;
        }

        return false;
    }

    /// <summary>
    /// 随机将非目标属性的格子转化为目标属性格�?
    /// </summary>
    public int ConvertRandomNonElementCells(LightElement targetElement, int maxCount, Rng rng)
    {
        if (maxCount <= 0)
            return 0;

        var cells = AttributeCells.Items.ToList();
        var candidateIndices = cells
            .Select((cell, index) => (cell, index))
            .Where(entry => !IsElementCell(entry.cell, targetElement))
            .Select(entry => entry.index)
            .ToList();

        if (candidateIndices.Count == 0)
            return 0;

        var converted = 0;
        while (converted < maxCount && candidateIndices.Count > 0)
        {
            var pickIndex = rng.NextInt(candidateIndices.Count);
            var cellIndex = candidateIndices[pickIndex];
            candidateIndices.RemoveAt(pickIndex);

            var original = cells[cellIndex];
            cells[cellIndex] = new AttributeCell(targetElement, AttributeCellKind.Normal, original.EnhancedCardTypeName);
            converted++;
        }

        AttributeCells.ReplaceAll(cells);
        UpdateRainbowState();
        return converted;
    }

    /// <summary>
    /// 按概率将非目标属性格重置为目标属性格；未命中时原格不变。
    /// </summary>
    public void ResetNonElementCells(LightElement targetElement, Rng rng, int normalChancePercent, int prismChancePercent)
    {
        var cells = AttributeCells.Items.ToList();
        var rebuilt = new List<AttributeCell>(cells.Count);

        foreach (var cell in cells)
        {
            if (IsElementCell(cell, targetElement))
            {
                rebuilt.Add(cell);
                continue;
            }

            var roll = rng.NextInt(100);
            if (roll < prismChancePercent)
                rebuilt.Add(new AttributeCell(targetElement, AttributeCellKind.Prism));
            else if (roll < prismChancePercent + normalChancePercent)
                rebuilt.Add(new AttributeCell(targetElement));
            else
                rebuilt.Add(cell);
        }

        AttributeCells.ReplaceAll(rebuilt);
        UpdateRainbowState();
    }

    /// <summary>
    /// 重置所有非火属性格：先 roll 是否出火；未出火再等概率出森/雷/水。
    /// </summary>
    public void ResetNonFireCellsWithRandomOther(Rng rng, int fireChancePercent = 60)
    {
        var cells = AttributeCells.Items.ToList();
        var rebuilt = new List<AttributeCell>(cells.Count);
        var otherElements = new[]
        {
            LightElement.Forest,
            LightElement.Thunder,
            LightElement.Water,
        };

        foreach (var cell in cells)
        {
            if (IsElementCell(cell, LightElement.Fire))
            {
                rebuilt.Add(cell);
                continue;
            }

            // 第一次 roll：是否出火
            if (rng.NextInt(100) < fireChancePercent)
            {
                rebuilt.Add(new AttributeCell(LightElement.Fire));
                continue;
            }

            // 第二次 roll：其余三属性等概率
            rebuilt.Add(new AttributeCell(otherElements[rng.NextInt(otherElements.Length)]));
        }

        AttributeCells.ReplaceAll(rebuilt);
        UpdateRainbowState();
    }

    /// <summary>
    /// 将所有属性格转为目标属性，非目标格按概率变为普通格或深色格�?
    /// </summary>
    public int ConvertAllCellsToElement(LightElement targetElement, Rng rng, int darkChancePercent = 15)
    {
        var cells = AttributeCells.Items.ToList();
        if (cells.Count == 0)
            return 0;

        var converted = 0;
        for (var i = 0; i < cells.Count; i++)
        {
            var original = cells[i];
            if (IsElementCell(original, targetElement))
                continue;

            var roll = rng.NextInt(100);
            var kind = roll < darkChancePercent
                ? AttributeCellKind.Dark
                : AttributeCellKind.Normal;
            cells[i] = new AttributeCell(targetElement, kind, original.EnhancedCardTypeName);
            converted++;
        }

        AttributeCells.ReplaceAll(cells);
        UpdateRainbowState();
        return converted;
    }

    /// <summary>
    /// 随机转化属性格：非雷格转为雷格；已是雷格则小概率转为雷属性深色格�?
    /// </summary>
    public (int Converted, int DarkCreated) ConvertRandomCellsToThunderWithDark(int maxCount, Rng rng, int darkChancePercent = 25)
    {
        if (maxCount <= 0)
            return (0, 0);

        var cells = AttributeCells.Items.ToList();
        var candidateIndices = Enumerable.Range(0, cells.Count).ToList();
        var converted = 0;
        var darkCreated = 0;

        while (converted < maxCount && candidateIndices.Count > 0)
        {
            var pickIndex = rng.NextInt(candidateIndices.Count);
            var cellIndex = candidateIndices[pickIndex];
            candidateIndices.RemoveAt(pickIndex);

            var original = cells[cellIndex];
            if (IsElementCell(original, LightElement.Thunder) && original.Kind != AttributeCellKind.Dark)
            {
                if (rng.NextInt(100) < darkChancePercent)
                {
                    cells[cellIndex] = new AttributeCell(
                        LightElement.Thunder,
                        AttributeCellKind.Dark,
                        original.EnhancedCardTypeName);
                    converted++;
                    darkCreated++;
                }

                continue;
            }

            if (!IsElementCell(original, LightElement.Thunder))
            {
                cells[cellIndex] = new AttributeCell(
                    LightElement.Thunder,
                    AttributeCellKind.Normal,
                    original.EnhancedCardTypeName);
                converted++;
            }
        }

        AttributeCells.ReplaceAll(cells);
        UpdateRainbowState();
        return (converted, darkCreated);
    }

    /// <summary>
    /// 随机将指定属性的普通格转为深色格�?
    /// </summary>
    public int ConvertRandomNormalCellToDark(LightElement element, Rng rng, int maxCount)
    {
        if (maxCount <= 0)
            return 0;

        var cells = AttributeCells.Items.ToList();
        var candidateIndices = cells
            .Select((cell, index) => (cell, index))
            .Where(pair => IsElementCell(pair.cell, element) && pair.cell.Kind == AttributeCellKind.Normal)
            .Select(pair => pair.index)
            .ToList();

        var converted = 0;
        while (converted < maxCount && candidateIndices.Count > 0)
        {
            var pickIndex = rng.NextInt(candidateIndices.Count);
            var cellIndex = candidateIndices[pickIndex];
            candidateIndices.RemoveAt(pickIndex);

            var original = cells[cellIndex];
            cells[cellIndex] = new AttributeCell(
                element,
                AttributeCellKind.Dark,
                original.EnhancedCardTypeName);
            converted++;
        }

        if (converted > 0)
        {
            AttributeCells.ReplaceAll(cells);
            UpdateRainbowState();
        }

        return converted;
    }

    /// <summary>
    /// 如意神雷：将所有属性格转为雷属性，已是雷属性的格子加强为深色格�?
    /// </summary>
    public int ConvertAllCellsToThunderWithDarkUpgrade()
    {
        var cells = AttributeCells.Items.ToList();
        if (cells.Count == 0)
            return 0;

        var converted = 0;
        for (var i = 0; i < cells.Count; i++)
        {
            var original = cells[i];
            if (IsElementCell(original, LightElement.Thunder))
            {
                if (original.Kind == AttributeCellKind.Dark)
                    continue;

                cells[i] = new AttributeCell(
                    LightElement.Thunder,
                    AttributeCellKind.Dark,
                    original.EnhancedCardTypeName);
                converted++;
                continue;
            }

            cells[i] = new AttributeCell(
                LightElement.Thunder,
                AttributeCellKind.Normal,
                original.EnhancedCardTypeName);
            converted++;
        }

        AttributeCells.ReplaceAll(cells);
        UpdateRainbowState();
        return converted;
    }

    private static bool IsElementCell(AttributeCell cell, LightElement element) =>
        cell.Element == element || cell.Element == LightElement.Prismatic;

    /// <summary>
    /// 重置非目标属性格，按概率生成普通格或强化格；未命中时原格不变。
    /// </summary>
    public int ResetNonElementCellsWithEnhanced(
        LightElement targetElement,
        Rng rng,
        int normalChancePercent,
        int enhancedChancePercent,
        string? enhancedCardTypeName = null)
    {
        var cells = AttributeCells.Items.ToList();
        var rebuilt = new List<AttributeCell>(cells.Count);
        var reset = 0;

        foreach (var cell in cells)
        {
            if (IsElementCell(cell, targetElement))
            {
                rebuilt.Add(cell);
                continue;
            }

            var roll = rng.NextInt(100);
            if (roll < enhancedChancePercent)
            {
                rebuilt.Add(new AttributeCell(
                    targetElement,
                    AttributeCellKind.Enhanced,
                    enhancedCardTypeName));
                reset++;
            }
            else if (roll < enhancedChancePercent + normalChancePercent)
            {
                rebuilt.Add(new AttributeCell(targetElement));
                reset++;
            }
            else
            {
                rebuilt.Add(cell);
            }
        }

        AttributeCells.ReplaceAll(rebuilt);
        UpdateRainbowState();
        return reset;
    }

    /// <summary>
    /// 重置所有格子属性，按概率生成目标属性格或强化格�?
    /// </summary>
    public int ResetAllCellsWithEnhanced(
        LightElement targetElement,
        Rng rng,
        int normalChancePercent,
        int enhancedChancePercent,
        string? enhancedCardTypeName = null)
    {
        var cells = AttributeCells.Items.ToList();
        if (cells.Count == 0)
            return 0;

        var rebuilt = new List<AttributeCell>(cells.Count);
        foreach (var _ in cells)
        {
            var roll = rng.NextInt(100);
            if (roll < enhancedChancePercent)
            {
                rebuilt.Add(new AttributeCell(
                    targetElement,
                    AttributeCellKind.Enhanced,
                    enhancedCardTypeName));
            }
            else if (roll < enhancedChancePercent + normalChancePercent)
                rebuilt.Add(new AttributeCell(targetElement));
            else
                rebuilt.Add(new AttributeCell(targetElement));
        }

        AttributeCells.ReplaceAll(rebuilt);
        UpdateRainbowState();
        return cells.Count;
    }

    /// <summary>
    /// 将所有属性格转为目标属性，非目标格按概率变为普通格或强化格�?
    /// </summary>
    public int ConvertAllCellsToElementWithEnhanced(
        LightElement targetElement,
        Rng rng,
        int normalChancePercent,
        int enhancedChancePercent,
        string? enhancedCardTypeName = null)
    {
        var cells = AttributeCells.Items.ToList();
        if (cells.Count == 0)
            return 0;

        var converted = 0;
        for (var i = 0; i < cells.Count; i++)
        {
            var original = cells[i];
            if (IsElementCell(original, targetElement) && original.Kind == AttributeCellKind.Enhanced)
                continue;

            var roll = rng.NextInt(100);
            AttributeCellKind kind;
            if (roll < enhancedChancePercent)
                kind = AttributeCellKind.Enhanced;
            else
                kind = AttributeCellKind.Normal;

            cells[i] = new AttributeCell(targetElement, kind, enhancedCardTypeName ?? original.EnhancedCardTypeName);
            converted++;
        }

        AttributeCells.ReplaceAll(cells);
        UpdateRainbowState();
        return converted;
    }

    public int CountEnhancedCells(LightElement element, string? cardTypeName = null)
    {
        return AttributeCells.Items.Count(cell =>
            IsElementCell(cell, element) &&
            cell.Kind == AttributeCellKind.Enhanced &&
            (cardTypeName == null || cell.EnhancedCardTypeName == cardTypeName));
    }

    public int CountForestCellsWeightedByEnhanced()
    {
        var total = 0;
        foreach (var cell in AttributeCells.Items)
        {
            if (!IsElementCell(cell, LightElement.Forest))
                continue;

            // 万色格不享受深色格双倍权重。
            if (cell.Kind == AttributeCellKind.Dark && cell.Element != LightElement.Prismatic)
                total += 2;
            else if (cell.Kind == AttributeCellKind.Enhanced)
                total += 2;
            else
                total += 1;
        }

        return total;
    }

    public bool TryEnhanceRandomCell(LightElement element, Rng rng, string? enhancedCardTypeName = null)
    {
        var cells = AttributeCells.Items.ToList();
        var candidateIndices = cells
            .Select((cell, index) => (cell, index))
            .Where(entry => IsElementCell(entry.cell, element) && entry.cell.Kind != AttributeCellKind.Enhanced)
            .Select(entry => entry.index)
            .ToList();

        if (candidateIndices.Count == 0)
            return false;

        var pickIndex = rng.NextInt(candidateIndices.Count);
        var cellIndex = candidateIndices[pickIndex];
        var original = cells[cellIndex];
        cells[cellIndex] = new AttributeCell(
            element,
            AttributeCellKind.Enhanced,
            enhancedCardTypeName ?? original.EnhancedCardTypeName);
        AttributeCells.ReplaceAll(cells);
        UpdateRainbowState();
        return true;
    }

    public int ConsumeAllEnhancedCells(LightElement element, string? cardTypeName = null)
    {
        var cells = AttributeCells.Items.ToList();
        var consumed = 0;

        for (var i = 0; i < cells.Count; i++)
        {
            var cell = cells[i];
            if (!IsElementCell(cell, element) || cell.Kind != AttributeCellKind.Enhanced)
                continue;

            if (cardTypeName != null && cell.EnhancedCardTypeName != cardTypeName)
                continue;

            cells[i] = new AttributeCell(element, AttributeCellKind.Normal, cell.EnhancedCardTypeName);
            consumed++;
        }

        if (consumed > 0)
        {
            AttributeCells.ReplaceAll(cells);
            UpdateRainbowState();
        }

        return consumed;
    }

    public int ConvertAllCellsToEnhanced(LightElement element)
    {
        var cells = AttributeCells.Items.ToList();
        if (cells.Count == 0)
            return 0;

        for (var i = 0; i < cells.Count; i++)
        {
            var original = cells[i];
            cells[i] = new AttributeCell(
                element,
                AttributeCellKind.Enhanced,
                original.EnhancedCardTypeName);
        }

        AttributeCells.ReplaceAll(cells);
        UpdateRainbowState();
        return cells.Count;
    }

    /// <summary>
    /// 将转色栏填满为 MaxSlots，并保证四种基础属性各至少出现一次。
    /// </summary>
    public int ResetWithFourDistinctAttributes(Rng rng, bool allowSpecialCells)
    {
        var slotCount = AttributeCells.MaxSlots;
        if (slotCount <= 0)
            return 0;

        var baseElements = LightElementExtensions.BaseElements.ToList();
        // 打乱四种基础属性顺序，前四个槽位各用一种。
        for (var i = baseElements.Count - 1; i > 0; i--)
        {
            var j = rng.NextInt(i + 1);
            (baseElements[i], baseElements[j]) = (baseElements[j], baseElements[i]);
        }

        var rebuilt = new List<AttributeCell>(slotCount);
        for (var i = 0; i < slotCount; i++)
        {
            var element = i < baseElements.Count
                ? baseElements[i]
                : baseElements[rng.NextInt(baseElements.Count)];
            rebuilt.Add(new AttributeCell(element, ResolveAwakeningCellKind(rng, allowSpecialCells)));
        }

        AttributeCells.ReplaceAll(rebuilt);
        UpdateRainbowState();
        return rebuilt.Count;
    }

    private static AttributeCellKind ResolveAwakeningCellKind(Rng rng, bool allowSpecialCells)
    {
        if (!allowSpecialCells || rng.NextInt(100) >= 75)
            return AttributeCellKind.Normal;

        return rng.NextInt(100) < 50
            ? AttributeCellKind.Dark
            : AttributeCellKind.Enhanced;
    }
}
