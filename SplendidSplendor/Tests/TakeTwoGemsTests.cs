using Xunit;
using SplendidSplendor.Model;
using SplendidSplendor.Logic;

namespace SplendidSplendor.Tests;

public class TakeTwoGemsTests
{
    private GameState CreateGame(int players = 2) => GameEngine.SetupGame(players);

    // === Validation ===

    [Fact]
    public void Take_2_same_is_valid_when_4_or_more_in_bank()
    {
        var state = CreateGame(); // 2 players = 4 each
        var action = GameAction.TakeTwoGems(GemType.White);
        Assert.True(ActionValidator.IsValid(state, action));
    }

    [Fact]
    public void Take_2_same_is_valid_when_more_than_4_in_bank()
    {
        var state = CreateGame(3); // 3 players = 5 each
        var action = GameAction.TakeTwoGems(GemType.Blue);
        Assert.True(ActionValidator.IsValid(state, action));
    }

    [Fact]
    public void Take_2_same_is_invalid_when_3_in_bank()
    {
        var state = CreateGame();
        state.Bank[GemType.White] = 3;
        var action = GameAction.TakeTwoGems(GemType.White);
        Assert.False(ActionValidator.IsValid(state, action));
    }

    [Fact]
    public void Take_2_same_is_invalid_when_0_in_bank()
    {
        var state = CreateGame();
        state.Bank[GemType.White] = 0;
        var action = GameAction.TakeTwoGems(GemType.White);
        Assert.False(ActionValidator.IsValid(state, action));
    }

    [Fact]
    public void Take_2_gold_is_invalid()
    {
        var state = CreateGame();
        var action = GameAction.TakeTwoGems(GemType.Gold);
        Assert.False(ActionValidator.IsValid(state, action));
    }

    // === Application ===

    [Fact]
    public void Apply_take_2_decreases_bank_by_2()
    {
        var state = CreateGame(); // 4 white
        var action = GameAction.TakeTwoGems(GemType.White);
        GameEngine.ApplyAction(state, action);
        Assert.Equal(2, state.Bank[GemType.White]);
    }

    [Fact]
    public void Apply_take_2_increases_player_gems_by_2()
    {
        var state = CreateGame();
        var action = GameAction.TakeTwoGems(GemType.Blue);
        GameEngine.ApplyAction(state, action);
        Assert.Equal(2, state.Players[0].Gems[GemType.Blue]);
    }

    [Fact]
    public void Apply_take_2_advances_turn()
    {
        var state = CreateGame();
        var action = GameAction.TakeTwoGems(GemType.White);
        GameEngine.ApplyAction(state, action);
        Assert.Equal(1, state.CurrentPlayerIndex);
    }

    [Fact]
    public void Other_bank_colors_unchanged()
    {
        var state = CreateGame(); // 4 each
        var action = GameAction.TakeTwoGems(GemType.White);
        GameEngine.ApplyAction(state, action);
        Assert.Equal(4, state.Bank[GemType.Blue]);
        Assert.Equal(4, state.Bank[GemType.Green]);
        Assert.Equal(5, state.Bank[GemType.Gold]);
    }
}
