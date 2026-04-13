using Godot;

namespace SplendidSplendor.UI;

public partial class Main : Control
{
    private Node? _currentView;

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

        var board = new GameBoard { PlayerCount = playerCount, IsAi = isAi };
        board.SetAnchorsPreset(LayoutPreset.FullRect);
        AddChild(board);
        _currentView = board;
    }
}
