using Godot;
using SplendidSplendor.Model;

namespace SplendidSplendor.UI;

public partial class NobleDisplay : PanelContainer
{
    private Noble? _noble;
    private bool _compact;

    public void SetNoble(Noble? noble, bool compact = false)
    {
        _noble = noble;
        _compact = compact;
        QueueRedraw();
    }

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

        float pointsX = _compact ? 6 : 12;
        float pointsY = _compact ? 18 : 28;
        int pointsSize = _compact ? 16 : 24;
        DrawString(ThemeDB.FallbackFont, new Vector2(pointsX, pointsY), "3",
            HorizontalAlignment.Left, -1, pointsSize, Colors.White);

        // Requirements
        float y = _compact ? 26 : 40;
        float circleX = _compact ? 14 : 24;
        float circleR = _compact ? 8 : 12;
        float textX = _compact ? 10 : 19;
        int textSize = _compact ? 11 : 16;
        float rowStep = _compact ? 18 : 28;

        var gemTypes = new[] { GemType.White, GemType.Blue, GemType.Green, GemType.Red, GemType.Black };
        foreach (var type in gemTypes)
        {
            int req = _noble.Requirements[type];
            if (req <= 0) continue;
            var gemColor = GemColors.GetColor(type);
            DrawCircle(new Vector2(circleX, y + circleR - 2), circleR, gemColor);
            var textColor = GemColors.GetTextColor(type);
            DrawString(ThemeDB.FallbackFont, new Vector2(textX, y + circleR + 3),
                req.ToString(), HorizontalAlignment.Left, -1, textSize, textColor);
            y += rowStep;
        }

        // Border
        DrawRect(rect, new Color(0.6f, 0.4f, 0.7f), false, _compact ? 1 : 2);
    }
}
