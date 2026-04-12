using Godot;
using SplendidSplendor.Model;

namespace SplendidSplendor.UI;

public partial class GemBank : HBoxContainer
{
    private GemCollection _bank = new();
    private bool _interactive;
    private readonly HashSet<GemType> _selected = new();

    [Signal]
    public delegate void SelectionChangedEventHandler();

    public IReadOnlyCollection<GemType> SelectedGems => _selected;

    public void SetBank(GemCollection bank, bool interactive = false)
    {
        _bank = bank;
        _interactive = interactive;
        _selected.Clear();
        UpdateDisplay();
    }

    public void ClearSelection()
    {
        _selected.Clear();
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
            // Don't show gold as selectable for take-gems action
            bool isInteractive = _interactive && type != GemType.Gold;
            var gem = new GemToken();
            gem.SetGem(type, _bank[type], isInteractive, _selected.Contains(type));
            gem.CustomMinimumSize = new Vector2(72, 40);
            gem.GemClicked += OnGemClicked;
            AddChild(gem);
        }
    }

    private void OnGemClicked(int gemType)
    {
        var type = (GemType)gemType;

        if (_selected.Contains(type))
        {
            _selected.Remove(type);
        }
        else if (_selected.Count < 3)
        {
            _selected.Add(type);
        }

        UpdateDisplay();
        EmitSignal(SignalName.SelectionChanged);
    }
}
