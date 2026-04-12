using SplendidSplendor.Model;

namespace SplendidSplendor.Logic;

public static class GameEngine
{
    private static readonly Random _rng = new();

    public static GameState SetupGame(int playerCount)
    {
        if (playerCount < 2 || playerCount > 4)
            throw new ArgumentException("Player count must be 2-4", nameof(playerCount));

        var db = CardDatabase.Load();
        var state = new GameState();

        // Initialize players
        for (int i = 0; i < playerCount; i++)
            state.Players.Add(new PlayerState());

        // Initialize bank
        int gemsPerColor = playerCount switch
        {
            2 => 4,
            3 => 5,
            4 => 7,
            _ => throw new ArgumentException()
        };
        state.Bank[GemType.White] = gemsPerColor;
        state.Bank[GemType.Blue] = gemsPerColor;
        state.Bank[GemType.Green] = gemsPerColor;
        state.Bank[GemType.Red] = gemsPerColor;
        state.Bank[GemType.Black] = gemsPerColor;
        state.Bank[GemType.Gold] = 5;

        // Shuffle and deal cards into tiers
        var tierCards = new[] {
            db.AllCards.Where(c => c.Tier == 1).ToList(),
            db.AllCards.Where(c => c.Tier == 2).ToList(),
            db.AllCards.Where(c => c.Tier == 3).ToList()
        };

        for (int tier = 0; tier < 3; tier++)
        {
            Shuffle(tierCards[tier]);
            // Deal 4 to market, rest stay in deck
            state.TierMarket[tier] = tierCards[tier].Take(4).ToList();
            state.TierDecks[tier] = tierCards[tier].Skip(4).ToList();
        }

        // Select nobles (playerCount + 1)
        var allNobles = new List<Noble>(db.AllNobles);
        Shuffle(allNobles);
        state.Nobles = allNobles.Take(playerCount + 1).ToList();

        state.CurrentPlayerIndex = 0;
        state.GameEndTriggered = false;

        return state;
    }

    public static void ApplyAction(GameState state, GameAction action)
    {
        if (!ActionValidator.IsValid(state, action))
            throw new InvalidOperationException("Invalid action");

        switch (action)
        {
            case GameAction.DiscardGemsAction a:
                ApplyDiscard(state, a);
                state.NeedsDiscard = false;
                AdvanceTurn(state);
                return;
            case GameAction.TakeThreeGemsAction a:
                ApplyTakeThreeGems(state, a);
                break;
            case GameAction.TakeTwoGemsAction a:
                ApplyTakeTwoGems(state, a);
                break;
            case GameAction.PurchaseCardAction a:
                ApplyPurchaseCard(state, a);
                break;
            case GameAction.ReserveCardAction a:
                ApplyReserveCard(state, a);
                break;
            case GameAction.PurchaseReservedAction a:
                ApplyPurchaseReserved(state, a);
                break;
        }

        // Check if player needs to discard (>10 tokens)
        if (state.CurrentPlayer.Gems.Total > 10)
        {
            state.NeedsDiscard = true;
            return; // Don't advance turn yet
        }

        AdvanceTurn(state);
    }

    private static void ApplyTakeThreeGems(GameState state, GameAction.TakeThreeGemsAction action)
    {
        var player = state.CurrentPlayer;
        foreach (var color in action.Colors)
        {
            state.Bank[color]--;
            player.Gems[color]++;
        }
    }

    private static void ApplyTakeTwoGems(GameState state, GameAction.TakeTwoGemsAction action)
    {
        var player = state.CurrentPlayer;
        state.Bank[action.Color] -= 2;
        player.Gems[action.Color] += 2;
    }

    private static void ApplyPurchaseCard(GameState state, GameAction.PurchaseCardAction action)
    {
        var player = state.CurrentPlayer;
        var card = state.TierMarket[action.Tier][action.MarketIndex];
        var bonuses = player.Bonuses;
        var gemTypes = new[] { GemType.White, GemType.Blue, GemType.Green, GemType.Red, GemType.Black };

        // Pay for the card
        int goldSpent = 0;
        foreach (var type in gemTypes)
        {
            int cost = card.Cost[type];
            int discount = bonuses[type];
            int effectiveCost = Math.Max(0, cost - discount);
            int gemsToSpend = Math.Min(effectiveCost, player.Gems[type]);
            int remainder = effectiveCost - gemsToSpend;

            player.Gems[type] -= gemsToSpend;
            state.Bank[type] += gemsToSpend;
            goldSpent += remainder;
        }

        player.Gems[GemType.Gold] -= goldSpent;
        state.Bank[GemType.Gold] += goldSpent;

        // Move card to player's tableau
        player.OwnedCards.Add(card);

        // Remove from market and refill
        state.TierMarket[action.Tier].RemoveAt(action.MarketIndex);
        if (state.TierDecks[action.Tier].Count > 0)
        {
            var replacement = state.TierDecks[action.Tier][0];
            state.TierDecks[action.Tier].RemoveAt(0);
            state.TierMarket[action.Tier].Add(replacement);
        }
    }

    private static void ApplyReserveCard(GameState state, GameAction.ReserveCardAction action)
    {
        var player = state.CurrentPlayer;
        Card card;

        if (action.MarketIndex == null)
        {
            // Take from deck top
            card = state.TierDecks[action.Tier][0];
            state.TierDecks[action.Tier].RemoveAt(0);
        }
        else
        {
            // Take from market and refill
            card = state.TierMarket[action.Tier][action.MarketIndex.Value];
            state.TierMarket[action.Tier].RemoveAt(action.MarketIndex.Value);
            if (state.TierDecks[action.Tier].Count > 0)
            {
                var replacement = state.TierDecks[action.Tier][0];
                state.TierDecks[action.Tier].RemoveAt(0);
                state.TierMarket[action.Tier].Add(replacement);
            }
        }

        player.ReservedCards.Add(card);

        // Give gold if available
        if (state.Bank[GemType.Gold] > 0)
        {
            state.Bank[GemType.Gold]--;
            player.Gems[GemType.Gold]++;
        }
    }

    private static void ApplyPurchaseReserved(GameState state, GameAction.PurchaseReservedAction action)
    {
        var player = state.CurrentPlayer;
        var card = player.ReservedCards[action.ReserveIndex];
        var bonuses = player.Bonuses;
        var gemTypes = new[] { GemType.White, GemType.Blue, GemType.Green, GemType.Red, GemType.Black };

        // Pay for the card (same logic as market purchase)
        int goldSpent = 0;
        foreach (var type in gemTypes)
        {
            int cost = card.Cost[type];
            int discount = bonuses[type];
            int effectiveCost = Math.Max(0, cost - discount);
            int gemsToSpend = Math.Min(effectiveCost, player.Gems[type]);
            int remainder = effectiveCost - gemsToSpend;

            player.Gems[type] -= gemsToSpend;
            state.Bank[type] += gemsToSpend;
            goldSpent += remainder;
        }

        player.Gems[GemType.Gold] -= goldSpent;
        state.Bank[GemType.Gold] += goldSpent;

        // Move card from reserved to owned
        player.ReservedCards.RemoveAt(action.ReserveIndex);
        player.OwnedCards.Add(card);
    }

    private static void ApplyDiscard(GameState state, GameAction.DiscardGemsAction action)
    {
        var player = state.CurrentPlayer;
        var gemTypes = new[] { GemType.White, GemType.Blue, GemType.Green, GemType.Red, GemType.Black, GemType.Gold };
        foreach (var type in gemTypes)
        {
            int amount = action.Gems[type];
            if (amount > 0)
            {
                player.Gems[type] -= amount;
                state.Bank[type] += amount;
            }
        }
    }

    private static void CheckNobleVisit(GameState state)
    {
        var player = state.CurrentPlayer;
        var bonuses = player.Bonuses;
        var gemTypes = new[] { GemType.White, GemType.Blue, GemType.Green, GemType.Red, GemType.Black };

        for (int i = 0; i < state.Nobles.Count; i++)
        {
            var noble = state.Nobles[i];
            bool qualifies = true;
            foreach (var type in gemTypes)
            {
                if (bonuses[type] < noble.Requirements[type])
                {
                    qualifies = false;
                    break;
                }
            }
            if (qualifies)
            {
                player.Nobles.Add(noble);
                state.Nobles.RemoveAt(i);
                return; // Only 1 noble per turn
            }
        }
    }

    private static void CheckGameEnd(GameState state)
    {
        if (state.CurrentPlayer.Score >= 15)
        {
            state.GameEndTriggered = true;
            // Remember who triggered — the round ends when it would be
            // this player's turn again (all others get one more turn)
            state.FinalRound = state.CurrentPlayerIndex;
        }
    }

    private static void AdvanceTurn(GameState state)
    {
        // Check noble visit before advancing
        CheckNobleVisit(state);

        // Check if this player triggered game end
        if (!state.GameEndTriggered)
            CheckGameEnd(state);

        // Advance to next player
        int nextPlayer = (state.CurrentPlayerIndex + 1) % state.Players.Count;
        state.CurrentPlayerIndex = nextPlayer;

        // Game is over when we wrap back to the player who started the
        // final round (player 0 in a normal game, or whoever's turn it
        // was when 15 was first reached). Everyone after the trigger
        // player gets exactly one more turn.
        if (state.GameEndTriggered && nextPlayer == 0)
        {
            state.GameOver = true;
        }
    }

    public static int GetWinner(GameState state)
    {
        int bestPlayer = 0;
        int bestScore = state.Players[0].Score;
        int fewestCards = state.Players[0].OwnedCards.Count;

        for (int i = 1; i < state.Players.Count; i++)
        {
            int score = state.Players[i].Score;
            int cards = state.Players[i].OwnedCards.Count;
            if (score > bestScore || (score == bestScore && cards < fewestCards))
            {
                bestPlayer = i;
                bestScore = score;
                fewestCards = cards;
            }
        }
        return bestPlayer;
    }

    private static void Shuffle<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = _rng.Next(i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
}
