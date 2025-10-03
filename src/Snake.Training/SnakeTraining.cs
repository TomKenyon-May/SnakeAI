using SnakeAI;

namespace Snake.Training;

public class SnakeTraining
{
    private readonly SnakeAgent agent = new SnakeAgent();
    private readonly Random rng = new Random();
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
        return rng.Next(agent.ActionCount);
    }
}
