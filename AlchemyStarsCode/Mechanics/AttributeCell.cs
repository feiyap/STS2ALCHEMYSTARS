namespace AlchemyStars.Mechanics;

/// <summary>
/// 转色栏中的单个属性格。
/// </summary>
public sealed class AttributeCell
{
    public LightElement Element { get; init; }
    public AttributeCellKind Kind { get; init; } = AttributeCellKind.Normal;

    /// <summary>
    /// 强化格子绑定的卡牌类型名；为空表示尚未绑定。
    /// </summary>
    public string? EnhancedCardTypeName { get; init; }

    public AttributeCell(LightElement element, AttributeCellKind kind = AttributeCellKind.Normal, string? enhancedCardTypeName = null)
    {
        Element = element;
        // 万色属性格不与深色格、棱镜格叠加以生效。
        Kind = NormalizeKind(element, kind);
        EnhancedCardTypeName = enhancedCardTypeName;
    }

    /// <summary>
    /// 万色格强制为普通格，禁止出现深色万色格 / 棱镜万色格。
    /// </summary>
    public static AttributeCellKind NormalizeKind(LightElement element, AttributeCellKind kind)
    {
        if (element == LightElement.Prismatic &&
            kind is AttributeCellKind.Dark or AttributeCellKind.Prism)
            return AttributeCellKind.Normal;

        return kind;
    }
}
