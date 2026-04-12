using Xunit;
using SplendidSplendor.Model;
using SplendidSplendor.Logic;

namespace SplendidSplendor.Tests;

public class CardDatabaseTests
{
    private readonly CardDatabase _db;

    public CardDatabaseTests()
    {
        _db = CardDatabase.Load();
    }

    [Fact]
    public void Loads_90_development_cards()
    {
        Assert.Equal(90, _db.AllCards.Count);
    }

    [Fact]
    public void Loads_40_tier_one_cards()
    {
        Assert.Equal(40, _db.AllCards.Count(c => c.Tier == 1));
    }

    [Fact]
    public void Loads_30_tier_two_cards()
    {
        Assert.Equal(30, _db.AllCards.Count(c => c.Tier == 2));
    }

    [Fact]
    public void Loads_20_tier_three_cards()
    {
        Assert.Equal(20, _db.AllCards.Count(c => c.Tier == 3));
    }

    [Fact]
    public void Loads_10_nobles()
    {
        Assert.Equal(10, _db.AllNobles.Count);
    }

    [Fact]
    public void All_nobles_are_worth_3_points()
    {
        Assert.All(_db.AllNobles, n => Assert.Equal(3, n.Points));
    }

    [Fact]
    public void Every_card_has_a_valid_bonus_type()
    {
        var validTypes = new[] { GemType.White, GemType.Blue, GemType.Green, GemType.Red, GemType.Black };
        Assert.All(_db.AllCards, c => Assert.Contains(c.BonusType, validTypes));
    }

    [Fact]
    public void Every_card_has_non_negative_cost()
    {
        Assert.All(_db.AllCards, c => Assert.False(c.Cost.HasNegative));
    }

    [Fact]
    public void Every_card_costs_at_least_one_gem()
    {
        Assert.All(_db.AllCards, c => Assert.True(c.Cost.Total > 0));
    }

    [Fact]
    public void Every_card_has_non_negative_points()
    {
        Assert.All(_db.AllCards, c => Assert.True(c.Points >= 0));
    }

    [Fact]
    public void Noble_requirements_use_only_non_gold_gems()
    {
        Assert.All(_db.AllNobles, n =>
        {
            Assert.Equal(0, n.Requirements[GemType.Gold]);
        });
    }

    [Fact]
    public void Noble_requirements_are_non_negative()
    {
        Assert.All(_db.AllNobles, n => Assert.False(n.Requirements.HasNegative));
    }
}
