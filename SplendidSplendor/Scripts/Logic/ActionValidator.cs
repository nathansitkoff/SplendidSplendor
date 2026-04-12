using SplendidSplendor.Model;

namespace SplendidSplendor.Logic;

public static class ActionValidator
{
    public static bool IsValid(GameState state, GameAction action)
    {
        return action switch
        {
            GameAction.TakeThreeGemsAction a => IsValidTakeThree(state, a),
            GameAction.TakeTwoGemsAction a => IsValidTakeTwo(state, a),
            _ => false
        };
    }

    private static bool IsValidTakeThree(GameState state, GameAction.TakeThreeGemsAction action)
    {
        var colors = action.Colors;

        // Must take at least 1
        if (colors.Count == 0)
            return false;

        // No gold
        if (colors.Any(c => c == GemType.Gold))
            return false;

        // No duplicates
        if (colors.Distinct().Count() != colors.Count)
            return false;

        // All selected colors must have at least 1 in bank
        if (colors.Any(c => state.Bank[c] <= 0))
            return false;

        // Must take as many as possible (up to 3)
        int availableColors = CountAvailableColors(state);
        int maxCanTake = Math.Min(3, availableColors);
        if (colors.Count != maxCanTake)
            return false;

        return true;
    }

    private static bool IsValidTakeTwo(GameState state, GameAction.TakeTwoGemsAction action)
    {
        // No gold
        if (action.Color == GemType.Gold)
            return false;

        // Must have 4 or more of that color
        if (state.Bank[action.Color] < 4)
            return false;

        return true;
    }

    private static int CountAvailableColors(GameState state)
    {
        int count = 0;
        var gemTypes = new[] { GemType.White, GemType.Blue, GemType.Green, GemType.Red, GemType.Black };
        foreach (var type in gemTypes)
        {
            if (state.Bank[type] > 0)
                count++;
        }
        return count;
    }
}
