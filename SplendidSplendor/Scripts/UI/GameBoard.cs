using Godot;
using SplendidSplendor.Model;
using SplendidSplendor.Logic;

namespace SplendidSplendor.UI;

public partial class GameBoard : Control
{
    private GameState _state = null!;

    // UI containers
    private HBoxContainer _noblesRow = null!;
    private VBoxContainer _marketArea = null!;
    private GemBank _gemBank = null!;
    private VBoxContainer _playersArea = null!;
    private Label _titleLabel = null!;
    private HBoxContainer _actionBar = null!;
    private Button _confirmButton = null!;
    private Button _cancelButton = null!;
    private Label _actionLabel = null!;

    public override void _Ready()
    {
        _state = GameEngine.SetupGame(2);
        BuildLayout();
        RefreshDisplay();
    }

    private void BuildLayout()
    {
        // Root layout
        var root = new VBoxContainer();
        root.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        root.AddThemeConstantOverride("separation", 4);

        // Title bar
        _titleLabel = new Label { Text = "Splendid Splendor" };
        _titleLabel.AddThemeColorOverride("font_color", Colors.White);
        _titleLabel.AddThemeFontSizeOverride("font_size", 26);
        root.AddChild(_titleLabel);

        // Top row: nobles + gem bank
        var topRow = new HBoxContainer();
        topRow.AddThemeConstantOverride("separation", 24);

        // Nobles
        var noblesSection = new VBoxContainer();
        var noblesLabel = new Label { Text = "NOBLES" };
        noblesLabel.AddThemeColorOverride("font_color", Colors.Gray);
        noblesLabel.AddThemeFontSizeOverride("font_size", 11);
        noblesSection.AddChild(noblesLabel);
        _noblesRow = new HBoxContainer();
        _noblesRow.AddThemeConstantOverride("separation", 6);
        noblesSection.AddChild(_noblesRow);
        topRow.AddChild(noblesSection);

        // Gem bank
        var bankSection = new VBoxContainer();
        var bankLabel = new Label { Text = "GEM BANK (click to select)" };
        bankLabel.AddThemeColorOverride("font_color", Colors.Gray);
        bankLabel.AddThemeFontSizeOverride("font_size", 11);
        bankSection.AddChild(bankLabel);
        _gemBank = new GemBank();
        _gemBank.SelectionChanged += OnGemSelectionChanged;
        bankSection.AddChild(_gemBank);

        // Action bar (confirm/cancel)
        _actionBar = new HBoxContainer();
        _actionBar.AddThemeConstantOverride("separation", 8);
        _actionLabel = new Label { Text = "" };
        _actionLabel.AddThemeColorOverride("font_color", Colors.Yellow);
        _actionLabel.AddThemeFontSizeOverride("font_size", 14);
        _actionBar.AddChild(_actionLabel);
        _confirmButton = new Button { Text = "Confirm", Visible = false };
        _confirmButton.Pressed += OnConfirmPressed;
        _actionBar.AddChild(_confirmButton);
        _cancelButton = new Button { Text = "Cancel", Visible = false };
        _cancelButton.Pressed += OnCancelPressed;
        _actionBar.AddChild(_cancelButton);
        bankSection.AddChild(_actionBar);

        topRow.AddChild(bankSection);
        root.AddChild(topRow);

        // Main area: market on left, players on right
        var mainArea = new HBoxContainer();
        mainArea.SizeFlagsVertical = SizeFlags.ExpandFill;
        mainArea.AddThemeConstantOverride("separation", 16);

        // Card market
        _marketArea = new VBoxContainer();
        _marketArea.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        _marketArea.AddThemeConstantOverride("separation", 2);

        var tierLabels = new[] { "TIER III", "TIER II", "TIER I" };
        for (int i = 0; i < 3; i++)
        {
            var tierSection = new VBoxContainer();
            tierSection.AddThemeConstantOverride("separation", 0);
            var tierLabel = new Label { Text = tierLabels[i] };
            tierLabel.AddThemeColorOverride("font_color", Colors.Gray);
            tierLabel.AddThemeFontSizeOverride("font_size", 10);
            tierSection.AddChild(tierLabel);

            var cardRow = new HBoxContainer();
            cardRow.AddThemeConstantOverride("separation", 4);
            cardRow.Name = $"TierRow{2 - i}";
            tierSection.AddChild(cardRow);
            _marketArea.AddChild(tierSection);
        }
        mainArea.AddChild(_marketArea);

        // Players
        _playersArea = new VBoxContainer();
        _playersArea.AddThemeConstantOverride("separation", 6);
        _playersArea.CustomMinimumSize = new Vector2(280, 0);
        var playersLabel = new Label { Text = "PLAYERS" };
        playersLabel.AddThemeColorOverride("font_color", Colors.Gray);
        playersLabel.AddThemeFontSizeOverride("font_size", 11);
        _playersArea.AddChild(playersLabel);
        mainArea.AddChild(_playersArea);

        root.AddChild(mainArea);

        // Margin wrapper
        var margin = new MarginContainer();
        margin.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        margin.AddThemeConstantOverride("margin_left", 12);
        margin.AddThemeConstantOverride("margin_right", 12);
        margin.AddThemeConstantOverride("margin_top", 8);
        margin.AddThemeConstantOverride("margin_bottom", 8);
        margin.AddChild(root);
        AddChild(margin);
    }

    private void OnGemSelectionChanged()
    {
        var selected = _gemBank.SelectedGems;
        if (selected.Count > 0)
        {
            var action = GameAction.TakeThreeGems(selected.ToArray());
            bool valid = ActionValidator.IsValid(_state, action);
            _actionLabel.Text = $"Take gems: {string.Join(", ", selected.Select(g => GemColors.GetLabel(g)))}";
            _confirmButton.Visible = true;
            _confirmButton.Disabled = !valid;
            _cancelButton.Visible = true;

            if (!valid)
            {
                // Provide hint about why
                int availableColors = 0;
                var gemTypes = new[] { GemType.White, GemType.Blue, GemType.Green, GemType.Red, GemType.Black };
                foreach (var t in gemTypes)
                    if (_state.Bank[t] > 0) availableColors++;
                int needed = Math.Min(3, availableColors);
                if (selected.Count < needed)
                    _actionLabel.Text += $" (select {needed})";
            }
        }
        else
        {
            _actionLabel.Text = "";
            _confirmButton.Visible = false;
            _cancelButton.Visible = false;
        }
    }

    private void OnConfirmPressed()
    {
        var selected = _gemBank.SelectedGems;
        if (selected.Count == 0) return;

        var action = GameAction.TakeThreeGems(selected.ToArray());
        if (!ActionValidator.IsValid(_state, action)) return;

        GameEngine.ApplyAction(_state, action);
        RefreshDisplay();
    }

    private void OnCancelPressed()
    {
        _gemBank.ClearSelection();
        _actionLabel.Text = "";
        _confirmButton.Visible = false;
        _cancelButton.Visible = false;
    }

    private void RefreshDisplay()
    {
        // Update title
        _titleLabel.Text = $"Splendid Splendor — Player {_state.CurrentPlayerIndex + 1}'s Turn";

        // Update nobles
        foreach (var child in _noblesRow.GetChildren())
            child.QueueFree();
        foreach (var noble in _state.Nobles)
        {
            var display = new NobleDisplay();
            display.CustomMinimumSize = new Vector2(110, 130);
            _noblesRow.AddChild(display);
            display.SetNoble(noble);
        }

        // Update market cards
        for (int tier = 0; tier < 3; tier++)
        {
            var rowName = $"TierRow{tier}";
            var row = _marketArea.FindChild(rowName, true, false) as HBoxContainer;
            if (row == null) continue;

            foreach (var child in row.GetChildren())
                child.QueueFree();

            // Deck indicator
            var deckCount = new Label
            {
                Text = $"[{_state.TierDecks[tier].Count}]",
                CustomMinimumSize = new Vector2(36, 180),
                VerticalAlignment = VerticalAlignment.Center
            };
            deckCount.AddThemeColorOverride("font_color", Colors.Gray);
            deckCount.AddThemeFontSizeOverride("font_size", 14);
            row.AddChild(deckCount);

            for (int i = 0; i < 4; i++)
            {
                var cardDisplay = new CardDisplay();
                cardDisplay.CustomMinimumSize = new Vector2(120, 180);
                row.AddChild(cardDisplay);
                var card = i < _state.TierMarket[tier].Count ? _state.TierMarket[tier][i] : null;
                cardDisplay.SetCard(card);
            }
        }

        // Update gem bank (interactive)
        _gemBank.SetBank(_state.Bank, interactive: true);
        _actionLabel.Text = "";
        _confirmButton.Visible = false;
        _cancelButton.Visible = false;

        // Update players
        var children = _playersArea.GetChildren();
        for (int i = children.Count - 1; i >= 1; i--)
            children[i].QueueFree();

        for (int i = 0; i < _state.Players.Count; i++)
        {
            var panel = new PlayerPanel();
            panel.CustomMinimumSize = new Vector2(280, 200);
            panel.SizeFlagsHorizontal = SizeFlags.ExpandFill;
            _playersArea.AddChild(panel);
            panel.SetPlayer(_state.Players[i], i, i == _state.CurrentPlayerIndex);
        }
    }
}
