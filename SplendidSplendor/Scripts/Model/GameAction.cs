namespace SplendidSplendor.Model;

public abstract class GameAction
{
    public static TakeThreeGemsAction TakeThreeGems(params GemType[] colors)
        => new(colors.ToList());

    public static TakeTwoGemsAction TakeTwoGems(GemType color)
        => new(color);

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
}
