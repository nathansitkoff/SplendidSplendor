using Godot;
using SplendidSplendor.Model;

namespace SplendidSplendor.UI;

public partial class GemBank : HBoxContainer
{
    private GemCollection _bank = new();

    public void SetBank(GemCollection bank)
    {
        _bank = bank;
        UpdateDisplay();
    }

    public override void _Ready()
    {
        AddThemeConstantOverride("separation", 14);
        UpdateDisplay();
    }

    private void UpdateDisplay()
    {
        foreach (var child in GetChildren())
            child.QueueFree();

        var gemTypes = new[] { GemType.White, GemType.Blue, GemType.Green, GemType.Red, GemType.Black, GemType.Gold };

        foreach (var type in gemTypes)
        {
            var gem = new GemToken();
            gem.SetGem(type, _bank[type]);
            gem.CustomMinimumSize = new Vector2(72, 40);
            AddChild(gem);
        }
    }
}
