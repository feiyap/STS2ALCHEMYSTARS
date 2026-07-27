namespace AlchemyStars.Mechanics;

/// <summary>
/// 光能与属性格的元素类型�?
/// </summary>
public enum LightElement
{
    Forest = 0,
    Thunder = 1,
    Water = 2,
    Fire = 3,
    Prismatic = 4,
}

public static class LightElementExtensions
{
    public static readonly LightElement[] BaseElements =
    [
        LightElement.Forest,
        LightElement.Thunder,
        LightElement.Water,
        LightElement.Fire,
    ];

    public static bool IsBaseElement(this LightElement element) =>
        element is >= LightElement.Forest and <= LightElement.Fire;

    public static bool Matches(LightElement required, LightElement available) =>
        required == available ||
        required == LightElement.Prismatic ||
        available == LightElement.Prismatic;
}
