namespace SplendidSplendor.Model;

public abstract class GameAction
{
    public static TakeThreeGemsAction TakeThreeGems(params GemType[] colors)
        => new(colors.ToList());

    public static TakeTwoGemsAction TakeTwoGems(GemType color)
        => new(color);

    public static PurchaseCardAction PurchaseCard(int tier, int marketIndex)
        => new(tier, marketIndex);

    public static ReserveCardAction ReserveCard(int tier, int? marketIndex)
        => new(tier, marketIndex);

    public static PurchaseReservedAction PurchaseReserved(int reserveIndex)
        => new(reserveIndex);

    private GameAction() { }

    public class TakeThreeGemsAction : GameAction
    {
        public List<GemType> Colors { get; }
        public TakeThreeGemsAction(List<GemType> colors) => Colors = colors;
    }

    public class TakeTwoGemsAction : GameAction
    {
        public GemType Color { get; }
        public TakeTwoGemsAction(GemType color) => Color = color;
    }

    public class PurchaseCardAction : GameAction
    {
        public int Tier { get; }
        public int MarketIndex { get; }
        public PurchaseCardAction(int tier, int marketIndex)
        {
            Tier = tier;
            MarketIndex = marketIndex;
        }
    }

    public class ReserveCardAction : GameAction
    {
        public int Tier { get; }
        public int? MarketIndex { get; }
        public ReserveCardAction(int tier, int? marketIndex)
        {
            Tier = tier;
            MarketIndex = marketIndex;
        }
    }

    public class PurchaseReservedAction : GameAction
    {
        public int ReserveIndex { get; }
        public PurchaseReservedAction(int reserveIndex) => ReserveIndex = reserveIndex;
    }
}
