namespace SplendidSplendor.Model;

public abstract class GameAction
{
    public static TakeThreeGemsAction TakeThreeGems(params GemType[] colors)
        => new(colors.ToList());

    private GameAction() { }

    public class TakeThreeGemsAction : GameAction
    {
        public List<GemType> Colors { get; }
        public TakeThreeGemsAction(List<GemType> colors) => Colors = colors;
    }
}
