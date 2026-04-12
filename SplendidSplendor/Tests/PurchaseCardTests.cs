using Xunit;
using SplendidSplendor.Model;
using SplendidSplendor.Logic;

namespace SplendidSplendor.Tests;

public class PurchaseCardTests
{
    private GameState CreateGame() => GameEngine.SetupGame(2);

    private Card MakeCard(int tier, GemType bonus, int points, GemCollection cost)
        => new() { Tier = tier, BonusType = bonus, Points = points, Cost = cost };

    // === Validation ===

    [Fact]
    public void Can_buy_card_with_exact_gems()
    {
        var state = CreateGame();
        var card = MakeCard(1, GemType.Blue, 0, new GemCollection { [GemType.White] = 2, [GemType.Red] = 1 });
        state.TierMarket[0] = new List<Card> { card };
        state.CurrentPlayer.Gems[GemType.White] = 2;
        state.CurrentPlayer.Gems[GemType.Red] = 1;

        var action = GameAction.PurchaseCard(0, 0);
        Assert.True(ActionValidator.IsValid(state, action));
    }

    [Fact]
    public void Cannot_buy_card_you_cant_afford()
    {
        var state = CreateGame();
        var card = MakeCard(1, GemType.Blue, 0, new GemCollection { [GemType.White] = 3 });
        state.TierMarket[0] = new List<Card> { card };
        state.CurrentPlayer.Gems[GemType.White] = 1;

        var action = GameAction.PurchaseCard(0, 0);
        Assert.False(ActionValidator.IsValid(state, action));
    }

    [Fact]
    public void Can_buy_with_bonus_discounts()
    {
        var state = CreateGame();
        var card = MakeCard(1, GemType.Green, 0, new GemCollection { [GemType.Blue] = 3 });
        state.TierMarket[0] = new List<Card> { card };
        // Player has 1 blue gem + 2 blue bonus cards
        state.CurrentPlayer.Gems[GemType.Blue] = 1;
        state.CurrentPlayer.OwnedCards.Add(MakeCard(1, GemType.Blue, 0, new GemCollection { [GemType.White] = 1 }));
        state.CurrentPlayer.OwnedCards.Add(MakeCard(1, GemType.Blue, 0, new GemCollection { [GemType.White] = 1 }));

        var action = GameAction.PurchaseCard(0, 0);
        Assert.True(ActionValidator.IsValid(state, action));
    }

    [Fact]
    public void Can_buy_with_gold_substitution()
    {
        var state = CreateGame();
        var card = MakeCard(1, GemType.Red, 0, new GemCollection { [GemType.White] = 2 });
        state.TierMarket[0] = new List<Card> { card };
        state.CurrentPlayer.Gems[GemType.White] = 1;
        state.CurrentPlayer.Gems[GemType.Gold] = 1;

        var action = GameAction.PurchaseCard(0, 0);
        Assert.True(ActionValidator.IsValid(state, action));
    }

    [Fact]
    public void Can_buy_free_card_with_enough_bonuses()
    {
        var state = CreateGame();
        var card = MakeCard(1, GemType.Red, 0, new GemCollection { [GemType.Blue] = 2 });
        state.TierMarket[0] = new List<Card> { card };
        // 2 blue bonuses = free
        state.CurrentPlayer.OwnedCards.Add(MakeCard(1, GemType.Blue, 0, new GemCollection { [GemType.White] = 1 }));
        state.CurrentPlayer.OwnedCards.Add(MakeCard(1, GemType.Blue, 0, new GemCollection { [GemType.White] = 1 }));

        var action = GameAction.PurchaseCard(0, 0);
        Assert.True(ActionValidator.IsValid(state, action));
    }

    [Fact]
    public void Invalid_market_index_is_rejected()
    {
        var state = CreateGame();
        var action = GameAction.PurchaseCard(0, 10);
        Assert.False(ActionValidator.IsValid(state, action));
    }

    [Fact]
    public void Invalid_tier_is_rejected()
    {
        var state = CreateGame();
        var action = GameAction.PurchaseCard(5, 0);
        Assert.False(ActionValidator.IsValid(state, action));
    }

    // === Application ===

    [Fact]
    public void Buy_card_moves_it_to_player_tableau()
    {
        var state = CreateGame();
        var card = MakeCard(1, GemType.Blue, 1, new GemCollection { [GemType.White] = 1 });
        state.TierMarket[0] = new List<Card> { card, MakeCard(1, GemType.Red, 0, new GemCollection { [GemType.White] = 1 }) };
        state.CurrentPlayer.Gems[GemType.White] = 1;

        GameEngine.ApplyAction(state, GameAction.PurchaseCard(0, 0));

        Assert.Contains(card, state.Players[0].OwnedCards);
        Assert.Equal(1, state.Players[0].Score);
    }

    [Fact]
    public void Buy_card_returns_gems_to_bank()
    {
        var state = CreateGame(); // bank has 4 white
        var card = MakeCard(1, GemType.Blue, 0, new GemCollection { [GemType.White] = 2 });
        state.TierMarket[0] = new List<Card> { card };
        state.CurrentPlayer.Gems[GemType.White] = 3;

        GameEngine.ApplyAction(state, GameAction.PurchaseCard(0, 0));

        Assert.Equal(1, state.Players[0].Gems[GemType.White]); // 3 - 2 = 1
        Assert.Equal(6, state.Bank[GemType.White]); // 4 + 2 = 6
    }

    [Fact]
    public void Buy_with_bonuses_spends_fewer_gems()
    {
        var state = CreateGame();
        var card = MakeCard(1, GemType.Green, 0, new GemCollection { [GemType.Blue] = 3 });
        state.TierMarket[0] = new List<Card> { card };
        state.CurrentPlayer.Gems[GemType.Blue] = 2;
        // 1 blue bonus
        state.CurrentPlayer.OwnedCards.Add(MakeCard(1, GemType.Blue, 0, new GemCollection { [GemType.White] = 1 }));

        GameEngine.ApplyAction(state, GameAction.PurchaseCard(0, 0));

        Assert.Equal(0, state.Players[0].Gems[GemType.Blue]); // spent 2 (3 - 1 bonus)
    }

    [Fact]
    public void Buy_with_gold_spends_gold_for_remainder()
    {
        var state = CreateGame();
        var card = MakeCard(1, GemType.Red, 0, new GemCollection { [GemType.White] = 3 });
        state.TierMarket[0] = new List<Card> { card };
        state.CurrentPlayer.Gems[GemType.White] = 1;
        state.CurrentPlayer.Gems[GemType.Gold] = 2;

        GameEngine.ApplyAction(state, GameAction.PurchaseCard(0, 0));

        Assert.Equal(0, state.Players[0].Gems[GemType.White]);
        Assert.Equal(0, state.Players[0].Gems[GemType.Gold]);
    }

    [Fact]
    public void Buy_card_refills_market_from_deck()
    {
        var state = CreateGame();
        var deckCard = state.TierDecks[0][0];
        var marketCard = state.TierMarket[0][0];
        state.CurrentPlayer.Gems[GemType.White] = 10;
        state.CurrentPlayer.Gems[GemType.Blue] = 10;
        state.CurrentPlayer.Gems[GemType.Green] = 10;
        state.CurrentPlayer.Gems[GemType.Red] = 10;
        state.CurrentPlayer.Gems[GemType.Black] = 10;

        int deckCountBefore = state.TierDecks[0].Count;
        GameEngine.ApplyAction(state, GameAction.PurchaseCard(0, 0));

        Assert.Equal(4, state.TierMarket[0].Count); // still 4 in market
        Assert.Equal(deckCountBefore - 1, state.TierDecks[0].Count);
        Assert.DoesNotContain(marketCard, state.TierMarket[0]);
    }

    [Fact]
    public void Buy_card_with_empty_deck_leaves_market_slot_empty()
    {
        var state = CreateGame();
        state.TierDecks[0].Clear(); // empty the deck
        state.CurrentPlayer.Gems[GemType.White] = 10;
        state.CurrentPlayer.Gems[GemType.Blue] = 10;
        state.CurrentPlayer.Gems[GemType.Green] = 10;
        state.CurrentPlayer.Gems[GemType.Red] = 10;
        state.CurrentPlayer.Gems[GemType.Black] = 10;

        GameEngine.ApplyAction(state, GameAction.PurchaseCard(0, 0));

        Assert.Equal(3, state.TierMarket[0].Count); // one fewer, no refill
    }

    [Fact]
    public void Buy_card_advances_turn()
    {
        var state = CreateGame();
        state.CurrentPlayer.Gems[GemType.White] = 10;
        state.CurrentPlayer.Gems[GemType.Blue] = 10;
        state.CurrentPlayer.Gems[GemType.Green] = 10;
        state.CurrentPlayer.Gems[GemType.Red] = 10;
        state.CurrentPlayer.Gems[GemType.Black] = 10;

        GameEngine.ApplyAction(state, GameAction.PurchaseCard(0, 0));
        Assert.Equal(1, state.CurrentPlayerIndex);
    }

    [Fact]
    public void Bonus_from_purchased_card_is_active_immediately()
    {
        var state = CreateGame();
        var card = MakeCard(1, GemType.Blue, 0, new GemCollection { [GemType.White] = 1 });
        state.TierMarket[0] = new List<Card> { card };
        state.CurrentPlayer.Gems[GemType.White] = 1;

        GameEngine.ApplyAction(state, GameAction.PurchaseCard(0, 0));

        Assert.Equal(1, state.Players[0].Bonuses[GemType.Blue]);
    }
}
