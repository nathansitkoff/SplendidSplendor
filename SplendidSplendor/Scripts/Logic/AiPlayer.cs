using SplendidSplendor.Model;

namespace SplendidSplendor.Logic;

/// <summary>
/// Heuristic priority-based AI for Splendor. Follows the ruleset in PLAN.md:
/// 1) win, 2) claim noble, 3) best point card, 4) reserved card,
/// 5) target card, 6) block threat, 7) gems toward target, 8) take-2,
/// 9) fallback take-3, 10) smart discard.
/// </summary>
public static class AiPlayer
{
    private static readonly GemType[] NonGoldTypes =
        { GemType.White, GemType.Blue, GemType.Green, GemType.Red, GemType.Black };

    public static Action<string>? LogCallback { get; set; }

    private static void Log(string msg) => LogCallback?.Invoke(msg);

    private static string FormatGems(GemCollection gems)
    {
        var parts = new List<string>();
        foreach (var type in new[] { GemType.White, GemType.Blue, GemType.Green, GemType.Red, GemType.Black, GemType.Gold })
        {
            if (gems[type] > 0) parts.Add($"{GemLabel(type)}{gems[type]}");
        }
        return parts.Count == 0 ? "none" : string.Join(" ", parts);
    }

    private static string FormatCard(Card c)
        => $"T{c.Tier}/{GemLabel(c.BonusType)}/{c.Points}pt (cost: {FormatGems(c.Cost)})";

    private static string GemLabel(GemType t) => t switch
    {
        GemType.White => "W",
        GemType.Blue => "B",
        GemType.Green => "G",
        GemType.Red => "R",
        GemType.Black => "K",
        GemType.Gold => "$",
        _ => "?"
    };

    public static GameAction ChooseAction(GameState state)
    {
        var currentPlayer = state.CurrentPlayer;
        Log($"");
        Log($"=== P{state.CurrentPlayerIndex + 1} | score={currentPlayer.Score} | gems={FormatGems(currentPlayer.Gems)} (total {currentPlayer.Gems.Total}) | bonuses={FormatGems(currentPlayer.Bonuses)} | reserved={currentPlayer.ReservedCards.Count} ===");

        if (state.NeedsDiscard)
        {
            var discardAction = ChooseDiscard(state);
            var d = (GameAction.DiscardGemsAction)discardAction;
            Log($"  Rule 10 (discard): discarding {FormatGems(d.Gems)}");
            return discardAction;
        }

        var player = state.CurrentPlayer;

        // Gather all affordable cards (market + reserved)
        var affordableMarket = new List<(int tier, int index, Card card)>();
        for (int t = 0; t < 3; t++)
        {
            for (int i = 0; i < state.TierMarket[t].Count; i++)
            {
                var c = state.TierMarket[t][i];
                if (ActionValidator.CanAffordCard(player, c))
                    affordableMarket.Add((t, i, c));
            }
        }

        var affordableReserved = new List<(int index, Card card)>();
        for (int i = 0; i < player.ReservedCards.Count; i++)
        {
            var c = player.ReservedCards[i];
            if (ActionValidator.CanAffordCard(player, c))
                affordableReserved.Add((i, c));
        }

        // === Rule 1: Win the game ===
        foreach (var (tier, idx, card) in affordableMarket)
        {
            if (WouldWin(state, player, card))
            {
                Log($"  Rule 1 (WIN): buying {FormatCard(card)}");
                return GameAction.PurchaseCard(tier, idx);
            }
        }
        foreach (var (idx, card) in affordableReserved)
        {
            if (WouldWin(state, player, card))
            {
                Log($"  Rule 1 (WIN): buying reserved {FormatCard(card)}");
                return GameAction.PurchaseReserved(idx);
            }
        }

        // === Rule 2: Claim a noble ===
        var nobleBuys = new List<(GameAction action, Card card, int score)>();
        foreach (var (tier, idx, card) in affordableMarket)
        {
            var noble = NobleClaimedByBuying(state, player, card);
            if (noble != null)
                nobleBuys.Add((GameAction.PurchaseCard(tier, idx), card, card.Points));
        }
        foreach (var (idx, card) in affordableReserved)
        {
            var noble = NobleClaimedByBuying(state, player, card);
            if (noble != null)
                nobleBuys.Add((GameAction.PurchaseReserved(idx), card, card.Points));
        }
        if (nobleBuys.Count > 0)
        {
            var pick = nobleBuys.OrderByDescending(x => x.score).First();
            Log($"  Rule 2 (claim noble): buying {FormatCard(pick.card)}");
            return pick.action;
        }

        // === Rule 3: Buy best point card ===
        var pointMarketBuys = affordableMarket.Where(x => x.card.Points > 0).ToList();
        // === Rule 4: Buy reserved point cards (considered together with rule 3) ===
        var pointReservedBuys = affordableReserved.Where(x => x.card.Points > 0).ToList();

        if (pointMarketBuys.Count > 0 || pointReservedBuys.Count > 0)
        {
            int bestPts = 0;
            GameAction? bestAction = null;
            Card? bestCard = null;
            int bestCost = int.MaxValue;

            foreach (var (tier, idx, card) in pointMarketBuys)
            {
                int cost = card.Cost.Total;
                if (card.Points > bestPts || (card.Points == bestPts && cost < bestCost))
                {
                    bestPts = card.Points;
                    bestCost = cost;
                    bestAction = GameAction.PurchaseCard(tier, idx);
                    bestCard = card;
                }
            }
            foreach (var (idx, card) in pointReservedBuys)
            {
                int cost = card.Cost.Total;
                if (card.Points > bestPts || (card.Points == bestPts && cost < bestCost))
                {
                    bestPts = card.Points;
                    bestCost = cost;
                    bestAction = GameAction.PurchaseReserved(idx);
                    bestCard = card;
                }
            }
            if (bestAction != null)
            {
                Log($"  Rule 3/4 (best point card): buying {FormatCard(bestCard!)}");
                return bestAction;
            }
        }

        // === Rule 4.5: Engine building / avoid overflow ===
        // If we can't afford a point card but CAN afford something,
        // buy the best-bonus card when either:
        //   (a) we have fewer than 4 bonus cards (early engine building), or
        //   (b) we have 8+ gems and risk discarding next turn
        if (affordableMarket.Count > 0 || affordableReserved.Count > 0)
        {
            int totalGems = player.Gems.Total;
            int bonusCount = player.OwnedCards.Count;
            bool earlyGame = bonusCount < 4;
            bool gemOverflow = totalGems >= 8;

            if (earlyGame || gemOverflow)
            {
                GameAction? engineBuy = null;
                Card? engineCard = null;
                int bestBonusScore = int.MinValue;

                foreach (var (tier, idx, card) in affordableMarket)
                {
                    int bonusScore = ScoreBonusUsefulness(state, player, card.BonusType)
                        - card.Cost.Total; // prefer cheap cards
                    if (bonusScore > bestBonusScore)
                    {
                        bestBonusScore = bonusScore;
                        engineBuy = GameAction.PurchaseCard(tier, idx);
                        engineCard = card;
                    }
                }
                foreach (var (idx, card) in affordableReserved)
                {
                    int bonusScore = ScoreBonusUsefulness(state, player, card.BonusType)
                        - card.Cost.Total;
                    if (bonusScore > bestBonusScore)
                    {
                        bestBonusScore = bonusScore;
                        engineBuy = GameAction.PurchaseReserved(idx);
                        engineCard = card;
                    }
                }
                if (engineBuy != null)
                {
                    var reason = earlyGame ? "early engine" : "avoid overflow";
                    Log($"  Rule 4.5 ({reason}): buying {FormatCard(engineCard!)} (bonus score {bestBonusScore})");
                    return engineBuy;
                }
            }
        }

        // === Rule 5: Pick a target card ===
        var target = PickTargetCard(state, player);
        if (target != null)
            Log($"  Target: {FormatCard(target)} (score {ScoreTargetCard(state, player, target)})");
        else
            Log($"  No target card could be picked");

        // === Rule 6: Reserve threatening card ===
        if (player.ReservedCards.Count < 3)
        {
            var threat = FindThreateningCard(state);
            if (threat != null)
            {
                var tc = state.TierMarket[threat.Value.tier][threat.Value.index];
                Log($"  Rule 6 (block threat): reserving {FormatCard(tc)}");
                return GameAction.ReserveCard(threat.Value.tier, threat.Value.index);
            }
        }

        // === Rules 7-9: Take gems ===
        if (target != null)
        {
            var neededColors = ColorsNeededFor(player, target);
            Log($"  Needed colors for target: {string.Join(",", neededColors.Select(GemLabel))}");

            // Rule 8: If only 1-2 colors needed and bank has 4+, take 2 same
            if (neededColors.Count <= 2)
            {
                foreach (var color in neededColors)
                {
                    if (state.Bank[color] >= 4)
                    {
                        Log($"  Rule 8 (take 2 same): taking 2x {GemLabel(color)}");
                        return GameAction.TakeTwoGems(color);
                    }
                }
            }

            // Rule 7: Take 3 different, prioritizing needed colors
            var toTake = new List<GemType>();
            foreach (var color in neededColors)
            {
                if (state.Bank[color] > 0 && toTake.Count < 3)
                    toTake.Add(color);
            }
            // Fill with any available colors if we don't have 3 needed
            foreach (var color in NonGoldTypes)
            {
                if (toTake.Count >= 3) break;
                if (!toTake.Contains(color) && state.Bank[color] > 0)
                    toTake.Add(color);
            }
            if (toTake.Count > 0)
            {
                var action = GameAction.TakeThreeGems(toTake.ToArray());
                if (ActionValidator.IsValid(state, action))
                {
                    Log($"  Rule 7 (take toward target): taking {string.Join(",", toTake.Select(GemLabel))}");
                    return action;
                }
            }
        }

        // === Rule 9: Fallback — take any 3 different gems ===
        var fallback = new List<GemType>();
        foreach (var color in NonGoldTypes)
        {
            if (fallback.Count >= 3) break;
            if (state.Bank[color] > 0) fallback.Add(color);
        }
        if (fallback.Count > 0)
        {
            var action = GameAction.TakeThreeGems(fallback.ToArray());
            if (ActionValidator.IsValid(state, action))
            {
                Log($"  Rule 9 (fallback take 3): taking {string.Join(",", fallback.Select(GemLabel))}");
                return action;
            }
        }

        // Last resort: take 2 of any color with 4+
        foreach (var color in NonGoldTypes)
        {
            if (state.Bank[color] >= 4)
            {
                Log($"  Rule 9b (last resort take 2): taking 2x {GemLabel(color)}");
                return GameAction.TakeTwoGems(color);
            }
        }

        // Truly desperate: reserve a card
        for (int t = 0; t < 3; t++)
        {
            if (state.TierMarket[t].Count > 0 && player.ReservedCards.Count < 3)
            {
                Log($"  Rule 9c (desperate reserve)");
                return GameAction.ReserveCard(t, 0);
            }
        }

        throw new InvalidOperationException("AI could not find any legal action");
    }

    // === Helpers ===

    private static bool WouldWin(GameState state, PlayerState player, Card card)
    {
        int projected = player.Score + card.Points;
        // Would this card also trigger a noble?
        var projectedBonuses = new GemCollection();
        foreach (var owned in player.OwnedCards)
            projectedBonuses[owned.BonusType]++;
        projectedBonuses[card.BonusType]++;

        foreach (var noble in state.Nobles)
        {
            bool qualifies = true;
            foreach (var type in NonGoldTypes)
            {
                if (projectedBonuses[type] < noble.Requirements[type])
                {
                    qualifies = false;
                    break;
                }
            }
            if (qualifies)
            {
                projected += noble.Points;
                break; // Only 1 noble per turn
            }
        }
        return projected >= 15;
    }

    private static Noble? NobleClaimedByBuying(GameState state, PlayerState player, Card card)
    {
        var projectedBonuses = new GemCollection();
        foreach (var owned in player.OwnedCards)
            projectedBonuses[owned.BonusType]++;
        projectedBonuses[card.BonusType]++;

        foreach (var noble in state.Nobles)
        {
            bool qualifies = true;
            foreach (var type in NonGoldTypes)
            {
                if (projectedBonuses[type] < noble.Requirements[type])
                {
                    qualifies = false;
                    break;
                }
            }
            if (qualifies) return noble;
        }
        return null;
    }

    private static Card? PickTargetCard(GameState state, PlayerState player)
    {
        Card? best = null;
        int bestScore = int.MinValue;

        for (int t = 0; t < 3; t++)
        {
            foreach (var card in state.TierMarket[t])
            {
                int score = ScoreTargetCard(state, player, card);
                if (score > bestScore)
                {
                    bestScore = score;
                    best = card;
                }
            }
        }
        foreach (var card in player.ReservedCards)
        {
            int score = ScoreTargetCard(state, player, card);
            if (score > bestScore)
            {
                bestScore = score;
                best = card;
            }
        }
        return best;
    }

    private static int ScoreTargetCard(GameState state, PlayerState player, Card card)
    {
        int score = 2 * card.Points;
        score += ScoreBonusUsefulness(state, player, card.BonusType);

        // Distance to afford: effective cost minus what we have
        int distance = 0;
        var bonuses = player.Bonuses;
        foreach (var type in NonGoldTypes)
        {
            int effectiveCost = Math.Max(0, card.Cost[type] - bonuses[type]);
            int shortfall = Math.Max(0, effectiveCost - player.Gems[type]);
            distance += shortfall;
        }
        distance = Math.Max(0, distance - player.Gems[GemType.Gold]);

        // Distance penalty — heavier for unreachable cards.
        // A card 4+ gems away is severely penalized to prevent hoarding.
        score -= 2 * distance;

        return score;
    }

    private static int ScoreBonusUsefulness(GameState state, PlayerState player, GemType bonusType)
    {
        if (bonusType == GemType.Gold) return 0;

        int score = 0;
        var bonuses = player.Bonuses;

        // Nobles that need this bonus type
        foreach (var noble in state.Nobles)
        {
            int need = noble.Requirements[bonusType];
            int have = bonuses[bonusType];
            if (need > have)
                score += 3;
        }

        // Market cards that cost this color — this bonus reduces future costs
        for (int t = 0; t < 3; t++)
        {
            foreach (var card in state.TierMarket[t])
            {
                if (card.Cost[bonusType] > 0)
                    score += 1;
            }
        }

        return score;
    }

    private static List<GemType> ColorsNeededFor(PlayerState player, Card card)
    {
        var needed = new List<GemType>();
        var bonuses = player.Bonuses;
        foreach (var type in NonGoldTypes)
        {
            int effectiveCost = Math.Max(0, card.Cost[type] - bonuses[type]);
            if (effectiveCost > player.Gems[type])
                needed.Add(type);
        }
        return needed;
    }

    private static (int tier, int index)? FindThreateningCard(GameState state)
    {
        // Check other players
        for (int p = 0; p < state.Players.Count; p++)
        {
            if (p == state.CurrentPlayerIndex) continue;
            var opp = state.Players[p];
            if (opp.Score < 13) continue; // not threatening yet

            // See if they can afford any market card that puts them at 15+
            for (int t = 0; t < 3; t++)
            {
                for (int i = 0; i < state.TierMarket[t].Count; i++)
                {
                    var card = state.TierMarket[t][i];
                    if (ActionValidator.CanAffordCard(opp, card) && opp.Score + card.Points >= 15)
                    {
                        return (t, i);
                    }
                }
            }
        }
        return null;
    }

    private static GameAction ChooseDiscard(GameState state)
    {
        var player = state.CurrentPlayer;
        int excess = player.Gems.Total - 10;
        var discard = new GemCollection();

        // Figure out which colors the AI cares about (target card)
        var target = PickTargetCard(state, player);
        var neededColors = target != null
            ? new HashSet<GemType>(ColorsNeededFor(player, target))
            : new HashSet<GemType>();

        // Discard priority: colors not needed first, then by excess
        var allTypes = new[]
        {
            GemType.White, GemType.Blue, GemType.Green, GemType.Red, GemType.Black, GemType.Gold
        };

        // Sort: not-needed first, then by current count (most first)
        var sorted = allTypes
            .OrderBy(t => t == GemType.Gold ? 2 : (neededColors.Contains(t) ? 1 : 0))
            .ThenByDescending(t => player.Gems[t])
            .ToList();

        int remaining = excess;
        foreach (var type in sorted)
        {
            if (remaining == 0) break;
            int have = player.Gems[type];
            int take = Math.Min(have, remaining);
            if (take > 0)
            {
                discard[type] = take;
                remaining -= take;
            }
        }

        return GameAction.DiscardGems(discard);
    }
}
