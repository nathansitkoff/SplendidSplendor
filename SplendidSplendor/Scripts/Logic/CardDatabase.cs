using System.Text.Json;
using SplendidSplendor.Model;

namespace SplendidSplendor.Logic;

public class CardDatabase
{
    public List<Card> AllCards { get; }
    public List<Noble> AllNobles { get; }

    private CardDatabase(List<Card> cards, List<Noble> nobles)
    {
        AllCards = cards;
        AllNobles = nobles;
    }

    public static CardDatabase Load()
    {
        var jsonPath = FindDataFile();
        var json = File.ReadAllText(jsonPath);
        return ParseJson(json);
    }

    public static CardDatabase ParseJson(string json)
    {
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        var cards = new List<Card>();
        foreach (var elem in root.GetProperty("cards").EnumerateArray())
        {
            cards.Add(new Card
            {
                Tier = elem.GetProperty("tier").GetInt32(),
                BonusType = Enum.Parse<GemType>(elem.GetProperty("bonus").GetString()!),
                Points = elem.GetProperty("points").GetInt32(),
                Cost = ParseCost(elem)
            });
        }

        var nobles = new List<Noble>();
        foreach (var elem in root.GetProperty("nobles").EnumerateArray())
        {
            nobles.Add(new Noble
            {
                Requirements = ParseCost(elem),
                Points = 3
            });
        }

        return new CardDatabase(cards, nobles);
    }

    private static GemCollection ParseCost(JsonElement elem)
    {
        var gems = new GemCollection();
        gems[GemType.White] = elem.GetProperty("white").GetInt32();
        gems[GemType.Blue] = elem.GetProperty("blue").GetInt32();
        gems[GemType.Green] = elem.GetProperty("green").GetInt32();
        gems[GemType.Red] = elem.GetProperty("red").GetInt32();
        gems[GemType.Black] = elem.GetProperty("black").GetInt32();
        return gems;
    }

    private static string FindDataFile()
    {
        // Search upward from the executing assembly to find Data/cards.json
        var dir = AppDomain.CurrentDomain.BaseDirectory;
        for (int i = 0; i < 10; i++)
        {
            var candidate = Path.Combine(dir, "Data", "cards.json");
            if (File.Exists(candidate))
                return candidate;
            var parent = Directory.GetParent(dir);
            if (parent == null) break;
            dir = parent.FullName;
        }

        // Fallback: check relative to current working directory
        var cwdPath = Path.Combine(Directory.GetCurrentDirectory(), "Data", "cards.json");
        if (File.Exists(cwdPath))
            return cwdPath;

        throw new FileNotFoundException("Could not find Data/cards.json");
    }
}
