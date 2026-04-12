namespace SplendidSplendor.Model;

public class Card
{
    public int Tier { get; set; }
    public GemCollection Cost { get; set; } = new();
    public GemType BonusType { get; set; }
    public int Points { get; set; }
}
