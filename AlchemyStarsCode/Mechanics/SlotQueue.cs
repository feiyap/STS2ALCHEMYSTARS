namespace AlchemyStars.Mechanics;

/// <summary>
/// 有上限的 FIFO 队列；超出上限时移除最前方的元素�?
/// </summary>
public sealed class SlotQueue<T>
{
    private readonly List<T> _items = [];

    public int MaxSlots { get; private set; }

    public IReadOnlyList<T> Items => _items;

    public int Count => _items.Count;

    public SlotQueue(int maxSlots)
    {
        MaxSlots = Math.Max(1, maxSlots);
    }

    public void SetMaxSlots(int maxSlots)
    {
        MaxSlots = Math.Max(1, maxSlots);
        TrimOverflow();
    }

    public void Enqueue(T item)
    {
        _items.Add(item);
        TrimOverflow();
    }

    public void EnqueueMany(IEnumerable<T> items)
    {
        foreach (var item in items)
            Enqueue(item);
    }

    /// <summary>
    /// 入队并返回因溢出被移出的元素。
    /// </summary>
    public List<T> EnqueueReturningOverflow(T item)
    {
        _items.Add(item);
        return TrimOverflowReturning();
    }

    public bool TryConsumeFromFront(Func<T, bool> predicate, out T consumed)
    {
        for (var i = 0; i < _items.Count; i++)
        {
            if (!predicate(_items[i]))
                continue;

            consumed = _items[i];
            _items.RemoveAt(i);
            return true;
        }

        consumed = default!;
        return false;
    }

    public bool TryConsumeManyFromFront(IReadOnlyList<LightElement> cost, out List<T> consumedItems)
    {
        consumedItems = [];
        var working = _items.ToList();

        foreach (var required in cost)
        {
            var index = working.FindIndex(item => ItemMatches(item, required));
            if (index < 0)
            {
                consumedItems.Clear();
                return false;
            }

            consumedItems.Add(working[index]);
            working.RemoveAt(index);
        }

        _items.Clear();
        _items.AddRange(working);
        return true;
    }

    public void Clear() => _items.Clear();

    public void ReplaceAll(IEnumerable<T> items)
    {
        _items.Clear();
        _items.AddRange(items);
        TrimOverflow();
    }

    private void TrimOverflow()
    {
        while (_items.Count > MaxSlots)
            _items.RemoveAt(0);
    }

    private List<T> TrimOverflowReturning()
    {
        var removed = new List<T>();
        while (_items.Count > MaxSlots)
        {
            removed.Add(_items[0]);
            _items.RemoveAt(0);
        }

        return removed;
    }

    private static bool ItemMatches(T item, LightElement required)
    {
        if (item is LightElement element)
            return LightElementExtensions.Matches(required, element);

        return false;
    }
}
