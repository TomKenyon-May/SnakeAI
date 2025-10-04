namespace Snake.Core;

public class SnakeGame
{
    public Point Apple = new Point(10, 10);

    public List<Point> Snake = new List<Point>()
    {
        new Point(5, 5),
        new Point(5, 6),
    };

    public Direction SnakeDirection = Direction.Right;

    public bool SnakeDead = false;

    public void MoveApple()
    {
        do
        {
            Apple.X = Random.Shared.Next(0, 20);
            Apple.Y = Random.Shared.Next(0, 20);
        } while (Snake.Contains(Apple));
    }

    public void MoveSnake()
    {
        Point snakeHead = Snake.Last();
        Point newSnakeHead = snakeHead;

        switch (SnakeDirection)
        {
            case Direction.Up:
                newSnakeHead = new Point(snakeHead.X, snakeHead.Y - 1);
                break;
            case Direction.Down:
                newSnakeHead = new Point(snakeHead.X, snakeHead.Y + 1);
                break;
            case Direction.Left:
                newSnakeHead = new Point(snakeHead.X - 1, snakeHead.Y);
                break;
            case Direction.Right:
                newSnakeHead = new Point(snakeHead.X + 1, snakeHead.Y);
                break;
            default:
                SnakeDead = true;
                return;
        }

        if (OutOfBounds(newSnakeHead))
        {
            SnakeDead = true;
            return;
        }

        if (newSnakeHead.X == Apple.X && newSnakeHead.Y == Apple.Y)
        {
            MoveApple();
        }
        else
        {
            Snake.RemoveAt(0);
        }
        
        Snake.Add(newSnakeHead);
    }

    public bool OutOfBounds(Point head)
    {
        // check wall collision
        if (head.X < 0 || head.X > 19 || head.Y < 0 || head.Y > 19)
        {
            return true;
        }

        // check self collision
        if (Snake.Contains(head))
        {
            return true;
        }
        return false;
    }
}
