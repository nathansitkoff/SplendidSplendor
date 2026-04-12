using Xunit;
using SplendidSplendor.Model;

namespace SplendidSplendor.Tests;

public class GemCollectionTests
{
    [Fact]
    public void New_collection_has_zero_for_all_gems()
    {
        var gems = new GemCollection();
        foreach (GemType type in Enum.GetValues<GemType>())
        {
            Assert.Equal(0, gems[type]);
        }
    }

    [Fact]
    public void Indexer_sets_and_gets_values()
    {
        var gems = new GemCollection();
        gems[GemType.Blue] = 3;
        Assert.Equal(3, gems[GemType.Blue]);
        Assert.Equal(0, gems[GemType.Red]);
    }

    [Fact]
    public void Add_combines_two_collections()
    {
        var a = new GemCollection { [GemType.Blue] = 2, [GemType.Red] = 1 };
        var b = new GemCollection { [GemType.Blue] = 1, [GemType.Green] = 3 };
        var result = a.Add(b);
        Assert.Equal(3, result[GemType.Blue]);
        Assert.Equal(1, result[GemType.Red]);
        Assert.Equal(3, result[GemType.Green]);
    }

    [Fact]
    public void Subtract_removes_values()
    {
        var a = new GemCollection { [GemType.Blue] = 5, [GemType.Red] = 2 };
        var b = new GemCollection { [GemType.Blue] = 3, [GemType.Red] = 1 };
        var result = a.Subtract(b);
        Assert.Equal(2, result[GemType.Blue]);
        Assert.Equal(1, result[GemType.Red]);
    }

    [Fact]
    public void CanAfford_returns_true_when_sufficient()
    {
        var have = new GemCollection { [GemType.Blue] = 3, [GemType.Red] = 2 };
        var cost = new GemCollection { [GemType.Blue] = 2, [GemType.Red] = 1 };
        Assert.True(have.CanAfford(cost));
    }

    [Fact]
    public void CanAfford_returns_false_when_insufficient()
    {
        var have = new GemCollection { [GemType.Blue] = 1 };
        var cost = new GemCollection { [GemType.Blue] = 2 };
        Assert.False(have.CanAfford(cost));
    }

    [Fact]
    public void CanAfford_returns_true_for_zero_cost()
    {
        var have = new GemCollection();
        var cost = new GemCollection();
        Assert.True(have.CanAfford(cost));
    }

    [Fact]
    public void Total_returns_sum_of_all_gems()
    {
        var gems = new GemCollection
        {
            [GemType.White] = 1,
            [GemType.Blue] = 2,
            [GemType.Green] = 3
        };
        Assert.Equal(6, gems.Total);
    }

    [Fact]
    public void HasNegative_detects_negative_values()
    {
        var gems = new GemCollection { [GemType.Blue] = -1 };
        Assert.True(gems.HasNegative);
    }

    [Fact]
    public void HasNegative_false_for_non_negative()
    {
        var gems = new GemCollection { [GemType.Blue] = 0, [GemType.Red] = 3 };
        Assert.False(gems.HasNegative);
    }
}
