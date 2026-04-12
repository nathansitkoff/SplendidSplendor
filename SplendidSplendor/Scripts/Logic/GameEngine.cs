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

    private static void Shuffle<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = _rng.Next(i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
}
