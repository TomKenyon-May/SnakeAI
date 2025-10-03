using SnakeAI;

public class SnakeTraining
{
    private readonly SnakeAgent agent = new SnakeAgent();
    private readonly Random rng = new Random();
    private int stepsSeen = 0;

    public static void Main(string[] args) => new SnakeTraining().Run();

    public void Run()
    {
        for (int episode = 0; episode < 1000; episode++)
        {
            var (state, reward, done) = (agent.Reset(), 0f, false);

            while (!done)
            {
                int action = DetermineAction(state);
                (state, reward, done) = agent.Step(action);
            }

            if (episode % 100 == 0)
            {
                Console.WriteLine($"Episode {episode} completed.");
            }
        }
    }

    private int DetermineAction(float[] state)
    {
        // Placeholder for action determination logic
        return new Random().Next(agent.ActionCount);
    }
}