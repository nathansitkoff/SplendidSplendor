using Godot;

namespace SplendidSplendor.UI;

public enum PlayerKind
{
    Human,
    AI
}

public partial class MainMenu : Control
{
    [Signal]
    public delegate void StartGameEventHandler(int playerCount);

    private int _playerCount = 2;
    private readonly PlayerKind[] _playerKinds = new PlayerKind[4]
    {
        PlayerKind.Human, PlayerKind.Human, PlayerKind.Human, PlayerKind.Human
    };

    private VBoxContainer _playerList = null!;
    private Button[] _countButtons = null!;

    public override void _Ready()
    {
        BuildLayout();
        UpdatePlayerList();
    }

    private void BuildLayout()
    {
        // Center container
        var center = new CenterContainer();
        center.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        AddChild(center);

        var panel = new PanelContainer();
        panel.AddThemeStyleboxOverride("panel", new StyleBoxFlat
        {
            BgColor = new Color(0.1f, 0.1f, 0.15f),
            BorderWidthLeft = 2, BorderWidthRight = 2,
            BorderWidthTop = 2, BorderWidthBottom = 2,
            BorderColor = new Color(0.9f, 0.75f, 0.1f)
        });
        center.AddChild(panel);

        var margin = new MarginContainer();
        margin.AddThemeConstantOverride("margin_left", 40);
        margin.AddThemeConstantOverride("margin_right", 40);
        margin.AddThemeConstantOverride("margin_top", 30);
        margin.AddThemeConstantOverride("margin_bottom", 30);
        panel.AddChild(margin);

        var content = new VBoxContainer();
        content.AddThemeConstantOverride("separation", 20);
        margin.AddChild(content);

        // Title
        var title = new Label { Text = "SPLENDID SPLENDOR" };
        title.AddThemeColorOverride("font_color", new Color(0.9f, 0.75f, 0.1f));
        title.AddThemeFontSizeOverride("font_size", 36);
        title.HorizontalAlignment = HorizontalAlignment.Center;
        content.AddChild(title);

        var subtitle = new Label { Text = "A digital Splendor" };
        subtitle.AddThemeColorOverride("font_color", Colors.Gray);
        subtitle.AddThemeFontSizeOverride("font_size", 14);
        subtitle.HorizontalAlignment = HorizontalAlignment.Center;
        content.AddChild(subtitle);

        // Spacer
        content.AddChild(new Control { CustomMinimumSize = new Vector2(0, 10) });

        // Number of players selector
        var countLabel = new Label { Text = "Number of players:" };
        countLabel.AddThemeColorOverride("font_color", Colors.White);
        countLabel.AddThemeFontSizeOverride("font_size", 16);
        content.AddChild(countLabel);

        var countRow = new HBoxContainer();
        countRow.AddThemeConstantOverride("separation", 8);
        _countButtons = new Button[3];
        for (int c = 2; c <= 4; c++)
        {
            int count = c;
            var btn = new Button { Text = $"{c} Players", ToggleMode = true };
            btn.CustomMinimumSize = new Vector2(100, 40);
            btn.Pressed += () => OnCountSelected(count);
            _countButtons[c - 2] = btn;
            countRow.AddChild(btn);
        }
        content.AddChild(countRow);

        // Spacer
        content.AddChild(new Control { CustomMinimumSize = new Vector2(0, 10) });

        // Per-player human/AI list
        var playersLabel = new Label { Text = "Players:" };
        playersLabel.AddThemeColorOverride("font_color", Colors.White);
        playersLabel.AddThemeFontSizeOverride("font_size", 16);
        content.AddChild(playersLabel);

        _playerList = new VBoxContainer();
        _playerList.AddThemeConstantOverride("separation", 6);
        content.AddChild(_playerList);

        // Spacer
        content.AddChild(new Control { CustomMinimumSize = new Vector2(0, 20) });

        // Start button
        var startBtn = new Button { Text = "Start Game" };
        startBtn.CustomMinimumSize = new Vector2(0, 50);
        startBtn.AddThemeFontSizeOverride("font_size", 20);
        startBtn.Pressed += () => EmitSignal(SignalName.StartGame, _playerCount);
        content.AddChild(startBtn);

        // Initial selection
        OnCountSelected(2);
    }

    private void OnCountSelected(int count)
    {
        _playerCount = count;
        for (int i = 0; i < 3; i++)
            _countButtons[i].ButtonPressed = (i + 2 == count);
        UpdatePlayerList();
    }

    private void UpdatePlayerList()
    {
        foreach (var child in _playerList.GetChildren())
            child.QueueFree();

        for (int i = 0; i < _playerCount; i++)
        {
            int idx = i;
            var row = new HBoxContainer();
            row.AddThemeConstantOverride("separation", 12);

            var label = new Label { Text = $"Player {i + 1}:" };
            label.AddThemeColorOverride("font_color", Colors.White);
            label.AddThemeFontSizeOverride("font_size", 14);
            label.CustomMinimumSize = new Vector2(80, 0);
            row.AddChild(label);

            var humanBtn = new Button { Text = "Human", ToggleMode = true };
            humanBtn.ButtonPressed = _playerKinds[i] == PlayerKind.Human;
            humanBtn.CustomMinimumSize = new Vector2(80, 32);
            humanBtn.Pressed += () => SetPlayerKind(idx, PlayerKind.Human);
            row.AddChild(humanBtn);

            var aiBtn = new Button { Text = "AI", ToggleMode = true, Disabled = true };
            aiBtn.TooltipText = "AI opponents coming soon";
            aiBtn.ButtonPressed = _playerKinds[i] == PlayerKind.AI;
            aiBtn.CustomMinimumSize = new Vector2(80, 32);
            aiBtn.Pressed += () => SetPlayerKind(idx, PlayerKind.AI);
            row.AddChild(aiBtn);

            _playerList.AddChild(row);
        }
    }

    private void SetPlayerKind(int index, PlayerKind kind)
    {
        _playerKinds[index] = kind;
        UpdatePlayerList();
    }
}
