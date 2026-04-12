namespace SplendidSplendor.Model;

public class GemCollection
{
    private readonly Dictionary<GemType, int> _gems = new();

    public int this[GemType type]
    {
        get => _gems.TryGetValue(type, out var count) ? count : 0;
        set => _gems[type] = value;
    }

    public int Total => _gems.Values.Sum();

    public bool HasNegative => _gems.Values.Any(v => v < 0);

    public GemCollection Add(GemCollection other)
    {
        var result = new GemCollection();
        foreach (GemType type in Enum.GetValues<GemType>())
        {
            result[type] = this[type] + other[type];
        }
        return result;
    }

    public GemCollection Subtract(GemCollection other)
    {
        var result = new GemCollection();
        foreach (GemType type in Enum.GetValues<GemType>())
        {
            result[type] = this[type] - other[type];
        }
        return result;
    }

    public bool CanAfford(GemCollection cost)
    {
        foreach (GemType type in Enum.GetValues<GemType>())
        {
            if (this[type] < cost[type])
                return false;
        }
        return true;
    }
}
