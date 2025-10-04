using SnakeAI;

namespace Snake.Training;

public sealed class SnakeTraining
{
    private readonly SnakeAgent agent;
    private readonly Mlp qNet;      // online network
    private readonly Mlp targetNet; // target network

    // ---- Hyperparameters ----
    private readonly int actionCount;
    private readonly float gamma = 0.99f;
    private readonly float lr = 1e-3f;

    private readonly float epsilonStart = 1.0f;
    private readonly float epsilonEnd = 0.05f;
    private readonly float epsilonDecayRate = 0.999995f;

    private readonly int replayCapacity = 50_000;
    private readonly int warmupSteps = 5_000;
    private readonly int batchSize = 64;
    private readonly int targetSyncEvery = 2_000;
    private readonly int trainEvery = 4;

    private readonly ReplayBuffer replay;
    private readonly Random rng = new Random(123);
    private float epsilon;
    private int globalStep;

    public SnakeTraining(SnakeAgent agent, Mlp qNet, Mlp targetNet, int actionCount)
    {
        this.agent = agent;
        this.qNet = qNet;
        this.targetNet = targetNet;
        this.actionCount = actionCount;

        epsilon = epsilonStart;
        targetNet.CopyWeightsFrom(qNet);

        replay = new ReplayBuffer(replayCapacity);
    }

    public void Run(int episodes = 1000)
    {
        for (int episode = 0; episode < episodes; episode++)
        {
            var state = agent.Reset();
            bool done = false;

            while (!done)
            {
                int action = DetermineAction(state);
                var (nextState, reward, isDone) = agent.Step(action);

                replay.Add(state, action, reward, nextState, isDone);

                globalStep++;
                EpsilonAnneal();

                if (globalStep > warmupSteps && globalStep % trainEvery == 0)
                    TrainOneBatch();

                if (globalStep % targetSyncEvery == 0)
                    targetNet.CopyWeightsFrom(qNet);

                state = nextState;
                done = isDone;
            }

            if (episode % 50 == 0)
                Console.WriteLine($"Episode {episode} | Steps={globalStep} | ε={epsilon:F3}");
        }
    }

    // ---- ε-greedy policy ----
    private int DetermineAction(float[] state)
    {
        if (rng.NextDouble() < epsilon)
            return rng.Next(actionCount);

        var q = qNet.Predict(state);
        int best = 0;
        float bestQ = q[0];
        for (int i = 1; i < actionCount; i++)
        {
            if (q[i] > bestQ)
            {
                bestQ = q[i];
                best = i;
            }
        }
        return best;
    }

    private void EpsilonAnneal()
    {
        epsilon = MathF.Max(epsilonEnd, epsilon * epsilonDecayRate);
    }

    // ---- Training step ----
    private void TrainOneBatch()
    {
        if (replay.Count < batchSize)
            return;

        Batch batch = replay.Sample(batchSize, rng);

        float[] targets = new float[batchSize];
        for (int i = 0; i < batchSize; i++)
        {
            if (batch.Dones[i])
            {
                targets[i] = batch.Rewards[i];
            }
            else
            {
                var qNext = targetNet.Predict(batch.NextStates[i]);
                float maxNext = qNext[0];
                for (int a = 1; a < actionCount; a++)
                    if (qNext[a] > maxNext)
                        maxNext = qNext[a];

                targets[i] = batch.Rewards[i] + gamma * maxNext;
            }
        }

        qNet.BackwardAndStepBatch(batch, targets, lr, average: true);
    }

    // ---- Replay Buffer ----
    private sealed class ReplayBuffer
    {
        private readonly int capacity;
        private readonly List<float[]> states = new();
        private readonly List<int> actions = new();
        private readonly List<float> rewards = new();
        private readonly List<float[]> nextStates = new();
        private readonly List<bool> dones = new();

        public int Count => states.Count;

        public ReplayBuffer(int capacity)
        {
            this.capacity = capacity;
        }

        public void Add(float[] state, int action, float reward, float[] nextState, bool done)
        {
            if (states.Count >= capacity)
            {
                // Remove oldest
                states.RemoveAt(0);
                actions.RemoveAt(0);
                rewards.RemoveAt(0);
                nextStates.RemoveAt(0);
                dones.RemoveAt(0);
            }

            states.Add((float[])state.Clone());
            actions.Add(action);
            rewards.Add(reward);
            nextStates.Add((float[])nextState.Clone());
            dones.Add(done);
        }

        public Batch Sample(int batchSize, Random rng)
        {
            int n = states.Count;
            float[][] batchStates = new float[batchSize][];
            int[] batchActions = new int[batchSize];
            float[] batchRewards = new float[batchSize];
            float[][] batchNextStates = new float[batchSize][];
            bool[] batchDones = new bool[batchSize];

            for (int i = 0; i < batchSize; i++)
            {
                int idx = rng.Next(n);
                batchStates[i] = states[idx];
                batchActions[i] = actions[idx];
                batchRewards[i] = rewards[idx];
                batchNextStates[i] = nextStates[idx];
                batchDones[i] = dones[idx];
            }

            return new Batch
            {
                States = batchStates,
                Actions = batchActions,
                Rewards = batchRewards,
                NextStates = batchNextStates,
                Dones = batchDones
            };
        }
    }
}