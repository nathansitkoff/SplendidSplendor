# CLAUDE.md

## Project Overview

SplendidSplendor is a digital implementation of Marc André's board game Splendor (published by Asmodee), built in Godot 4.6 with C#.

- **Engine:** Godot 4.6 (.NET / C#)
- **Platform:** Desktop (1280x720)
- **Current scope:** Local hot-seat multiplayer (2-4 players), clean 2D style, core gameplay only. AI opponents planned for later.

## Architecture

- **Model layer** (`Scripts/Model/`): Pure C# classes with no Godot dependencies. Testable standalone.
- **Logic layer** (`Scripts/Logic/`): Game engine, action validation, card database. Also pure C#.
- **UI layer** (`Scripts/UI/`): Godot node scripts that render GameState and handle input.
- **Tests** (`Tests/`): xUnit test project. Tests target the Model and Logic layers.

## Workflow

- Implementation follows the phased plan in `PLAN.md`.
- Stop after each phase and check in before proceeding to the next.
- Always ask before pushing to git.
- **Test-driven development:** Write tests first, then implement code to make them pass.
- Each phase should be independently testable — unit tests for logic, manual verification for UI.

## Build & Test

```bash
dotnet build          # Build game + test projects
dotnet test           # Run xUnit tests
```

To run the game, open in Godot editor or:
```bash
/Applications/Godot_mono.app/Contents/MacOS/Godot --path .
```
