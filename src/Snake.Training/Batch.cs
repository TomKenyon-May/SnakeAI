namespace Snake.Training;

public sealed class Batch
    {
        public float[][] States { get; init; } = default!;
        public int[] Actions { get; init; } = default!;
        public float[] Rewards { get; init; } = default!;
        public float[][] NextStates { get; init; } = default!;
        public bool[] Dones { get; init; } = default!;
    }
