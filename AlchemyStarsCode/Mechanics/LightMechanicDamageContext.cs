namespace AlchemyStars.Mechanics;

/// <summary>
/// 当前正在结算的属性伤害类型上下文。
/// </summary>
public static class LightMechanicDamageContext
{
    private static readonly AsyncLocal<LightElement?> Current = new();
    private static readonly AsyncLocal<bool> FireAndThunder = new();

    public static LightElement? CurrentElement
    {
        get => Current.Value;
        set => Current.Value = value;
    }

    /// <summary>
    /// 伤害同时视为火与雷（享受火/雷格加成与特效，不含森/水格加成）。
    /// </summary>
    public static bool IsFireAndThunder => FireAndThunder.Value;

    public static IDisposable Use(LightElement element)
    {
        var previous = Current.Value;
        var previousDual = FireAndThunder.Value;
        Current.Value = element;
        FireAndThunder.Value = false;
        return new Scope(previous, previousDual);
    }

    public static IDisposable UseFireAndThunder()
    {
        var previous = Current.Value;
        var previousDual = FireAndThunder.Value;
        Current.Value = LightElement.Prismatic;
        FireAndThunder.Value = true;
        return new Scope(previous, previousDual);
    }

    private sealed class Scope(LightElement? previous, bool previousDual) : IDisposable
    {
        public void Dispose()
        {
            Current.Value = previous;
            FireAndThunder.Value = previousDual;
        }
    }
}
