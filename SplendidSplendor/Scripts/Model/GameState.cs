namespace SplendidSplendor.Model;

public class GameState
{
    public List<PlayerState> Players { get; set; } = new();
    public int CurrentPlayerIndex { get; set; }
    public GemCollection Bank { get; set; } = new();
    public List<Card>[] TierDecks { get; set; } = { new(), new(), new() };
    public List<Card>[] TierMarket { get; set; } = { new(), new(), new() };
    public List<Noble> Nobles { get; set; } = new();
    public bool NeedsDiscard { get; set; }
    public bool GameEndTriggered { get; set; }
    public int FinalRound { get; set; }

    public PlayerState CurrentPlayer => Players[CurrentPlayerIndex];
}
