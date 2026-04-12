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
    private DiscardPanel _discardPanel = null!;
    private PanelContainer _victoryPanel = null!;
    private Label _victoryLabel = null!;

    public int PlayerCount { get; set; } = 2;

    public override void _Ready()
    {
        _state = GameEngine.SetupGame(PlayerCount);
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

        // Card market
        _marketArea = new VBoxContainer();
        _marketArea.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        _marketArea.SizeFlagsVertical = SizeFlags.ExpandFill;
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
        root.AddChild(_marketArea);

        // Margin wrapper for left side (title, nobles, bank, market)
        var margin = new MarginContainer();
        margin.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        margin.AddThemeConstantOverride("margin_left", 12);
        margin.AddThemeConstantOverride("margin_right", 640);
        margin.AddThemeConstantOverride("margin_top", 8);
        margin.AddThemeConstantOverride("margin_bottom", 8);
        margin.AddChild(root);
        AddChild(margin);

        // Players panel — absolutely positioned, anchored top-right, full height
        var playerPanel = new ScrollContainer();
        playerPanel.AnchorLeft = 1.0f;
        playerPanel.AnchorRight = 1.0f;
        playerPanel.AnchorTop = 0.0f;
        playerPanel.AnchorBottom = 1.0f;
        playerPanel.OffsetLeft = -630;
        playerPanel.OffsetRight = -8;
        playerPanel.OffsetTop = 8;
        playerPanel.OffsetBottom = -8;
        playerPanel.HorizontalScrollMode = ScrollContainer.ScrollMode.Disabled;

        _playersArea = new VBoxContainer();
        _playersArea.AddThemeConstantOverride("separation", 6);
        _playersArea.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        var playersLabel = new Label { Text = "PLAYERS" };
        playersLabel.AddThemeColorOverride("font_color", Colors.Gray);
        playersLabel.AddThemeFontSizeOverride("font_size", 11);
        _playersArea.AddChild(playersLabel);
        playerPanel.AddChild(_playersArea);
        AddChild(playerPanel);

        // Discard panel — centered overlay, hidden by default
        _discardPanel = new DiscardPanel();
        _discardPanel.Visible = false;
        _discardPanel.AnchorLeft = 0.15f;
        _discardPanel.AnchorRight = 0.65f;
        _discardPanel.AnchorTop = 0.35f;
        _discardPanel.AnchorBottom = 0.65f;
        _discardPanel.DiscardConfirmed += OnDiscardConfirmed;
        // Dark background
        _discardPanel.AddThemeStyleboxOverride("panel", new StyleBoxFlat
        {
            BgColor = new Color(0.1f, 0.1f, 0.15f, 0.95f),
            BorderWidthLeft = 2, BorderWidthRight = 2,
            BorderWidthTop = 2, BorderWidthBottom = 2,
            BorderColor = Colors.Yellow
        });
        AddChild(_discardPanel);

        // Victory overlay — hidden by default
        _victoryPanel = new PanelContainer();
        _victoryPanel.Visible = false;
        _victoryPanel.AnchorLeft = 0.2f;
        _victoryPanel.AnchorRight = 0.6f;
        _victoryPanel.AnchorTop = 0.25f;
        _victoryPanel.AnchorBottom = 0.75f;
        _victoryPanel.AddThemeStyleboxOverride("panel", new StyleBoxFlat
        {
            BgColor = new Color(0.05f, 0.05f, 0.1f, 0.95f),
            BorderWidthLeft = 3, BorderWidthRight = 3,
            BorderWidthTop = 3, BorderWidthBottom = 3,
            BorderColor = new Color(0.9f, 0.75f, 0.1f)
        });
        var victoryMargin = new MarginContainer();
        victoryMargin.AddThemeConstantOverride("margin_left", 20);
        victoryMargin.AddThemeConstantOverride("margin_right", 20);
        victoryMargin.AddThemeConstantOverride("margin_top", 20);
        victoryMargin.AddThemeConstantOverride("margin_bottom", 20);
        _victoryLabel = new Label();
        _victoryLabel.AddThemeColorOverride("font_color", Colors.White);
        _victoryLabel.AddThemeFontSizeOverride("font_size", 20);
        _victoryLabel.AutowrapMode = TextServer.AutowrapMode.Word;
        victoryMargin.AddChild(_victoryLabel);
        _victoryPanel.AddChild(victoryMargin);
        AddChild(_victoryPanel);
    }

    private GameAction? BuildCurrentAction()
    {
        var takeTwoColor = _gemBank.TakeTwoColor;
        if (takeTwoColor != null)
            return GameAction.TakeTwoGems(takeTwoColor.Value);

        var selected = _gemBank.SelectedGems;
        if (selected.Count > 0)
            return GameAction.TakeThreeGems(selected.ToArray());

        return null;
    }

    private void OnGemSelectionChanged()
    {
        var action = BuildCurrentAction();
        if (action != null)
        {
            bool valid = ActionValidator.IsValid(_state, action);

            if (action is GameAction.TakeTwoGemsAction t2)
                _actionLabel.Text = $"Take 2x {GemColors.GetLabel(t2.Color)}";
            else if (action is GameAction.TakeThreeGemsAction t3)
            {
                _actionLabel.Text = $"Take gems: {string.Join(", ", t3.Colors.Select(g => GemColors.GetLabel(g)))}";
                if (!valid)
                {
                    int availableColors = 0;
                    var gemTypes = new[] { GemType.White, GemType.Blue, GemType.Green, GemType.Red, GemType.Black };
                    foreach (var t in gemTypes)
                        if (_state.Bank[t] > 0) availableColors++;
                    int needed = Math.Min(3, availableColors);
                    if (t3.Colors.Count < needed)
                        _actionLabel.Text += $" (select {needed})";
                }
            }

            _confirmButton.Visible = true;
            _confirmButton.Disabled = !valid;
            _cancelButton.Visible = true;
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
        var action = BuildCurrentAction();
        if (action == null) return;
        if (!ActionValidator.IsValid(_state, action)) return;

        GameEngine.ApplyAction(_state, action);
        RefreshDisplay();
    }

    private void OnCardClicked(int tier, int marketIndex)
    {
        var action = GameAction.PurchaseCard(tier, marketIndex);
        if (!ActionValidator.IsValid(_state, action)) return;

        GameEngine.ApplyAction(_state, action);
        RefreshDisplay();
    }

    private void OnCardReserved(int tier, int marketIndex)
    {
        var action = GameAction.ReserveCard(tier, marketIndex);
        if (!ActionValidator.IsValid(_state, action)) return;

        GameEngine.ApplyAction(_state, action);
        RefreshDisplay();
    }

    private void OnReservedCardClicked(int reserveIndex)
    {
        var action = GameAction.PurchaseReserved(reserveIndex);
        if (!ActionValidator.IsValid(_state, action)) return;

        GameEngine.ApplyAction(_state, action);
        RefreshDisplay();
    }

    private void OnDiscardConfirmed()
    {
        var gems = _discardPanel.GetDiscardSelection();
        var action = GameAction.DiscardGems(gems);
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
        // Victory check
        if (_state.GameOver)
        {
            int winner = GameEngine.GetWinner(_state);
            var scores = string.Join("\n", _state.Players.Select((p, i) =>
                $"Player {i + 1}: {p.Score} points ({p.OwnedCards.Count} cards)"));
            _victoryLabel.Text = $"GAME OVER\n\nPlayer {winner + 1} wins!\n\n{scores}";
            _victoryPanel.Visible = true;
            _discardPanel.Visible = false;
        }
        else
        {
            _victoryPanel.Visible = false;
        }

        // Discard panel
        if (_state.NeedsDiscard && !_state.GameOver)
        {
            _discardPanel.Setup(_state.CurrentPlayer);
            _discardPanel.Visible = true;
        }
        else
        {
            _discardPanel.Visible = false;
        }

        // Update title
        var turnText = _state.GameOver
            ? "Game Over!"
            : _state.NeedsDiscard
            ? $"Player {_state.CurrentPlayerIndex + 1} — Discard gems!"
            : $"Player {_state.CurrentPlayerIndex + 1}'s Turn";
        _titleLabel.Text = $"Splendid Splendor — {turnText}";

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
                bool affordable = card != null && ActionValidator.CanAffordCard(_state.CurrentPlayer, card);
                cardDisplay.SetCard(card, interactive: true, affordable: affordable, tier: tier, marketIndex: i);
                cardDisplay.CardClicked += OnCardClicked;
                cardDisplay.CardReserved += OnCardReserved;
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
            bool isCurrentPlayer = i == _state.CurrentPlayerIndex;

            // Each player is a row: [reserved cards] [player info panel]
            var playerRow = new HBoxContainer();
            playerRow.AddThemeConstantOverride("separation", 6);
            playerRow.SizeFlagsHorizontal = SizeFlags.ExpandFill;

            // Reserved cards on the left (always reserve the space for 3 slots
            // so player panels align vertically)
            var reserveRow = new HBoxContainer();
            reserveRow.AddThemeConstantOverride("separation", 4);
            reserveRow.CustomMinimumSize = new Vector2(372, 180); // 3 * 120 + 2 * 4 + padding
            for (int j = 0; j < 3; j++)
            {
                var rDisplay = new CardDisplay();
                rDisplay.CustomMinimumSize = new Vector2(120, 180);
                if (j < _state.Players[i].ReservedCards.Count)
                {
                    var rCard = _state.Players[i].ReservedCards[j];
                    bool canBuy = isCurrentPlayer && ActionValidator.CanAffordCard(_state.CurrentPlayer, rCard);
                    rDisplay.SetCard(rCard, interactive: isCurrentPlayer, affordable: canBuy, tier: 0, marketIndex: j);
                    if (isCurrentPlayer)
                    {
                        int reserveIdx = j;
                        rDisplay.CardClicked += (_, _) => OnReservedCardClicked(reserveIdx);
                    }
                }
                else
                {
                    rDisplay.SetCard(null);
                }
                reserveRow.AddChild(rDisplay);
            }
            playerRow.AddChild(reserveRow);

            // Player info panel on the right (narrower)
            var panel = new PlayerPanel();
            panel.CustomMinimumSize = new Vector2(220, 180);
            panel.SizeFlagsHorizontal = SizeFlags.ExpandFill;
            panel.SetPlayer(_state.Players[i], i, isCurrentPlayer);
            playerRow.AddChild(panel);

            _playersArea.AddChild(playerRow);
        }
    }
}
