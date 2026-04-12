using Xunit;
using SplendidSplendor.Model;
using SplendidSplendor.Logic;

namespace SplendidSplendor.Tests;

public class NoblesAndVictoryTests
{
    private Card MakeCard(GemType bonus, int points = 0)
        => new() { Tier = 1, BonusType = bonus, Points = points, Cost = new GemCollection { [GemType.White] = 1 } };

    // === Noble visits ===

    [Fact]
    public void Noble_visits_when_player_has_required_bonuses()
    {
        var state = GameEngine.SetupGame(2);
        // Noble requires 3 blue, 3 green
        state.Nobles.Clear();
        state.Nobles.Add(new Noble
        {
            Requirements = new GemCollection { [GemType.Blue] = 3, [GemType.Green] = 3 },
            Points = 3
        });

        var player = state.CurrentPlayer;
        // Give player 3 blue + 3 green bonuses
        for (int i = 0; i < 3; i++)
        {
            player.OwnedCards.Add(MakeCard(GemType.Blue));
            player.OwnedCards.Add(MakeCard(GemType.Green));
        }
        // Give player a gem to take an action (buy a cheap card)
        player.Gems[GemType.White] = 1;
        var cheapCard = MakeCard(GemType.Red);
        state.TierMarket[0] = new List<Card> { cheapCard };

        GameEngine.ApplyAction(state, GameAction.PurchaseCard(0, 0));

        Assert.Single(state.Players[0].Nobles);
        Assert.Equal(3, state.Players[0].Nobles[0].Points);
        Assert.Empty(state.Nobles); // noble removed from board
    }

    [Fact]
    public void Noble_does_not_visit_without_enough_bonuses()
    {
        var state = GameEngine.SetupGame(2);
        state.Nobles.Clear();
        state.Nobles.Add(new Noble
        {
            Requirements = new GemCollection { [GemType.Blue] = 4 },
            Points = 3
        });

        var player = state.CurrentPlayer;
        // Only 2 blue bonuses — not enough
        player.OwnedCards.Add(MakeCard(GemType.Blue));
        player.OwnedCards.Add(MakeCard(GemType.Blue));
        player.Gems[GemType.White] = 1;
        state.TierMarket[0] = new List<Card> { MakeCard(GemType.Red) };

        GameEngine.ApplyAction(state, GameAction.PurchaseCard(0, 0));

        Assert.Empty(state.Players[0].Nobles);
        Assert.Single(state.Nobles); // noble stays on board
    }

    [Fact]
    public void Only_one_noble_per_turn_even_if_multiple_qualify()
    {
        var state = GameEngine.SetupGame(2);
        state.Nobles.Clear();
        // Two nobles, both requiring 3 blue
        state.Nobles.Add(new Noble
        {
            Requirements = new GemCollection { [GemType.Blue] = 3 },
            Points = 3
        });
        state.Nobles.Add(new Noble
        {
            Requirements = new GemCollection { [GemType.Blue] = 3 },
            Points = 3
        });

        var player = state.CurrentPlayer;
        for (int i = 0; i < 3; i++)
            player.OwnedCards.Add(MakeCard(GemType.Blue));
        player.Gems[GemType.White] = 1;
        state.TierMarket[0] = new List<Card> { MakeCard(GemType.Red) };

        GameEngine.ApplyAction(state, GameAction.PurchaseCard(0, 0));

        Assert.Single(state.Players[0].Nobles); // only 1
        Assert.Single(state.Nobles); // 1 remains on board
    }

    [Fact]
    public void Noble_points_count_toward_score()
    {
        var state = GameEngine.SetupGame(2);
        var player = state.Players[0];
        player.Nobles.Add(new Noble { Requirements = new GemCollection(), Points = 3 });
        Assert.Equal(3, player.Score);
    }

    // === Victory ===

    [Fact]
    public void Game_end_triggers_at_15_points()
    {
        var state = GameEngine.SetupGame(2);
        var player = state.CurrentPlayer;
        // Give player cards worth 14 points
        player.OwnedCards.Add(MakeCard(GemType.Blue, 14));
        // Buy a 1-point card to reach 15
        var card = new Card
        {
            Tier = 1, BonusType = GemType.Red, Points = 1,
            Cost = new GemCollection { [GemType.White] = 1 }
        };
        state.TierMarket[0] = new List<Card> { card };
        player.Gems[GemType.White] = 1;

        GameEngine.ApplyAction(state, GameAction.PurchaseCard(0, 0));

        Assert.True(state.GameEndTriggered);
    }

    [Fact]
    public void Game_does_not_end_below_15_points()
    {
        var state = GameEngine.SetupGame(2);
        var player = state.CurrentPlayer;
        player.OwnedCards.Add(MakeCard(GemType.Blue, 13));
        var card = new Card
        {
            Tier = 1, BonusType = GemType.Red, Points = 1,
            Cost = new GemCollection { [GemType.White] = 1 }
        };
        state.TierMarket[0] = new List<Card> { card };
        player.Gems[GemType.White] = 1;

        GameEngine.ApplyAction(state, GameAction.PurchaseCard(0, 0));

        Assert.False(state.GameEndTriggered);
    }

    [Fact]
    public void Game_continues_after_trigger_until_round_completes()
    {
        var state = GameEngine.SetupGame(2);
        // Player 0 triggers end
        state.CurrentPlayer.OwnedCards.Add(MakeCard(GemType.Blue, 14));
        var card = new Card
        {
            Tier = 1, BonusType = GemType.Red, Points = 1,
            Cost = new GemCollection { [GemType.White] = 1 }
        };
        state.TierMarket[0] = new List<Card> { card };
        state.CurrentPlayer.Gems[GemType.White] = 1;

        GameEngine.ApplyAction(state, GameAction.PurchaseCard(0, 0));

        Assert.True(state.GameEndTriggered);
        Assert.False(state.GameOver); // Player 1 still gets a turn
        Assert.Equal(1, state.CurrentPlayerIndex);

        // Player 1 takes a turn
        GameEngine.ApplyAction(state, GameAction.TakeThreeGems(GemType.White, GemType.Blue, GemType.Green));

        Assert.True(state.GameOver); // Now the round is complete
    }

    [Fact]
    public void All_players_get_equal_turns()
    {
        // 3 players, player 1 (index 1) triggers at 15
        var state = GameEngine.SetupGame(3);

        // Player 0 takes a turn
        GameEngine.ApplyAction(state, GameAction.TakeThreeGems(GemType.White, GemType.Blue, GemType.Green));

        // Player 1 triggers game end
        Assert.Equal(1, state.CurrentPlayerIndex);
        state.CurrentPlayer.OwnedCards.Add(MakeCard(GemType.Blue, 14));
        var card = new Card
        {
            Tier = 1, BonusType = GemType.Red, Points = 1,
            Cost = new GemCollection { [GemType.White] = 1 }
        };
        state.TierMarket[0] = new List<Card> { card };
        state.CurrentPlayer.Gems[GemType.White] = 1;
        GameEngine.ApplyAction(state, GameAction.PurchaseCard(0, 0));

        Assert.True(state.GameEndTriggered);
        Assert.False(state.GameOver); // Player 2 still needs a turn

        // Player 2 takes a turn
        GameEngine.ApplyAction(state, GameAction.TakeThreeGems(GemType.Red, GemType.Black, GemType.White));

        Assert.True(state.GameOver); // All players had equal turns
    }

    // === Tiebreaker ===

    [Fact]
    public void Winner_is_player_with_highest_score()
    {
        var state = GameEngine.SetupGame(2);
        state.Players[0].OwnedCards.Add(MakeCard(GemType.Blue, 15));
        state.Players[1].OwnedCards.Add(MakeCard(GemType.Red, 10));
        state.GameEndTriggered = true;
        state.GameOver = true;

        var winner = GameEngine.GetWinner(state);
        Assert.Equal(0, winner);
    }

    [Fact]
    public void Tiebreaker_fewest_cards_wins()
    {
        var state = GameEngine.SetupGame(2);
        // Both at 15 points
        state.Players[0].OwnedCards.Add(MakeCard(GemType.Blue, 5));
        state.Players[0].OwnedCards.Add(MakeCard(GemType.Blue, 5));
        state.Players[0].OwnedCards.Add(MakeCard(GemType.Blue, 5));
        // Player 1: fewer cards
        state.Players[1].OwnedCards.Add(MakeCard(GemType.Red, 15));
        state.GameEndTriggered = true;
        state.GameOver = true;

        var winner = GameEngine.GetWinner(state);
        Assert.Equal(1, winner); // Player 1 wins with fewer cards
    }
}
