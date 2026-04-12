using Godot;
using SplendidSplendor.Model;

namespace SplendidSplendor.UI;

public partial class CardDisplay : PanelContainer
{
    private Card? _card;
    private bool _affordable;
    private bool _interactive;

    [Signal]
    public delegate void CardClickedEventHandler(int tier, int marketIndex);

    private int _tier;
    private int _marketIndex;

    public void SetCard(Card? card, bool interactive = false, bool affordable = false, int tier = 0, int marketIndex = 0)
    {
        _card = card;
        _interactive = interactive && card != null;
        _affordable = affordable;
        _tier = tier;
        _marketIndex = marketIndex;
        MouseDefaultCursorShape = _interactive && _affordable
            ? CursorShape.PointingHand
            : CursorShape.Arrow;
        QueueRedraw();
    }

    public override void _GuiInput(InputEvent @event)
    {
        if (!_interactive || !_affordable) return;
        if (@event is InputEventMouseButton mb && mb.Pressed && mb.ButtonIndex == MouseButton.Left)
        {
            EmitSignal(SignalName.CardClicked, _tier, _marketIndex);
            AcceptEvent();
        }
    }

    public override Vector2 _GetMinimumSize() => new(120, 180);

    public override void _Draw()
    {
        var rect = new Rect2(Vector2.Zero, Size);

        if (_card == null)
        {
            DrawRect(rect, new Color(0.15f, 0.15f, 0.15f));
            DrawRect(rect, new Color(0.3f, 0.3f, 0.3f), false, 2);
            return;
        }

        // Card background (light grey)
        DrawRect(rect, new Color(0.7f, 0.7f, 0.72f));

        // Bonus gem stripe at top
        var bonusColor = GemColors.GetColor(_card.BonusType);
        DrawRect(new Rect2(0, 0, Size.X, 34), bonusColor);

        // Bonus letter
        var bonusLabel = GemColors.GetLabel(_card.BonusType);
        var bonusTextColor = GemColors.GetTextColor(_card.BonusType);
        DrawString(ThemeDB.FallbackFont, new Vector2(10, 24), bonusLabel,
            HorizontalAlignment.Left, -1, 20, bonusTextColor);

        // Points (top right)
        if (_card.Points > 0)
        {
            DrawString(ThemeDB.FallbackFont, new Vector2(Size.X - 28, 25),
                _card.Points.ToString(), HorizontalAlignment.Right, -1, 22, bonusTextColor);
        }

        // Cost gems (bottom area)
        float y = Size.Y - 12;
        var gemTypes = new[] { GemType.White, GemType.Blue, GemType.Green, GemType.Red, GemType.Black };
        foreach (var type in gemTypes)
        {
            int cost = _card.Cost[type];
            if (cost <= 0) continue;
            y -= 28;
            var gemColor = GemColors.GetColor(type);
            DrawCircle(new Vector2(22, y + 10), 11, gemColor);
            var textColor = GemColors.GetTextColor(type);
            DrawString(ThemeDB.FallbackFont, new Vector2(17, y + 16),
                cost.ToString(), HorizontalAlignment.Left, -1, 16, textColor);
        }

        // Border — green if affordable, dim if not
        if (_interactive && _affordable)
        {
            DrawRect(rect, new Color(0.2f, 0.9f, 0.3f), false, 3);
        }
        else
        {
            DrawRect(rect, new Color(0.4f, 0.4f, 0.4f), false, 1);
        }
    }
}
