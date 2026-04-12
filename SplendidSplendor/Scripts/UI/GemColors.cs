using Godot;
using SplendidSplendor.Model;

namespace SplendidSplendor.UI;

public static class GemColors
{
    public static Color GetColor(GemType type) => type switch
    {
        GemType.White => new Color(0.95f, 0.95f, 0.95f),
        GemType.Blue => new Color(0.2f, 0.4f, 0.85f),
        GemType.Green => new Color(0.15f, 0.7f, 0.3f),
        GemType.Red => new Color(0.85f, 0.2f, 0.2f),
        GemType.Black => new Color(0.2f, 0.2f, 0.2f),
        GemType.Gold => new Color(0.9f, 0.75f, 0.1f),
        _ => Colors.Gray
    };

    public static Color GetTextColor(GemType type) => type switch
    {
        GemType.White => Colors.Black,
        GemType.Gold => Colors.Black,
        _ => Colors.White
    };

    public static string GetLabel(GemType type) => type switch
    {
        GemType.White => "W",
        GemType.Blue => "B",
        GemType.Green => "G",
        GemType.Red => "R",
        GemType.Black => "K",
        GemType.Gold => "$",
        _ => "?"
    };
}
