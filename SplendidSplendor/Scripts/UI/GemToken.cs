using Godot;
using SplendidSplendor.Model;

namespace SplendidSplendor.UI;

public partial class GemToken : Control
{
    private GemType _type;
    private int _count;
    private bool _selected;
    private bool _interactive;

    [Signal]
    public delegate void GemClickedEventHandler(int gemType);

    public void SetGem(GemType type, int count, bool interactive = false, bool selected = false)
    {
        _type = type;
        _count = count;
        _interactive = interactive;
        _selected = selected;
        MouseDefaultCursorShape = interactive && count > 0
            ? CursorShape.PointingHand
            : CursorShape.Arrow;
        QueueRedraw();
    }

    public override void _GuiInput(InputEvent @event)
    {
        if (!_interactive || _count <= 0) return;
        if (@event is InputEventMouseButton mb && mb.Pressed && mb.ButtonIndex == MouseButton.Left)
        {
            EmitSignal(SignalName.GemClicked, (int)_type);
            AcceptEvent();
        }
    }

    public override void _Draw()
    {
        var color = GemColors.GetColor(_type);
        var textColor = GemColors.GetTextColor(_type);
        var label = GemColors.GetLabel(_type);

        var center = new Vector2(Size.Y / 2, Size.Y / 2);
        var radius = Size.Y / 2 - 2;

        // Selection highlight
        if (_selected)
        {
            DrawCircle(center, radius + 4, Colors.Orange);
            DrawCircle(center, radius + 2, new Color(0.1f, 0.1f, 0.1f));
        }

        DrawCircle(center, radius, color);

        DrawString(ThemeDB.FallbackFont, new Vector2(center.X - 5, center.Y + 6),
            label, HorizontalAlignment.Left, -1, 16, textColor);

        DrawString(ThemeDB.FallbackFont, new Vector2(Size.Y + 6, center.Y + 7),
            $"x{_count}", HorizontalAlignment.Left, -1, 18, Colors.White);
    }
}
