using Godot;
using SplendidSplendor.Model;

namespace SplendidSplendor.UI;

public partial class DiscardPanel : PanelContainer
{
    private PlayerState _player = null!;
    private readonly Dictionary<GemType, int> _discarding = new();
    private int _excess;

    [Signal]
    public delegate void DiscardConfirmedEventHandler();

    private VBoxContainer _layout = null!;
    private Label _infoLabel = null!;
    private HBoxContainer _gemsRow = null!;
    private Button _confirmBtn = null!;

    public GemCollection GetDiscardSelection()
    {
        var gems = new GemCollection();
        foreach (var (type, count) in _discarding)
            gems[type] = count;
        return gems;
    }

    public void Setup(PlayerState player)
    {
        _player = player;
        _discarding.Clear();
        _excess = player.Gems.Total - 10;

        if (_layout == null)
            BuildLayout();
        UpdateDisplay();
    }

    private void BuildLayout()
    {
        _layout = new VBoxContainer();
        _layout.AddThemeConstantOverride("separation", 8);

        _infoLabel = new Label();
        _infoLabel.AddThemeColorOverride("font_color", Colors.Yellow);
        _infoLabel.AddThemeFontSizeOverride("font_size", 16);
        _layout.AddChild(_infoLabel);

        _gemsRow = new HBoxContainer();
        _gemsRow.AddThemeConstantOverride("separation", 12);
        _layout.AddChild(_gemsRow);

        _confirmBtn = new Button { Text = "Confirm Discard", Disabled = true };
        _confirmBtn.Pressed += () => EmitSignal(SignalName.DiscardConfirmed);
        _layout.AddChild(_confirmBtn);

        // Margin
        var margin = new MarginContainer();
        margin.AddThemeConstantOverride("margin_left", 12);
        margin.AddThemeConstantOverride("margin_right", 12);
        margin.AddThemeConstantOverride("margin_top", 8);
        margin.AddThemeConstantOverride("margin_bottom", 8);
        margin.AddChild(_layout);
        AddChild(margin);
    }

    private void UpdateDisplay()
    {
        int totalDiscarding = _discarding.Values.Sum();
        int remaining = _excess - totalDiscarding;
        _infoLabel.Text = $"You have {_player.Gems.Total} gems — discard {_excess}. Select {remaining} more to discard.";
        if (remaining == 0)
            _infoLabel.Text = $"Discard {_excess} gems. Ready to confirm.";

        _confirmBtn.Disabled = remaining != 0;

        // Rebuild gem buttons
        foreach (var child in _gemsRow.GetChildren())
            child.QueueFree();

        var gemTypes = new[] { GemType.White, GemType.Blue, GemType.Green, GemType.Red, GemType.Black, GemType.Gold };
        foreach (var type in gemTypes)
        {
            int held = _player.Gems[type];
            if (held <= 0) continue;

            int discardCount = _discarding.GetValueOrDefault(type, 0);

            var col = new VBoxContainer();
            col.AddThemeConstantOverride("separation", 4);

            var gem = new GemToken();
            gem.SetGem(type, held, interactive: true, selected: discardCount > 0);
            gem.CustomMinimumSize = new Vector2(72, 40);
            var capturedType = type;
            gem.GemClicked += (_) => OnGemClicked(capturedType);
            col.AddChild(gem);

            if (discardCount > 0)
            {
                var discLabel = new Label { Text = $"-{discardCount}" };
                discLabel.AddThemeColorOverride("font_color", Colors.Red);
                discLabel.AddThemeFontSizeOverride("font_size", 14);
                discLabel.HorizontalAlignment = HorizontalAlignment.Center;
                col.AddChild(discLabel);
            }

            _gemsRow.AddChild(col);
        }
    }

    private void OnGemClicked(GemType type)
    {
        int held = _player.Gems[type];
        int current = _discarding.GetValueOrDefault(type, 0);
        int totalDiscarding = _discarding.Values.Sum();

        if (current < held && totalDiscarding < _excess)
        {
            // Add one more to discard
            _discarding[type] = current + 1;
        }
        else if (current > 0)
        {
            // Remove from discard (toggle back)
            _discarding[type] = current - 1;
            if (_discarding[type] == 0)
                _discarding.Remove(type);
        }

        UpdateDisplay();
    }
}
