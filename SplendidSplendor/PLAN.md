# SplendidSplendor — Splendor in Godot 4.6 C#

## Context
Digital implementation of Marc André's Splendor. Local hot-seat multiplayer (2-4 players), clean 2D style, core gameplay only. AI comes later.

## Project Structure
```
SplendidSplendor/
├── project.godot
├── SplendidSplendor.sln         # Solution with both projects
├── SplendidSplendor.csproj      # Godot game project
├── Tests/
│   ├── SplendidSplendor.Tests.csproj  # xUnit test project
│   ├── GemCollectionTests.cs
│   ├── CardDatabaseTests.cs
│   ├── GameSetupTests.cs
│   ├── ActionValidatorTests.cs
│   └── GameEngineTests.cs
├── Data/
│   └── cards.json              # All 90 dev cards + 10 nobles
├── Scripts/
│   ├── Model/                  # Pure C# — no Godot dependencies
│   │   ├── GemType.cs          # Enum: White, Blue, Green, Red, Black, Gold
│   │   ├── GemCollection.cs    # Dictionary wrapper for gem counts
│   │   ├── Card.cs             # Tier, cost, bonus color, points
│   │   ├── Noble.cs            # Requirements, points (always 3)
│   │   ├── PlayerState.cs      # Gems, cards owned, reserved, points
│   │   ├── GameState.cs        # Full game state: players, bank, market, nobles, turn
│   │   └── GameAction.cs       # Sum type: TakeGems / TakeTwoGems / Reserve / Purchase
│   ├── Logic/
│   │   ├── GameEngine.cs       # Applies actions to GameState, validates moves
│   │   ├── ActionValidator.cs  # Returns list of legal actions for current player
│   │   └── CardDatabase.cs     # Loads cards.json, shuffles, deals tiers
│   └── UI/
│       ├── GameBoard.cs        # Main scene controller
│       ├── CardDisplay.cs      # Single card visual
│       ├── GemBank.cs          # Token bank display + interaction
│       ├── PlayerPanel.cs      # Player's tableau, gems, score
│       ├── NobleDisplay.cs     # Noble tile visual
│       └── ActionDialog.cs     # Gem selection / discard dialog
├── Scenes/
│   ├── Main.tscn               # Main game scene
│   ├── Card.tscn               # Reusable card scene
│   ├── Noble.tscn              # Reusable noble scene
│   └── PlayerPanel.tscn        # Player info panel
└── Resources/
    └── Fonts/                  # Clean sans-serif font
```

## Core Data Model

**GemCollection** — wraps `Dictionary<GemType, int>`, supports Add, Subtract, CanAfford, operator overloads. Key building block used everywhere.

**Card** — `int Tier`, `GemCollection Cost`, `GemType BonusType`, `int Points`

**Noble** — `GemCollection Requirements` (counts of card bonuses, not gems), `int Points = 3`

**PlayerState** — `GemCollection Gems`, `List<Card> OwnedCards`, `List<Card> ReservedCards`, computed `GemCollection Bonuses` (from owned cards), computed `int Score`

**GameState** — `List<PlayerState> Players`, `int CurrentPlayerIndex`, `GemCollection Bank`, `List<Card>[] TierDecks` (3 draw piles), `List<Card>[] TierMarket` (3 rows of up to 4), `List<Noble> Nobles`, `bool GameEndTriggered`, `int FinalRound`

**GameAction** — discriminated union:
- `TakeThreeGems(GemType[] colors)` — 3 different colors (or fewer if <3 available)
- `TakeTwoGems(GemType color)` — 2 of same, requires 4+ in bank
- `ReserveCard(int tier, int? marketIndex)` — from market or top of deck
- `PurchaseCard(int tier, int marketIndex, GemCollection payment)` — or from reserved
- `PurchaseReserved(int reserveIndex, GemCollection payment)`

## Game Engine

`GameEngine.ApplyAction(GameState state, GameAction action) -> GameState`
- Validates action via ActionValidator
- Mutates state (or returns new state)
- After action: check noble visits, check 15-point trigger, advance turn
- Returns gems to bank on purchase

`ActionValidator.GetLegalActions(GameState state) -> List<GameAction>`
- Enumerates all valid moves for current player
- Used by UI to enable/disable buttons, and later by AI

## UI Layout (single screen)
```
┌─────────────────────────────────────────┐
│  Nobles (3-5 tiles across top)          │
├─────────────────────────────────────────┤
│  Tier III:  [card] [card] [card] [card] │
│  Tier II:   [card] [card] [card] [card] │
│  Tier I:    [card] [card] [card] [card] │
├──────────────────────┬──────────────────┤
│   Gem Bank           │  Current Player  │
│   ◆5 ◆5 ◆5 ◆5 ◆5   │  Gems: ...       │
│   Gold: 5            │  Cards: ...      │
│                      │  Reserved: ...   │
│   [Take Gems]        │  Score: 0        │
├──────────────────────┴──────────────────┤
│  Player tabs: P1 | P2 | P3 | P4        │
└─────────────────────────────────────────┘
```

## Implementation Phases (iterative, each phase is testable)

### Phase 1: Project Skeleton
- Create Godot C# project, directory structure, .gitignore
- Create xUnit test project (`Tests/SplendidSplendor.Tests.csproj`) referencing the game project
- A blank scene that runs
- **Test:** `dotnet build` succeeds, `dotnet test` runs (0 tests), project opens in Godot

### Phase 2: Card Data
- Create `cards.json` with all 90 development cards + 10 nobles
- Implement `CardDatabase.cs` to load and parse it
- Implement `GemType.cs`, `GemCollection.cs`, `Card.cs`, `Noble.cs`
- **Unit tests (CardDatabaseTests, GemCollectionTests):**
  - Loads all 90 cards: 40 tier-I, 30 tier-II, 20 tier-III
  - Loads all 10 nobles
  - GemCollection arithmetic: Add, Subtract, CanAfford, negative check
  - Every card has valid cost (non-negative, at least one gem required)

### Phase 3: Game State + Setup
- Implement `PlayerState.cs`, `GameState.cs`
- Add `GameEngine.SetupGame(int playerCount)` — initializes bank, shuffles decks, deals market, selects nobles
- **Unit tests (GameSetupTests):**
  - 2-player setup: 4 gems each color, 5 gold, 3 nobles, 4 cards per tier row
  - 3-player setup: 5 gems each color, 5 gold, 4 nobles
  - 4-player setup: 7 gems each color, 5 gold, 5 nobles
  - All dealt market cards removed from their respective decks
  - Player states initialized with 0 gems, 0 cards, 0 points

### Phase 4: Static Board Display
- Build `Main.tscn` scene with the board layout (card market, gem bank, nobles, player panel)
- Implement `CardDisplay.cs`, `NobleDisplay.cs`, `GemBank.cs`, `PlayerPanel.cs`
- Wire up `GameBoard.cs` to render GameState visually
- No interaction yet — just display the dealt game
- **Test:** Run game, see 12 face-up cards in 3 tiers, nobles across top, gem counts in bank, player panel

### Phase 5: Take 3 Different Gems
- Implement `GameAction.cs` (start with TakeThreeGems only)
- Implement `ActionValidator.IsValid()` for this action
- Implement `GameEngine.ApplyAction()` for this action
- UI: click gems in bank to select up to 3 different colors, confirm button
- Turn advances to next player, UI refreshes
- **Unit tests (ActionValidatorTests, GameEngineTests):**
  - Valid: take 3 different colors that exist in bank
  - Valid: take fewer than 3 if fewer colors available
  - Invalid: take 2 of same color via this action
  - Invalid: take from empty color
  - Bank decreases by 1 each, player increases by 1 each
  - Turn advances to next player
- **Manual test:** Two players alternate taking gems in UI

### Phase 6: Take 2 Same-Color Gems
- Add TakeTwoGems action + validation (requires 4+ in bank)
- UI: double-click or toggle to select 2 of same color
- **Unit tests:**
  - Valid when 4+ of that color in bank
  - Invalid when 3 or fewer
  - Bank decreases by 2, player increases by 2
- **Manual test:** Take 2 same in UI, verify can't when <4

### Phase 7: Purchase Cards
- Add PurchaseCard action + validation
- Gem bonuses from owned cards reduce cost
- Spent gems return to bank, gold can substitute
- Purchased card goes to player's tableau, new card dealt from deck
- UI: click a card to see if affordable, confirm to buy
- **Unit tests:**
  - Buy card with exact gems — gems returned, card in tableau, bonus active
  - Buy with gold substitution
  - Buy with card bonus discounts (e.g., own 2 blue cards, cost reduced by 2 blue)
  - Can't buy card you can't afford
  - New card dealt from deck to fill market slot
  - Deck empty — market slot stays empty
- **Manual test:** Buy cards in UI, verify discounts and gold work

### Phase 8: Reserve Cards
- Add ReserveCard action + validation (max 3 reserved, gain 1 gold if available)
- Add PurchaseReserved action
- UI: right-click or reserve button on a card, reserved cards shown in player panel
- **Unit tests:**
  - Reserve from market: card moves to hand, gold gained, market refilled
  - Reserve from deck top (blind): card in hand, gold gained
  - Can't reserve when already holding 3
  - No gold given when gold bank is empty
  - Purchase from reserved: same rules as market purchase
- **Manual test:** Reserve and buy from reserves in UI

### Phase 9: Token Limit + Discard
- Enforce 10-token hand limit
- After taking gems, if >10, show discard dialog to return excess
- **Unit tests:**
  - Player with 8 gems takes 3 → must discard 1
  - Discarded gems return to bank
  - Can't end turn with >10 tokens
- **Manual test:** Trigger discard dialog, choose which gems to return

### Phase 10: Nobles + Victory
- After each turn, check if current player qualifies for a noble (auto-visit)
- Track when a player hits 15 points — set GameEndTriggered, finish the round
- Show victory screen with final scores
- **Unit tests:**
  - Noble visits when player has required bonuses
  - Only 1 noble per turn if multiple qualify
  - Game end triggers at 15 points but round completes
  - All players get equal turns
  - Tiebreaker: fewest purchased cards
- **Manual test:** Play to noble + victory in UI

## Verification Milestones
Each phase has its own test criteria above. Full integration test after Phase 10:
- Play a complete 2-player game from start to 15+ points
- Exercise all 4 action types
- Trigger at least one noble visit
- Hit the 10-token discard limit at least once
- Verify game-end round completion
