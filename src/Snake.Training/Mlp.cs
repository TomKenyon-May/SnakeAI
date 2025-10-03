using System.Runtime.CompilerServices;
using System.Threading.Tasks.Dataflow;

namespace Snake.Training;

public sealed class Mlp
{
    // fields: weights, biases, rng...
    private readonly int InputSize, Hidden1, Hidden2, OutputSize;
    private readonly float[] Weight1, Weight2, Weight3;
    private readonly float[] Bias1, Bias2, Bias3;
    private readonly Random rng;
    public Mlp(int inputSize, int hidden1, int hidden2, int outputSize)
    {
        // Initialize the MLP with given sizes
        InputSize = inputSize;
        Hidden1 = hidden1;
        Hidden2 = hidden2;
        OutputSize = outputSize;

        rng = new Random();

        Weight1 = new float[inputSize * Hidden1];
        Bias1 = new float[Hidden1];

        Weight2 = new float[Hidden1 * Hidden2];
        Bias2 = new float[Hidden2];

        Weight3 = new float[Hidden2 * OutputSize];
        Bias3 = new float[OutputSize];

        HeInitUniform(Weight1, fanIn: InputSize);
        HeInitUniform(Weight2, fanIn: Hidden1);
        HeInitUniform(Weight3, fanIn: Hidden2);
    }

    private void HeInitUniform(float[] weights, int fanIn)
    {
        float limit = (float)Math.Sqrt(6.0 / fanIn);
        for (int i = 0; i < weights.Length; i++)
        {
            weights[i] = (float)NextUniform(-limit, limit);
        }
    }

    private double NextUniform(double minValue, double maxValue)
    {
        return minValue + (maxValue - minValue) * rng.NextDouble();
    }

    private static int Index(int row, int col, int cols) => row * cols + col;

    public float[] Predict(float[] state)
    {
        // forward: x -> a1 -> a2 -> q
        // return q
        return new float[0];
    }

    public void TrainStep(Batch batch, float gamma, float learningRate)
    {
        // Perform a training step using the provided batch
    }

    public void Save(string path)
    {
        // Save model parameters to the specified path
    }

    public void Load(string path)
    {
        // Load model parameters from the specified path
    }

    public sealed class Batch
    {
        public float[][] States { get; init; } = default!;
        public int[] Actions { get; init; } = default!;
        public float[] Rewards { get; init; } = default!;
        public float[][] NextStates { get; init; } = default!;
        public bool[] Dones { get; init; } = default!;
    }
}
