using Snake.Core;

namespace SnakeAI;

public sealed class SnakeAgent
{
    public SnakeGame Game { get; private set; } = new SnakeGame();
    private float gridCellSize = 20f;
    public readonly int StateDim = 406; // 400 for grid + 2 for apple pos + 4 for direction
    public int ActionCount => 3;
    private int StepCount = 0;

    public float[] Reset()
    {
        Game = new SnakeGame();
        StepCount = 0;
        return EncodeState();
    }

    private float[] EncodeState()
    {
        var state = new float[StateDim];

        for (int y = 0; y < (int)gridCellSize; y++)
        {
            for (int x = 0; x < (int)gridCellSize; x++)
            {
                state[y * (int)gridCellSize + x] = 0f;
            }
        }

        foreach (var segment in Game.Snake)
        {
            state[segment.Y * (int)gridCellSize + segment.X] = 1f;
        }

        // Normalize apple position to [0, 1]
        state[400] = Game.Apple.X / (gridCellSize - 1);
        state[401] = Game.Apple.Y / (gridCellSize - 1);

        // Direction one-hot encoding [Up, Right, Down, Left]
        switch (Game.SnakeDirection)
        {
            case Direction.Up:
                state[402] = 1f;
                break;
            case Direction.Right:
                state[403] = 1f;
                break;
            case Direction.Down:
                state[404] = 1f;
                break;
            case Direction.Left:
                state[405] = 1f;
                break;
        }

        return state;
    }

    private void ApplyAction(int action)
    {
        // Action: 0 = turn left, 1 = straight, 2 = turn right
        switch (action)
        {
            case 0:
                Game.SnakeDirection = (Direction)(((int)Game.SnakeDirection + 3) % 4);
                break;
            case 1:
                break;
            case 2:
                Game.SnakeDirection = (Direction)(((int)Game.SnakeDirection + 1) % 4);
                break;
            default:
                break;
        }
    }

    private float CalculateReward(int lenBefore, int distBefore)
    {
        if (Game.SnakeDead)
            return -1f;

        if (Game.Snake.Count > lenBefore)
            return 1f;
        
        var head = Game.Snake[^1];
        int distAfter = Math.Abs(head.X - Game.Apple.X) + Math.Abs(head.Y - Game.Apple.Y);

        float reward = -0.01f;

        if (distAfter < distBefore)
            reward += 0.1f;
        else if (distAfter > distBefore)
            reward -= 0.1f;

        return reward;
    }

    public (float[] nextState, float reward, bool done) Step(int action)
    {
        int lenBefore = Game.Snake.Count;
        int distBefore = Math.Abs(Game.Snake[^1].X - Game.Apple.X) + Math.Abs(Game.Snake[^1].Y - Game.Apple.Y);
        ApplyAction(action);
        Game.MoveSnake();
        StepCount++;

        var nextState = EncodeState();
        var reward = CalculateReward(lenBefore, distBefore);
        var done = Game.SnakeDead || StepCount > (200 + Game.Snake.Count * 5); // Prevent stalling
        return (nextState, reward, done);
    }
}
