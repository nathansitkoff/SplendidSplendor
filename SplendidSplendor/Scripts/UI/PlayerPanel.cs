using Godot;
using SplendidSplendor.Model;

namespace SplendidSplendor.UI;

public partial class PlayerPanel : PanelContainer
{
    private PlayerState? _player;
    private int _playerIndex;
    private bool _isCurrentPlayer;

    public void SetPlayer(PlayerState player, int index, bool isCurrent)
    {
        _player = player;
        _playerIndex = index;
        _isCurrentPlayer = isCurrent;
        QueueRedraw();
    }

    public override Vector2 _GetMinimumSize() => new(260, 200);

    public override void _Draw()
    {
        var rect = new Rect2(Vector2.Zero, Size);

        // Background
        var bgColor = _isCurrentPlayer
            ? new Color(0.15f, 0.2f, 0.3f)
            : new Color(0.1f, 0.1f, 0.12f);
        DrawRect(rect, bgColor);

        if (_player == null) return;

        // Player name and score
        var nameColor = _isCurrentPlayer ? Colors.Yellow : Colors.White;
        DrawString(ThemeDB.FallbackFont, new Vector2(14, 28),
            $"Player {_playerIndex + 1}", HorizontalAlignment.Left, -1, 22, nameColor);
        DrawString(ThemeDB.FallbackFont, new Vector2(Size.X - 14, 28),
            $"Score: {_player.Score}", HorizontalAlignment.Right, (int)Size.X - 28, 18, Colors.White);

        // Current player indicator
        if (_isCurrentPlayer)
        {
            DrawRect(new Rect2(0, 0, 4, Size.Y), Colors.Yellow);
        }

        // Gems held
        float y = 46;
        DrawString(ThemeDB.FallbackFont, new Vector2(14, y + 16),
            "Gems:", HorizontalAlignment.Left, -1, 15, Colors.Gray);
        y += 22;
        float x = 14;
        var gemTypes = new[] { GemType.White, GemType.Blue, GemType.Green, GemType.Red, GemType.Black, GemType.Gold };
        foreach (var type in gemTypes)
        {
            int count = _player.Gems[type];
            if (count <= 0) continue;
            var gemColor = GemColors.GetColor(type);
            DrawCircle(new Vector2(x + 12, y + 10), 11, gemColor);
            var textColor = GemColors.GetTextColor(type);
            DrawString(ThemeDB.FallbackFont, new Vector2(x + 7, y + 15),
                count.ToString(), HorizontalAlignment.Left, -1, 14, textColor);
            x += 30;
        }

        // Bonuses from owned cards
        y += 32;
        DrawString(ThemeDB.FallbackFont, new Vector2(14, y + 16),
            "Bonuses:", HorizontalAlignment.Left, -1, 15, Colors.Gray);
        y += 22;
        x = 14;
        var bonuses = _player.Bonuses;
        var nonGoldTypes = new[] { GemType.White, GemType.Blue, GemType.Green, GemType.Red, GemType.Black };
        foreach (var type in nonGoldTypes)
        {
            int count = bonuses[type];
            if (count <= 0) continue;
            var gemColor = GemColors.GetColor(type);
            DrawCircle(new Vector2(x + 12, y + 10), 11, gemColor);
            var textColor = GemColors.GetTextColor(type);
            DrawString(ThemeDB.FallbackFont, new Vector2(x + 7, y + 15),
                count.ToString(), HorizontalAlignment.Left, -1, 14, textColor);
            x += 30;
        }

        // Reserved count
        y += 32;
        DrawString(ThemeDB.FallbackFont, new Vector2(14, y + 16),
            $"Reserved: {_player.ReservedCards.Count}/3", HorizontalAlignment.Left, -1, 15, Colors.Gray);

        // Nobles
        if (_player.Nobles.Count > 0)
        {
            y += 24;
            DrawString(ThemeDB.FallbackFont, new Vector2(14, y + 16),
                $"Nobles: {_player.Nobles.Count} (={_player.Nobles.Count * 3}pts)",
                HorizontalAlignment.Left, -1, 15, new Color(0.7f, 0.5f, 0.9f));
        }

        // Border
        var borderColor = _isCurrentPlayer ? Colors.Yellow : new Color(0.3f, 0.3f, 0.3f);
        DrawRect(rect, borderColor, false, _isCurrentPlayer ? 2 : 1);
    }
}
