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

namespace Snake.Wpf;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private readonly SnakeGame _game = new SnakeGame();
    private readonly DispatcherTimer _timer = new DispatcherTimer();
    private const int CellSize = 40;
    public MainWindow()
    {
        InitializeComponent();

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

        _game.MoveSnake();
        DrawGame();
    }
}