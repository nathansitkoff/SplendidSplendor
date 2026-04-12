using Xunit;
using SplendidSplendor.Model;
using SplendidSplendor.Logic;

namespace SplendidSplendor.Tests;

public class TakeGemsTests
{
    private GameState CreateGame(int players = 2) => GameEngine.SetupGame(players);

    // === Validation ===

    [Fact]
    public void Take_3_different_colors_is_valid()
    {
        var state = CreateGame();
        var action = GameAction.TakeThreeGems(GemType.White, GemType.Blue, GemType.Green);
        Assert.True(ActionValidator.IsValid(state, action));
    }

    [Fact]
    public void Take_2_different_colors_is_valid_when_only_2_available()
    {
        var state = CreateGame();
        // Empty out 3 colors so only 2 remain
        state.Bank[GemType.Green] = 0;
        state.Bank[GemType.Red] = 0;
        state.Bank[GemType.Black] = 0;
        var action = GameAction.TakeThreeGems(GemType.White, GemType.Blue);
        Assert.True(ActionValidator.IsValid(state, action));
    }

    [Fact]
    public void Take_1_color_is_valid_when_only_1_available()
    {
        var state = CreateGame();
        state.Bank[GemType.Blue] = 0;
        state.Bank[GemType.Green] = 0;
        state.Bank[GemType.Red] = 0;
        state.Bank[GemType.Black] = 0;
        var action = GameAction.TakeThreeGems(GemType.White);
        Assert.True(ActionValidator.IsValid(state, action));
    }

    [Fact]
    public void Take_3_with_duplicate_color_is_invalid()
    {
        var state = CreateGame();
        var action = GameAction.TakeThreeGems(GemType.White, GemType.White, GemType.Blue);
        Assert.False(ActionValidator.IsValid(state, action));
    }

    [Fact]
    public void Take_from_empty_color_is_invalid()
    {
        var state = CreateGame();
        state.Bank[GemType.White] = 0;
        var action = GameAction.TakeThreeGems(GemType.White, GemType.Blue, GemType.Green);
        Assert.False(ActionValidator.IsValid(state, action));
    }

    [Fact]
    public void Take_gold_is_invalid()
    {
        var state = CreateGame();
        var action = GameAction.TakeThreeGems(GemType.Gold, GemType.Blue, GemType.Green);
        Assert.False(ActionValidator.IsValid(state, action));
    }

    [Fact]
    public void Take_0_gems_is_invalid()
    {
        var state = CreateGame();
        var action = GameAction.TakeThreeGems();
        Assert.False(ActionValidator.IsValid(state, action));
    }

    [Fact]
    public void Take_2_when_3_colors_available_is_invalid()
    {
        var state = CreateGame();
        // All 5 colors available, so must take 3
        var action = GameAction.TakeThreeGems(GemType.White, GemType.Blue);
        Assert.False(ActionValidator.IsValid(state, action));
    }

    // === Application ===

    [Fact]
    public void Apply_take_3_decreases_bank_by_1_each()
    {
        var state = CreateGame(); // 2 players = 4 gems each
        var action = GameAction.TakeThreeGems(GemType.White, GemType.Blue, GemType.Green);
        GameEngine.ApplyAction(state, action);
        Assert.Equal(3, state.Bank[GemType.White]);
        Assert.Equal(3, state.Bank[GemType.Blue]);
        Assert.Equal(3, state.Bank[GemType.Green]);
    }

    [Fact]
    public void Apply_take_3_increases_player_gems_by_1_each()
    {
        var state = CreateGame();
        var action = GameAction.TakeThreeGems(GemType.White, GemType.Blue, GemType.Green);
        GameEngine.ApplyAction(state, action);
        // Player 0 took the gems, but turn already advanced, so check player 0
        Assert.Equal(1, state.Players[0].Gems[GemType.White]);
        Assert.Equal(1, state.Players[0].Gems[GemType.Blue]);
        Assert.Equal(1, state.Players[0].Gems[GemType.Green]);
    }

    [Fact]
    public void Apply_take_3_advances_turn()
    {
        var state = CreateGame();
        Assert.Equal(0, state.CurrentPlayerIndex);
        var action = GameAction.TakeThreeGems(GemType.White, GemType.Blue, GemType.Green);
        GameEngine.ApplyAction(state, action);
        Assert.Equal(1, state.CurrentPlayerIndex);
    }

    [Fact]
    public void Turn_wraps_around_to_player_0()
    {
        var state = CreateGame(); // 2 players
        GameEngine.ApplyAction(state, GameAction.TakeThreeGems(GemType.White, GemType.Blue, GemType.Green));
        Assert.Equal(1, state.CurrentPlayerIndex);
        GameEngine.ApplyAction(state, GameAction.TakeThreeGems(GemType.Red, GemType.Black, GemType.White));
        Assert.Equal(0, state.CurrentPlayerIndex);
    }

    [Fact]
    public void Apply_invalid_action_throws()
    {
        var state = CreateGame();
        state.Bank[GemType.White] = 0;
        var action = GameAction.TakeThreeGems(GemType.White, GemType.Blue, GemType.Green);
        Assert.Throws<InvalidOperationException>(() => GameEngine.ApplyAction(state, action));
    }

    [Fact]
    public void Untouched_bank_colors_unchanged()
    {
        var state = CreateGame(); // 4 each
        var action = GameAction.TakeThreeGems(GemType.White, GemType.Blue, GemType.Green);
        GameEngine.ApplyAction(state, action);
        Assert.Equal(4, state.Bank[GemType.Red]);
        Assert.Equal(4, state.Bank[GemType.Black]);
        Assert.Equal(5, state.Bank[GemType.Gold]);
    }
}
