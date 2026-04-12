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

    private void OnStartGame(int playerCount)
    {
        _currentView?.QueueFree();

        var board = new GameBoard { PlayerCount = playerCount };
        board.SetAnchorsPreset(LayoutPreset.FullRect);
        AddChild(board);
        _currentView = board;
    }
}
