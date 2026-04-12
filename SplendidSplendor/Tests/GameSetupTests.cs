using Xunit;
using SplendidSplendor.Model;
using SplendidSplendor.Logic;

namespace SplendidSplendor.Tests;

public class GameSetupTests
{
    [Theory]
    [InlineData(2, 4)]
    [InlineData(3, 5)]
    [InlineData(4, 7)]
    public void Bank_has_correct_gem_count_per_player_count(int playerCount, int expectedGems)
    {
        var state = GameEngine.SetupGame(playerCount);
        Assert.Equal(expectedGems, state.Bank[GemType.White]);
        Assert.Equal(expectedGems, state.Bank[GemType.Blue]);
        Assert.Equal(expectedGems, state.Bank[GemType.Green]);
        Assert.Equal(expectedGems, state.Bank[GemType.Red]);
        Assert.Equal(expectedGems, state.Bank[GemType.Black]);
    }

    [Theory]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    public void Bank_always_has_5_gold(int playerCount)
    {
        var state = GameEngine.SetupGame(playerCount);
        Assert.Equal(5, state.Bank[GemType.Gold]);
    }

    [Theory]
    [InlineData(2, 3)]
    [InlineData(3, 4)]
    [InlineData(4, 5)]
    public void Noble_count_is_player_count_plus_one(int playerCount, int expectedNobles)
    {
        var state = GameEngine.SetupGame(playerCount);
        Assert.Equal(expectedNobles, state.Nobles.Count);
    }

    [Theory]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    public void Each_tier_market_has_4_cards(int playerCount)
    {
        var state = GameEngine.SetupGame(playerCount);
        for (int tier = 0; tier < 3; tier++)
        {
            Assert.Equal(4, state.TierMarket[tier].Count);
        }
    }

    [Theory]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    public void Market_cards_removed_from_decks(int playerCount)
    {
        var state = GameEngine.SetupGame(playerCount);
        int totalCards = 0;
        for (int tier = 0; tier < 3; tier++)
        {
            totalCards += state.TierDecks[tier].Count + state.TierMarket[tier].Count;
        }
        Assert.Equal(90, totalCards);
    }

    [Theory]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    public void Correct_number_of_players(int playerCount)
    {
        var state = GameEngine.SetupGame(playerCount);
        Assert.Equal(playerCount, state.Players.Count);
    }

    [Theory]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    public void Players_start_with_zero_gems(int playerCount)
    {
        var state = GameEngine.SetupGame(playerCount);
        foreach (var player in state.Players)
        {
            Assert.Equal(0, player.Gems.Total);
        }
    }

    [Theory]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    public void Players_start_with_zero_cards(int playerCount)
    {
        var state = GameEngine.SetupGame(playerCount);
        foreach (var player in state.Players)
        {
            Assert.Empty(player.OwnedCards);
            Assert.Empty(player.ReservedCards);
        }
    }

    [Theory]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    public void Players_start_with_zero_points(int playerCount)
    {
        var state = GameEngine.SetupGame(playerCount);
        foreach (var player in state.Players)
        {
            Assert.Equal(0, player.Score);
        }
    }

    [Fact]
    public void Current_player_starts_at_zero()
    {
        var state = GameEngine.SetupGame(2);
        Assert.Equal(0, state.CurrentPlayerIndex);
    }

    [Fact]
    public void Game_end_not_triggered_at_start()
    {
        var state = GameEngine.SetupGame(2);
        Assert.False(state.GameEndTriggered);
    }

    [Fact]
    public void Market_cards_match_their_tier()
    {
        var state = GameEngine.SetupGame(2);
        for (int tier = 0; tier < 3; tier++)
        {
            Assert.All(state.TierMarket[tier], c => Assert.Equal(tier + 1, c.Tier));
        }
    }

    [Fact]
    public void Deck_cards_match_their_tier()
    {
        var state = GameEngine.SetupGame(2);
        for (int tier = 0; tier < 3; tier++)
        {
            Assert.All(state.TierDecks[tier], c => Assert.Equal(tier + 1, c.Tier));
        }
    }
}
