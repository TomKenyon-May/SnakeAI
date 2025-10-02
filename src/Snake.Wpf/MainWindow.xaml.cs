using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Snake.Core;
using System.Windows.Threading;
using System.Runtime.CompilerServices;
using System.CodeDom;

namespace Snake.Wpf;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private readonly SnakeGame _game = new SnakeGame();
    private readonly DispatcherTimer _timer = new DispatcherTimer();
    private Core.Action _action = Core.Action.MoveForward;
    private const int CellSize = 40;
    public MainWindow()
    {
        InitializeComponent();

        this.PreviewKeyDown += OnKeyDown;

        Loaded += (_, __) =>
        {
            GameCanvas.Focusable = true;
            GameCanvas.Focus();
        };

        _timer.Interval = TimeSpan.FromMilliseconds(100);
        _timer.Tick += GameLoop;
        _timer.Start();
    }

    private void DrawSquare(int x, int y, Brush colour)
    {
        var rectangle = new Rectangle
        {
            Width = CellSize,
            Height = CellSize,
            Fill = colour
        };
        Canvas.SetLeft(rectangle, x * CellSize);
        Canvas.SetTop(rectangle, y * CellSize);
        GameCanvas.Children.Add(rectangle);
    }

    private void DrawGame()
    {
        GameCanvas.Children.Clear();

        foreach (Core.Point point in _game.Snake)
        {
            DrawSquare(point.X, point.Y, Brushes.Green);
        }

        DrawSquare(_game.Apple.X, _game.Apple.Y, Brushes.Red);
    }

    private void GameLoop(object? sender, EventArgs e)
    {
        if (_game.SnakeDead)
        {
            _timer.Stop();
            MessageBox.Show("Game Over");
            return;
        }

        ApplyAction(_action);
        _action = Core.Action.MoveForward;

        _game.MoveSnake();
        DrawGame();
    }

    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (TryMapKey(e.Key, out var act))
        {
            _action = act;
            e.Handled = true;
        }
    }

    private void ApplyAction(Core.Action action)
    {
        switch (action)
        {
            case Core.Action.TurnLeft:
                _game.SnakeDirection = (Direction)(((int)_game.SnakeDirection + 3) % 4);
                break;
            case Core.Action.TurnRight:
                _game.SnakeDirection = (Direction)(((int)_game.SnakeDirection + 1) % 4);
                break;
            case Core.Action.MoveForward:
            default:
                break;
        }
    }

    private bool TryMapKey(Key key, out Core.Action act)
    {
        switch (key)
        {
            case Key.W:
            case Key.Up:
                act = _game.SnakeDirection is Direction.Left ? Core.Action.TurnRight :
                        _game.SnakeDirection is Direction.Right ? Core.Action.TurnLeft :
                        Core.Action.MoveForward;
                return act != Core.Action.MoveForward;
            case Key.S:
            case Key.Down:
                act = _game.SnakeDirection is Direction.Left ? Core.Action.TurnLeft :
                        _game.SnakeDirection is Direction.Right ? Core.Action.TurnRight :
                        Core.Action.MoveForward;
                return act != Core.Action.MoveForward;
            case Key.A:
            case Key.Left:
                act = _game.SnakeDirection is Direction.Up ? Core.Action.TurnLeft :
                        _game.SnakeDirection is Direction.Down ? Core.Action.TurnRight :
                        Core.Action.MoveForward;
                return act != Core.Action.MoveForward;
            case Key.D:
            case Key.Right:
                act = _game.SnakeDirection is Direction.Up ? Core.Action.TurnRight :
                        _game.SnakeDirection is Direction.Down ? Core.Action.TurnLeft :
                        Core.Action.MoveForward;
                return act != Core.Action.MoveForward;
            default:
                act = Core.Action.MoveForward;
                return false;
        }
    }
}