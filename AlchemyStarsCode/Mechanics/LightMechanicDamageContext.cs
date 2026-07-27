namespace AlchemyStars.Mechanics;

/// <summary>
/// 当前正在结算的属性伤害类型上下文�?
/// </summary>
public static class LightMechanicDamageContext
{
    private static readonly AsyncLocal<LightElement?> Current = new();

    public static LightElement? CurrentElement
    {
        get => Current.Value;
        set => Current.Value = value;
    }

    public static IDisposable Use(LightElement element)
    {
        var previous = Current.Value;
        Current.Value = element;
        return new Scope(previous);
    }

    private sealed class Scope(LightElement? previous) : IDisposable
    {
        public void Dispose() => Current.Value = previous;
    }
}
