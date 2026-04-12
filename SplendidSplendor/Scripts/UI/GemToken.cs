using Godot;
using SplendidSplendor.Model;

namespace SplendidSplendor.UI;

public partial class GemToken : Control
{
    private GemType _type;
    private int _count;

    public void SetGem(GemType type, int count)
    {
        _type = type;
        _count = count;
        QueueRedraw();
    }

    public override void _Draw()
    {
        var color = GemColors.GetColor(_type);
        var textColor = GemColors.GetTextColor(_type);
        var label = GemColors.GetLabel(_type);

        var center = new Vector2(Size.Y / 2, Size.Y / 2);
        DrawCircle(center, Size.Y / 2 - 2, color);

        DrawString(ThemeDB.FallbackFont, new Vector2(center.X - 5, center.Y + 6),
            label, HorizontalAlignment.Left, -1, 16, textColor);

        DrawString(ThemeDB.FallbackFont, new Vector2(Size.Y + 6, center.Y + 7),
            $"x{_count}", HorizontalAlignment.Left, -1, 18, Colors.White);
    }
}
