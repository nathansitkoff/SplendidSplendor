using Xunit;
using SplendidSplendor.Model;
using SplendidSplendor.Logic;

namespace SplendidSplendor.Tests;

public class DiscardTests
{
    private GameState CreateGame() => GameEngine.SetupGame(2);

    // === Detection ===

    [Fact]
    public void Taking_gems_over_10_requires_discard()
    {
        var state = CreateGame();
        // Give player 8 gems, take 3 = 11
        state.CurrentPlayer.Gems[GemType.White] = 3;
        state.CurrentPlayer.Gems[GemType.Blue] = 3;
        state.CurrentPlayer.Gems[GemType.Green] = 2;

        GameEngine.ApplyAction(state, GameAction.TakeThreeGems(GemType.Red, GemType.Black, GemType.White));

        Assert.True(state.NeedsDiscard);
        // Turn should NOT advance yet
        Assert.Equal(0, state.CurrentPlayerIndex);
    }

    [Fact]
    public void Taking_gems_at_or_under_10_does_not_require_discard()
    {
        var state = CreateGame();
        // Player has 0 gems, takes 3 = 3, no discard needed
        GameEngine.ApplyAction(state, GameAction.TakeThreeGems(GemType.White, GemType.Blue, GemType.Green));

        Assert.False(state.NeedsDiscard);
        Assert.Equal(1, state.CurrentPlayerIndex); // turn advanced
    }

    [Fact]
    public void Take_2_over_10_requires_discard()
    {
        var state = CreateGame(3); // 5 gems each
        state.CurrentPlayer.Gems[GemType.White] = 4;
        state.CurrentPlayer.Gems[GemType.Blue] = 5;

        GameEngine.ApplyAction(state, GameAction.TakeTwoGems(GemType.Green));

        Assert.True(state.NeedsDiscard);
        Assert.Equal(0, state.CurrentPlayerIndex);
    }

    // === Discard action ===

    [Fact]
    public void Discard_valid_when_over_10()
    {
        var state = CreateGame();
        state.CurrentPlayer.Gems[GemType.White] = 5;
        state.CurrentPlayer.Gems[GemType.Blue] = 6;
        state.NeedsDiscard = true;

        var action = GameAction.DiscardGems(new GemCollection { [GemType.Blue] = 1 });
        Assert.True(ActionValidator.IsValid(state, action));
    }

    [Fact]
    public void Discard_invalid_when_not_in_discard_state()
    {
        var state = CreateGame();
        state.CurrentPlayer.Gems[GemType.White] = 5;
        state.CurrentPlayer.Gems[GemType.Blue] = 6;
        state.NeedsDiscard = false;

        var action = GameAction.DiscardGems(new GemCollection { [GemType.Blue] = 1 });
        Assert.False(ActionValidator.IsValid(state, action));
    }

    [Fact]
    public void Discard_must_bring_total_to_10()
    {
        var state = CreateGame();
        state.CurrentPlayer.Gems[GemType.White] = 5;
        state.CurrentPlayer.Gems[GemType.Blue] = 7;
        state.NeedsDiscard = true;

        // Discard only 1 — still at 11, invalid
        var tooFew = GameAction.DiscardGems(new GemCollection { [GemType.Blue] = 1 });
        Assert.False(ActionValidator.IsValid(state, tooFew));

        // Discard 2 — at 10, valid
        var justRight = GameAction.DiscardGems(new GemCollection { [GemType.Blue] = 2 });
        Assert.True(ActionValidator.IsValid(state, justRight));

        // Discard 3 — at 9, invalid (must be exactly 10)
        var tooMany = GameAction.DiscardGems(new GemCollection { [GemType.Blue] = 3 });
        Assert.False(ActionValidator.IsValid(state, tooMany));
    }

    [Fact]
    public void Cannot_discard_more_than_you_have()
    {
        var state = CreateGame();
        state.CurrentPlayer.Gems[GemType.White] = 11;
        state.NeedsDiscard = true;

        var action = GameAction.DiscardGems(new GemCollection { [GemType.Blue] = 1 });
        Assert.False(ActionValidator.IsValid(state, action)); // player has 0 blue
    }

    // === Application ===

    [Fact]
    public void Discard_returns_gems_to_bank_and_advances_turn()
    {
        var state = CreateGame(); // bank has 4 white
        state.CurrentPlayer.Gems[GemType.White] = 8;
        state.CurrentPlayer.Gems[GemType.Blue] = 3;
        state.NeedsDiscard = true;

        GameEngine.ApplyAction(state, GameAction.DiscardGems(new GemCollection { [GemType.Blue] = 1 }));

        Assert.Equal(2, state.Players[0].Gems[GemType.Blue]);
        Assert.Equal(5, state.Bank[GemType.Blue]); // 4 + 1
        Assert.False(state.NeedsDiscard);
        Assert.Equal(1, state.CurrentPlayerIndex); // turn now advances
    }

    [Fact]
    public void Cannot_take_gems_while_in_discard_state()
    {
        var state = CreateGame();
        state.NeedsDiscard = true;

        var take = GameAction.TakeThreeGems(GemType.White, GemType.Blue, GemType.Green);
        Assert.False(ActionValidator.IsValid(state, take));
    }

    [Fact]
    public void Cannot_purchase_while_in_discard_state()
    {
        var state = CreateGame();
        state.NeedsDiscard = true;
        state.CurrentPlayer.Gems[GemType.White] = 10;

        var purchase = GameAction.PurchaseCard(0, 0);
        Assert.False(ActionValidator.IsValid(state, purchase));
    }

    [Fact]
    public void Cannot_reserve_while_in_discard_state()
    {
        var state = CreateGame();
        state.NeedsDiscard = true;

        var reserve = GameAction.ReserveCard(0, 0);
        Assert.False(ActionValidator.IsValid(state, reserve));
    }

    private GameState CreateGame(int players = 2) => GameEngine.SetupGame(players);
}
