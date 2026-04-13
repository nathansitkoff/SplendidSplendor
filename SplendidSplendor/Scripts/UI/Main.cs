using Godot;

namespace SplendidSplendor.UI;

public partial class Main : Control
{
    private Node? _currentView;
    private int _lastPlayerCount = 2;
    private bool[] _lastIsAi = System.Array.Empty<bool>();

    public override void _Ready()
    {
        ShowMainMenu();
    }

    private void ShowMainMenu()
    {
        _currentView?.QueueFree();

        var menu = new MainMenu();
        menu.SetAnchorsPreset(LayoutPreset.FullRect);
        menu.StartGame += OnStartGame;
        AddChild(menu);
        _currentView = menu;
    }

    private void OnStartGame(int playerCount, int[] aiFlags)
    {
        _currentView?.QueueFree();

        var isAi = new bool[playerCount];
        for (int i = 0; i < playerCount; i++)
            isAi[i] = aiFlags[i] == 1;

        _lastPlayerCount = playerCount;
        _lastIsAi = isAi;
        StartNewGame();
    }

    private void StartNewGame()
    {
        _currentView?.QueueFree();

        var board = new GameBoard { PlayerCount = _lastPlayerCount, IsAi = _lastIsAi };
        board.SetAnchorsPreset(LayoutPreset.FullRect);
        board.NewGameRequested += StartNewGame;
        board.MainMenuRequested += ShowMainMenu;
        AddChild(board);
        _currentView = board;
    }
}
