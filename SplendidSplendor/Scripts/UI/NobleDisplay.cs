using Godot;
using SplendidSplendor.Model;

namespace SplendidSplendor.UI;

public partial class NobleDisplay : PanelContainer
{
    private Noble? _noble;

    public void SetNoble(Noble? noble)
    {
        _noble = noble;
        QueueRedraw();
    }

    public override Vector2 _GetMinimumSize() => new(110, 130);

    public override void _Draw()
    {
        var rect = new Rect2(Vector2.Zero, Size);

        if (_noble == null)
        {
            DrawRect(rect, new Color(0.15f, 0.15f, 0.15f));
            return;
        }

        // Noble background (purple)
        DrawRect(rect, new Color(0.35f, 0.2f, 0.45f));

        // Points
        DrawString(ThemeDB.FallbackFont, new Vector2(12, 28), "3",
            HorizontalAlignment.Left, -1, 24, Colors.White);

        // Requirements
        float y = 40;
        var gemTypes = new[] { GemType.White, GemType.Blue, GemType.Green, GemType.Red, GemType.Black };
        foreach (var type in gemTypes)
        {
            int req = _noble.Requirements[type];
            if (req <= 0) continue;
            var gemColor = GemColors.GetColor(type);
            DrawCircle(new Vector2(24, y + 10), 12, gemColor);
            var textColor = GemColors.GetTextColor(type);
            DrawString(ThemeDB.FallbackFont, new Vector2(19, y + 16),
                req.ToString(), HorizontalAlignment.Left, -1, 16, textColor);
            y += 28;
        }

        // Border
        DrawRect(rect, new Color(0.6f, 0.4f, 0.7f), false, 2);
    }
}
