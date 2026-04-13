using Xunit;
using SplendidSplendor.Model;
using SplendidSplendor.Logic;

namespace SplendidSplendor.Tests;

public class AiPlayerTests
{
    private Card MakeCard(GemType bonus, int points, GemCollection cost, int tier = 1)
        => new() { Tier = tier, BonusType = bonus, Points = points, Cost = cost };

    private GemCollection Cost(int w = 0, int b = 0, int g = 0, int r = 0, int k = 0)
        => new()
        {
            [GemType.White] = w,
            [GemType.Blue] = b,
            [GemType.Green] = g,
            [GemType.Red] = r,
            [GemType.Black] = k
        };

    // === Rule 1: Win the game ===

    [Fact]
    public void Rule1_buys_card_that_wins_game()
    {
        var state = GameEngine.SetupGame(2);
        var player = state.CurrentPlayer;
        // Player has 13 points from existing cards
        player.OwnedCards.Add(MakeCard(GemType.Blue, 13, Cost(w: 1)));
        // Affordable 2-point card puts them at 15
        var winCard = MakeCard(GemType.Red, 2, Cost(w: 2));
        state.TierMarket[0] = new List<Card> { winCard };
        player.Gems[GemType.White] = 2;

        var action = AiPlayer.ChooseAction(state);

        Assert.IsType<GameAction.PurchaseCardAction>(action);
        var purchase = (GameAction.PurchaseCardAction)action;
        Assert.Equal(0, purchase.Tier);
        Assert.Equal(0, purchase.MarketIndex);
    }

    [Fact]
    public void Rule1_wins_via_noble_bonus()
    {
        var state = GameEngine.SetupGame(2);
        var player = state.CurrentPlayer;
        // Player has 12 points and 2 blue bonuses
        player.OwnedCards.Add(MakeCard(GemType.Blue, 12, Cost(w: 1)));
        player.OwnedCards.Add(MakeCard(GemType.Blue, 0, Cost(w: 1)));
        // Noble requires 3 blue
        state.Nobles.Clear();
        state.Nobles.Add(new Noble { Requirements = Cost(b: 3), Points = 3 });
        // Buy a blue card: gives 1 point + noble (3) = +4 → 16 total
        var winCard = MakeCard(GemType.Blue, 1, Cost(w: 1));
        state.TierMarket[0] = new List<Card> { winCard };
        player.Gems[GemType.White] = 1;

        var action = AiPlayer.ChooseAction(state);

        Assert.IsType<GameAction.PurchaseCardAction>(action);
    }

    // === Rule 2: Claim a noble this turn ===

    [Fact]
    public void Rule2_buys_card_that_claims_noble()
    {
        var state = GameEngine.SetupGame(2);
        var player = state.CurrentPlayer;
        // Player has 2 blue bonuses
        player.OwnedCards.Add(MakeCard(GemType.Blue, 0, Cost(w: 1)));
        player.OwnedCards.Add(MakeCard(GemType.Blue, 0, Cost(w: 1)));
        // Noble requires 3 blue
        state.Nobles.Clear();
        state.Nobles.Add(new Noble { Requirements = Cost(b: 3), Points = 3 });
        // Two affordable cards: blue (claims noble) and red (more points but no noble)
        var blueCard = MakeCard(GemType.Blue, 0, Cost(w: 1));
        var redCard = MakeCard(GemType.Red, 2, Cost(w: 1));
        state.TierMarket[0] = new List<Card> { redCard, blueCard };
        player.Gems[GemType.White] = 2;

        var action = AiPlayer.ChooseAction(state);

        Assert.IsType<GameAction.PurchaseCardAction>(action);
        var purchase = (GameAction.PurchaseCardAction)action;
        // Should buy the blue card (index 1) to claim the noble
        Assert.Equal(1, purchase.MarketIndex);
    }

    // === Rule 3: Buy the best point card ===

    [Fact]
    public void Rule3_buys_highest_point_affordable_card()
    {
        var state = GameEngine.SetupGame(2);
        var player = state.CurrentPlayer;
        // No nobles close
        state.Nobles.Clear();
        // Two affordable cards: 1 point and 3 points
        var lowPts = MakeCard(GemType.Red, 1, Cost(w: 1));
        var highPts = MakeCard(GemType.Green, 3, Cost(w: 2));
        state.TierMarket[0] = new List<Card> { lowPts, highPts };
        player.Gems[GemType.White] = 2;

        var action = AiPlayer.ChooseAction(state);

        Assert.IsType<GameAction.PurchaseCardAction>(action);
        var purchase = (GameAction.PurchaseCardAction)action;
        Assert.Equal(1, purchase.MarketIndex); // highPts card
    }

    // === Rule 4: Buy from reserved ===

    [Fact]
    public void Rule4_buys_reserved_card_with_points()
    {
        var state = GameEngine.SetupGame(2);
        var player = state.CurrentPlayer;
        state.Nobles.Clear();
        // Reserved card with points, affordable
        var reserved = MakeCard(GemType.Blue, 2, Cost(w: 1));
        player.ReservedCards.Add(reserved);
        // No affordable market cards with points
        state.TierMarket[0] = new List<Card>
        {
            MakeCard(GemType.Red, 0, Cost(w: 1)) // affordable but 0 points
        };
        player.Gems[GemType.White] = 1;

        var action = AiPlayer.ChooseAction(state);

        // Should prefer the reserved 2-point card over the 0-point market card
        Assert.IsType<GameAction.PurchaseReservedAction>(action);
    }

    // === Rule 5/7: Take gems toward target card ===

    [Fact]
    public void Rule7_takes_gems_toward_target_card()
    {
        var state = GameEngine.SetupGame(2);
        var player = state.CurrentPlayer;
        state.Nobles.Clear();
        // One unaffordable target card costing white, blue, green
        var target = MakeCard(GemType.Red, 3, Cost(w: 2, b: 2, g: 2));
        state.TierMarket[0] = new List<Card> { target };
        state.TierMarket[1] = new List<Card>();
        state.TierMarket[2] = new List<Card>();

        var action = AiPlayer.ChooseAction(state);

        Assert.IsType<GameAction.TakeThreeGemsAction>(action);
        var take = (GameAction.TakeThreeGemsAction)action;
        // Should take the 3 colors the target card needs
        Assert.Contains(GemType.White, take.Colors);
        Assert.Contains(GemType.Blue, take.Colors);
        Assert.Contains(GemType.Green, take.Colors);
    }

    // === Rule 6: Reserve threatening card ===

    [Fact]
    public void Rule6_reserves_card_opponent_can_use_to_win()
    {
        var state = GameEngine.SetupGame(2);
        // Opponent (player 1) has 13 points
        state.Players[1].OwnedCards.Add(MakeCard(GemType.Blue, 13, Cost(w: 1)));
        // Give opponent enough gems to afford a specific card next turn
        state.Players[1].Gems[GemType.White] = 3;
        // Target card: 2 points, affordable by opponent with their gems
        var threatCard = MakeCard(GemType.Red, 2, Cost(w: 3));
        state.TierMarket[0] = new List<Card> { threatCard };

        // AI (player 0) can't afford it and has no point cards to buy
        state.Nobles.Clear();
        var player = state.CurrentPlayer;
        player.Gems[GemType.Blue] = 0;

        var action = AiPlayer.ChooseAction(state);

        // Should reserve the threat card
        Assert.IsType<GameAction.ReserveCardAction>(action);
        var reserve = (GameAction.ReserveCardAction)action;
        Assert.Equal(0, reserve.Tier);
        Assert.Equal(0, reserve.MarketIndex);
    }

    // === Rule 8: Take 2 same color ===

    [Fact]
    public void Rule8_takes_2_same_when_only_one_color_needed()
    {
        var state = GameEngine.SetupGame(2);
        var player = state.CurrentPlayer;
        state.Nobles.Clear();
        // Target card needs only 2 white
        var target = MakeCard(GemType.Red, 3, Cost(w: 2));
        state.TierMarket[0] = new List<Card> { target };
        state.TierMarket[1] = new List<Card>();
        state.TierMarket[2] = new List<Card>();
        // White has 4+ in bank (enables take-2)
        state.Bank[GemType.White] = 4;

        var action = AiPlayer.ChooseAction(state);

        // Should take 2 white
        if (action is GameAction.TakeTwoGemsAction t2)
        {
            Assert.Equal(GemType.White, t2.Color);
        }
        else
        {
            // Acceptable fallback: take 3 including white
            Assert.IsType<GameAction.TakeThreeGemsAction>(action);
        }
    }

    // === Rule 9: Fallback take 3 gems ===

    [Fact]
    public void Rule9_fallback_takes_3_gems_when_no_target()
    {
        var state = GameEngine.SetupGame(2);
        var player = state.CurrentPlayer;
        state.Nobles.Clear();
        // Empty market — no target possible
        state.TierMarket[0] = new List<Card>();
        state.TierMarket[1] = new List<Card>();
        state.TierMarket[2] = new List<Card>();

        var action = AiPlayer.ChooseAction(state);

        Assert.IsType<GameAction.TakeThreeGemsAction>(action);
    }

    // === Discard handling ===

    [Fact]
    public void Discard_brings_total_to_10()
    {
        var state = GameEngine.SetupGame(2);
        var player = state.CurrentPlayer;
        player.Gems[GemType.White] = 4;
        player.Gems[GemType.Blue] = 4;
        player.Gems[GemType.Red] = 4; // total 12
        state.NeedsDiscard = true;

        var action = AiPlayer.ChooseAction(state);

        Assert.IsType<GameAction.DiscardGemsAction>(action);
        var discard = (GameAction.DiscardGemsAction)action;
        Assert.Equal(2, discard.Gems.Total);
    }

    // === Action is always valid ===

    [Theory]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    public void AI_action_is_always_valid_on_fresh_game(int playerCount)
    {
        var state = GameEngine.SetupGame(playerCount);
        var action = AiPlayer.ChooseAction(state);
        Assert.True(ActionValidator.IsValid(state, action),
            $"AI chose invalid action: {action.GetType().Name}");
    }

    [Fact]
    public void AI_can_play_a_full_turn_without_throwing()
    {
        var state = GameEngine.SetupGame(2);
        var action = AiPlayer.ChooseAction(state);
        // Should not throw
        GameEngine.ApplyAction(state, action);
    }
}
