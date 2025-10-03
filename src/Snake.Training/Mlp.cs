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

        Weight1 = new float[Hidden1 * inputSize];
        Bias1 = new float[Hidden1];

        Weight2 = new float[Hidden2 * Hidden1];
        Bias2 = new float[Hidden2];

        Weight3 = new float[OutputSize * Hidden2];
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

    private static void Linear(float[] W, int rows, int cols, float[] x, float[] b, float[] y)
    {
        for (int r = 0; r < rows; r++)
        {
            float s = b[r];
            int off = r * cols;
            for (int c = 0; c < cols; c++)
                s += W[off + c] * x[c];
            y[r] = s;
        }
    }

    private static void Relu(float[] W, int rows, int cols, float[] x, float[] b, float[] y)
    {
        // 1. Do the linear transform into y
        Linear(W, rows, cols, x, b, y);

        // 2. Apply ReLU in-place
        for (int i = 0; i < y.Length; i++)
        {
            if (y[i] < 0f) y[i] = 0f;
        }
    }

    public float[] Predict(float[] state)
    {
        if (state.Length != InputSize)
            throw new ArgumentException($"Expected state length {InputSize}, got {state.Length}");

        var a1 = new float[Hidden1];
        var a2 = new float[Hidden2];
        var q = new float[OutputSize];

        Relu(Weight1, Hidden1, InputSize, state, Bias1, a1);
        Relu(Weight2, Hidden2, Hidden1, a1, Bias2, a2);
        Linear(Weight3, OutputSize, Hidden2, a2, Bias3, q);

        return q;
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
