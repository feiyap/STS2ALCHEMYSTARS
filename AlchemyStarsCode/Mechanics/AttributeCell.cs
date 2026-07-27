namespace AlchemyStars.Mechanics;

/// <summary>
/// 转色栏中的单个属性格�?
/// </summary>
public sealed class AttributeCell
{
    public LightElement Element { get; init; }
    public AttributeCellKind Kind { get; init; } = AttributeCellKind.Normal;

    /// <summary>
    /// 强化格子绑定的卡牌类型名；为空表示尚未绑定�?
    /// </summary>
    public string? EnhancedCardTypeName { get; init; }

    public AttributeCell(LightElement element, AttributeCellKind kind = AttributeCellKind.Normal, string? enhancedCardTypeName = null)
    {
        Element = element;
        Kind = kind;
        EnhancedCardTypeName = enhancedCardTypeName;
    }
}
