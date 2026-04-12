using Xunit;
using SplendidSplendor.Model;
using SplendidSplendor.Logic;

namespace SplendidSplendor.Tests;

public class ReserveCardTests
{
    private GameState CreateGame() => GameEngine.SetupGame(2);

    private Card MakeCard(int tier, GemType bonus, int points, GemCollection cost)
        => new() { Tier = tier, BonusType = bonus, Points = points, Cost = cost };

    // === Reserve from market ===

    [Fact]
    public void Reserve_from_market_is_valid()
    {
        var state = CreateGame();
        var action = GameAction.ReserveCard(0, 0);
        Assert.True(ActionValidator.IsValid(state, action));
    }

    [Fact]
    public void Reserve_moves_card_to_player_reserved()
    {
        var state = CreateGame();
        var card = state.TierMarket[0][0];
        GameEngine.ApplyAction(state, GameAction.ReserveCard(0, 0));
        Assert.Contains(card, state.Players[0].ReservedCards);
    }

    [Fact]
    public void Reserve_gives_player_one_gold()
    {
        var state = CreateGame(); // 5 gold in bank
        GameEngine.ApplyAction(state, GameAction.ReserveCard(0, 0));
        Assert.Equal(1, state.Players[0].Gems[GemType.Gold]);
        Assert.Equal(4, state.Bank[GemType.Gold]);
    }

    [Fact]
    public void Reserve_refills_market_from_deck()
    {
        var state = CreateGame();
        int deckBefore = state.TierDecks[0].Count;
        GameEngine.ApplyAction(state, GameAction.ReserveCard(0, 0));
        Assert.Equal(4, state.TierMarket[0].Count);
        Assert.Equal(deckBefore - 1, state.TierDecks[0].Count);
    }

    [Fact]
    public void Reserve_advances_turn()
    {
        var state = CreateGame();
        GameEngine.ApplyAction(state, GameAction.ReserveCard(0, 0));
        Assert.Equal(1, state.CurrentPlayerIndex);
    }

    // === Reserve from deck top ===

    [Fact]
    public void Reserve_from_deck_is_valid()
    {
        var state = CreateGame();
        var action = GameAction.ReserveCard(0, null);
        Assert.True(ActionValidator.IsValid(state, action));
    }

    [Fact]
    public void Reserve_from_deck_takes_top_card()
    {
        var state = CreateGame();
        var topCard = state.TierDecks[0][0];
        GameEngine.ApplyAction(state, GameAction.ReserveCard(0, null));
        Assert.Contains(topCard, state.Players[0].ReservedCards);
    }

    [Fact]
    public void Reserve_from_deck_does_not_change_market_count()
    {
        var state = CreateGame();
        GameEngine.ApplyAction(state, GameAction.ReserveCard(0, null));
        Assert.Equal(4, state.TierMarket[0].Count);
    }

    [Fact]
    public void Reserve_from_empty_deck_is_invalid()
    {
        var state = CreateGame();
        state.TierDecks[0].Clear();
        var action = GameAction.ReserveCard(0, null);
        Assert.False(ActionValidator.IsValid(state, action));
    }

    // === Max 3 reserves ===

    [Fact]
    public void Cannot_reserve_when_already_holding_3()
    {
        var state = CreateGame();
        state.CurrentPlayer.ReservedCards.Add(MakeCard(1, GemType.White, 0, new GemCollection { [GemType.White] = 1 }));
        state.CurrentPlayer.ReservedCards.Add(MakeCard(1, GemType.White, 0, new GemCollection { [GemType.White] = 1 }));
        state.CurrentPlayer.ReservedCards.Add(MakeCard(1, GemType.White, 0, new GemCollection { [GemType.White] = 1 }));

        var action = GameAction.ReserveCard(0, 0);
        Assert.False(ActionValidator.IsValid(state, action));
    }

    // === Gold edge case ===

    [Fact]
    public void No_gold_given_when_gold_bank_is_empty()
    {
        var state = CreateGame();
        state.Bank[GemType.Gold] = 0;
        GameEngine.ApplyAction(state, GameAction.ReserveCard(0, 0));
        Assert.Equal(0, state.Players[0].Gems[GemType.Gold]);
        Assert.Equal(0, state.Bank[GemType.Gold]);
    }

    // === Purchase from reserved ===

    [Fact]
    public void Purchase_reserved_is_valid_when_affordable()
    {
        var state = CreateGame();
        var card = MakeCard(1, GemType.Blue, 1, new GemCollection { [GemType.White] = 1 });
        state.CurrentPlayer.ReservedCards.Add(card);
        state.CurrentPlayer.Gems[GemType.White] = 1;

        var action = GameAction.PurchaseReserved(0);
        Assert.True(ActionValidator.IsValid(state, action));
    }

    [Fact]
    public void Purchase_reserved_is_invalid_when_cant_afford()
    {
        var state = CreateGame();
        var card = MakeCard(1, GemType.Blue, 1, new GemCollection { [GemType.White] = 5 });
        state.CurrentPlayer.ReservedCards.Add(card);

        var action = GameAction.PurchaseReserved(0);
        Assert.False(ActionValidator.IsValid(state, action));
    }

    [Fact]
    public void Purchase_reserved_moves_card_to_tableau()
    {
        var state = CreateGame();
        var card = MakeCard(1, GemType.Blue, 1, new GemCollection { [GemType.White] = 1 });
        state.CurrentPlayer.ReservedCards.Add(card);
        state.CurrentPlayer.Gems[GemType.White] = 1;

        GameEngine.ApplyAction(state, GameAction.PurchaseReserved(0));

        Assert.Contains(card, state.Players[0].OwnedCards);
        Assert.DoesNotContain(card, state.Players[0].ReservedCards);
    }

    [Fact]
    public void Purchase_reserved_spends_gems()
    {
        var state = CreateGame();
        var card = MakeCard(1, GemType.Blue, 0, new GemCollection { [GemType.Red] = 2 });
        state.CurrentPlayer.ReservedCards.Add(card);
        state.CurrentPlayer.Gems[GemType.Red] = 3;

        GameEngine.ApplyAction(state, GameAction.PurchaseReserved(0));

        Assert.Equal(1, state.Players[0].Gems[GemType.Red]);
    }

    [Fact]
    public void Purchase_reserved_invalid_index()
    {
        var state = CreateGame();
        var action = GameAction.PurchaseReserved(0);
        Assert.False(ActionValidator.IsValid(state, action)); // no reserved cards
    }

    [Fact]
    public void Purchase_reserved_advances_turn()
    {
        var state = CreateGame();
        var card = MakeCard(1, GemType.Blue, 0, new GemCollection { [GemType.White] = 1 });
        state.CurrentPlayer.ReservedCards.Add(card);
        state.CurrentPlayer.Gems[GemType.White] = 1;

        GameEngine.ApplyAction(state, GameAction.PurchaseReserved(0));
        Assert.Equal(1, state.CurrentPlayerIndex);
    }
}
