using Godot;
using SplendidSplendor.Model;

namespace SplendidSplendor.UI;

public partial class GemBank : HBoxContainer
{
    private GemCollection _bank = new();
    private bool _interactive;
    private readonly HashSet<GemType> _selected = new();
    private GemType? _takeTwoColor;

    [Signal]
    public delegate void SelectionChangedEventHandler();

    public IReadOnlyCollection<GemType> SelectedGems => _selected;
    public GemType? TakeTwoColor => _takeTwoColor;

    public void SetBank(GemCollection bank, bool interactive = false)
    {
        _bank = bank;
        _interactive = interactive;
        _selected.Clear();
        _takeTwoColor = null;
        UpdateDisplay();
    }

    public void ClearSelection()
    {
        _selected.Clear();
        _takeTwoColor = null;
        UpdateDisplay();
    }

    public override void _Ready()
    {
        AddThemeConstantOverride("separation", 14);
    }

    private void UpdateDisplay()
    {
        foreach (var child in GetChildren())
            child.QueueFree();

        var gemTypes = new[] { GemType.White, GemType.Blue, GemType.Green, GemType.Red, GemType.Black, GemType.Gold };

        foreach (var type in gemTypes)
        {
            bool isInteractive = _interactive && type != GemType.Gold;
            bool isSelected = _selected.Contains(type) || _takeTwoColor == type;
            var gem = new GemToken();
            gem.SetGem(type, _bank[type], isInteractive, isSelected);
            gem.CustomMinimumSize = new Vector2(72, 40);
            gem.GemClicked += OnGemClicked;
            AddChild(gem);
        }
    }

    private void OnGemClicked(int gemType)
    {
        var type = (GemType)gemType;

        if (_takeTwoColor != null)
        {
            // Already in take-2 mode — clicking again cancels
            _takeTwoColor = null;
        }
        else if (_selected.Contains(type))
        {
            // Clicking an already-selected gem: try take-2 mode
            if (_bank[type] >= 4)
            {
                _selected.Clear();
                _takeTwoColor = type;
            }
            else
            {
                // Can't take 2, just deselect
                _selected.Remove(type);
            }
        }
        else if (_selected.Count < 3)
        {
            _selected.Add(type);
        }

        UpdateDisplay();
        EmitSignal(SignalName.SelectionChanged);
    }
}
