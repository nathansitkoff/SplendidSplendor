namespace SplendidSplendor.Model;

public class PlayerState
{
    public GemCollection Gems { get; set; } = new();
    public List<Card> OwnedCards { get; set; } = new();
    public List<Card> ReservedCards { get; set; } = new();

    public GemCollection Bonuses
    {
        get
        {
            var bonuses = new GemCollection();
            foreach (var card in OwnedCards)
            {
                bonuses[card.BonusType]++;
            }
            return bonuses;
        }
    }

    public int Score => OwnedCards.Sum(c => c.Points) + Nobles.Sum(n => n.Points);

    public List<Noble> Nobles { get; set; } = new();
}
