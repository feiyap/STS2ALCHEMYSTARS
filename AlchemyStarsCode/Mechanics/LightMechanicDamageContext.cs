namespace AlchemyStars.Mechanics;

/// <summary>
/// 当前正在结算的属性伤害类型上下文。
/// 使用普通静态栈，而不是 AsyncLocal：战斗逻辑会 await 原版计时器，
/// 那种等待不会带回执行上下文。即时模式不等待、普通/快速模式会等待，
/// 若上下文因此丢失，同一段伤害后续钩子就会在两边走出不同结果。
/// </summary>
public static class LightMechanicDamageContext
{
    private sealed class Frame
    {
        public LightElement? Element;
        public bool FireAndThunder;
        public bool WaterBonusDoubled;
    }

    private static readonly Stack<Frame> Frames = new();
    private static int _suppressOutgoing;
    private static int _skipRepeatedAttackMultipliers;

    /// <summary>
    /// 预先把力量、易伤算进伤害时先关掉属性增伤，留给真正的那次攻击乘一次。
    /// </summary>
    public static bool SuppressOutgoingMultiplier => _suppressOutgoing > 0;

    /// <summary>
    /// 旧印群体伤害已经吃过克制倍率。随后的无力量攻击不要再乘一次。
    /// </summary>
    public static bool SkipRepeatedAttackMultipliers => _skipRepeatedAttackMultipliers > 0;

    public static LightElement? CurrentElement
    {
        get => Frames.Count > 0 ? Frames.Peek().Element : null;
        set
        {
            if (Frames.Count > 0)
                Frames.Peek().Element = value;
        }
    }

    /// <summary>
    /// 伤害同时视为火与雷（享受火/雷格加成与特效，不含森/水格加成）。
    /// </summary>
    public static bool IsFireAndThunder =>
        Frames.Count > 0 && Frames.Peek().FireAndThunder;

    /// <summary>
    /// 水属性格伤害加成翻倍（青瞳·烈氮雨）。
    /// </summary>
    public static bool IsWaterBonusDoubled =>
        Frames.Count > 0 && Frames.Peek().WaterBonusDoubled;

    public static IDisposable SuppressOutgoing()
    {
        _suppressOutgoing++;
        return new CounterScope(() => _suppressOutgoing--);
    }

    public static IDisposable BeginResolvedAttack()
    {
        _skipRepeatedAttackMultipliers++;
        return new CounterScope(() => _skipRepeatedAttackMultipliers--);
    }

    public static IDisposable Use(LightElement element)
    {
        Frames.Push(new Frame
        {
            Element = element,
            FireAndThunder = false,
            WaterBonusDoubled = CurrentWaterBonus()
        });
        return new Scope();
    }

    public static IDisposable UseFireAndThunder()
    {
        Frames.Push(new Frame
        {
            Element = LightElement.Prismatic,
            FireAndThunder = true,
            WaterBonusDoubled = false
        });
        return new Scope();
    }

    public static IDisposable UseWaterBonusDoubled()
    {
        Frames.Push(new Frame
        {
            Element = CurrentElement,
            FireAndThunder = IsFireAndThunder,
            WaterBonusDoubled = true
        });
        return new Scope();
    }

    private static bool CurrentWaterBonus() =>
        Frames.Count > 0 && Frames.Peek().WaterBonusDoubled;

    private sealed class Scope : IDisposable
    {
        public void Dispose()
        {
            if (Frames.Count > 0)
                Frames.Pop();
        }
    }

    private sealed class CounterScope : IDisposable
    {
        private readonly Action _onDispose;

        public CounterScope(Action onDispose) => _onDispose = onDispose;

        public void Dispose() => _onDispose();
    }
}
