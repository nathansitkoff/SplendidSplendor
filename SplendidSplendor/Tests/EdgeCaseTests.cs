using Xunit;
using SplendidSplendor.Model;
using SplendidSplendor.Logic;

namespace SplendidSplendor.Tests;

public class EdgeCaseTests
{
    // GameEngine.SetupGame with invalid player counts
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(-1)]
    public void SetupGame_invalid_player_count_throws(int count)
    {
        Assert.Throws<ArgumentException>(() => GameEngine.SetupGame(count));
    }

    // ActionValidator with unknown action type — can't easily test since
    // GameAction constructor is private, but we can test invalid tier on reserve
    [Theory]
    [InlineData(-1)]
    [InlineData(3)]
    [InlineData(10)]
    public void Reserve_invalid_tier_is_rejected(int tier)
    {
        var state = GameEngine.SetupGame(2);
        var action = GameAction.ReserveCard(tier, 0);
        Assert.False(ActionValidator.IsValid(state, action));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(3)]
    [InlineData(10)]
    public void Reserve_from_deck_invalid_tier_is_rejected(int tier)
    {
        var state = GameEngine.SetupGame(2);
        var action = GameAction.ReserveCard(tier, null);
        Assert.False(ActionValidator.IsValid(state, action));
    }

    // GameState.FinalRound is settable
    [Fact]
    public void GameState_FinalRound_is_settable()
    {
        var state = GameEngine.SetupGame(2);
        state.FinalRound = 3;
        Assert.Equal(3, state.FinalRound);
    }
}
