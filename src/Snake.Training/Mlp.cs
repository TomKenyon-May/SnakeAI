namespace Snake.Training;

public sealed class Mlp
{
    // fields: weights, biases, rng...
    private readonly int InputSize, Hidden1, Hidden2, OutputSize;
    private readonly float[] Weight1, Weight2, Weight3;
    private readonly float[] Bias1, Bias2, Bias3;
    private readonly Random rng;
    private const int FileMagic = 0x4D4C5031; // 'MLP1'
    private const int Version = 1;

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

        Forward(state, out var a1, out var a2, out var q);
        return q;
    }

    public void CopyWeightsFrom(Mlp other)
    {
        if (other.InputSize != InputSize || other.Hidden1 != Hidden1 || other.Hidden2 != Hidden2 || other.OutputSize != OutputSize)
            throw new ArgumentException("Network architecture mismatch.");

        Array.Copy(other.Weight1, Weight1, Weight1.Length);
        Array.Copy(other.Bias1, Bias1, Bias1.Length);
        Array.Copy(other.Weight2, Weight2, Weight2.Length);
        Array.Copy(other.Bias2, Bias2, Bias2.Length);
        Array.Copy(other.Weight3, Weight3, Weight3.Length);
        Array.Copy(other.Bias3, Bias3, Bias3.Length);
    }

    private void Forward(float[] x, out float[] a1, out float[] a2, out float[] q)
    {
        a1 = new float[Hidden1];
        a2 = new float[Hidden2];
        q = new float[OutputSize];

        Relu(Weight1, Hidden1, InputSize, x, Bias1, a1);
        Relu(Weight2, Hidden2, Hidden1, a1, Bias2, a2);
        Linear(Weight3, OutputSize, Hidden2, a2, Bias3, q);
    }

    public void BackwardAndStep(float[] state, int action, float target, float learningRate)
    {
        Forward(state, out var a1, out var a2, out var q);

        var dQ = new float[OutputSize];
        float error = q[action] - target;
        dQ[action] = error;

        var dW3 = new float[Weight3.Length];
        var dB3 = new float[Bias3.Length];
        var dA2 = new float[Hidden2];

        for (int r = 0; r < OutputSize; r++)
        {
            float g = dQ[r];
            dB3[r] += g;
            int off = r * Hidden2;
            for (int c = 0; c < Hidden2; c++)
            {
                dW3[off + c] += g * a2[c];
                dA2[c] += g * Weight3[off + c];
            }
        }

        for (int i = 0; i < Hidden2; i++)
        {
            if (a2[i] <= 0f) dA2[i] = 0f;
        }

        var dW2 = new float[Weight2.Length];
        var dB2 = new float[Bias2.Length];
        var dA1 = new float[Hidden1];

        for (int r = 0; r < Hidden2; r++)
        {
            float g = dA2[r];
            dB2[r] += g;
            int off = r * Hidden1;
            for (int c = 0; c < Hidden1; c++)
            {
                dW2[off + c] += g * a1[c];
                dA1[c] += g * Weight2[off + c];
            }
        }

        for (int i = 0; i < Hidden1; i++)
        {
            if (a1[i] <= 0f) dA1[i] = 0f;
        }

        var dW1 = new float[Weight1.Length];
        var dB1 = new float[Bias1.Length];

        for (int r = 0; r < Hidden1; r++)
        {
            float g = dA1[r];
            dB1[r] += g;
            int off = r * InputSize;
            for (int c = 0; c < InputSize; c++)
            {
                dW1[off + c] += g * state[c];
            }
        }

        for (int i = 0; i < Weight3.Length; i++)
            Weight3[i] -= learningRate * dW3[i];

        for (int i = 0; i < Bias3.Length; i++)
            Bias3[i] -= learningRate * dB3[i];

        for (int i = 0; i < Weight2.Length; i++)
            Weight2[i] -= learningRate * dW2[i];

        for (int i = 0; i < Bias2.Length; i++)
            Bias2[i] -= learningRate * dB2[i];

        for (int i = 0; i < Weight1.Length; i++)
            Weight1[i] -= learningRate * dW1[i];

        for (int i = 0; i < Bias1.Length; i++)
            Bias1[i] -= learningRate * dB1[i];
    }

    public void BackwardAndStepBatch(Batch batch, float[] targets, float lr)
    {
        var dW3 = new float[Weight3.Length]; var dB3 = new float[Bias3.Length];
        var dW2 = new float[Weight2.Length]; var dB2 = new float[Bias2.Length];
        var dW1 = new float[Weight1.Length]; var dB1 = new float[Bias1.Length];

        int B = batch.Actions.Length;

        for (int n = 0; n < B; n++)
        {
            var x = batch.States[n];
            int a = batch.Actions[n];
            float t = targets[n];

            Forward(x, out var a1, out var a2, out var q);

            var dQ = new float[OutputSize];
            dQ[a] = q[a] - t;

            var dA2 = new float[Hidden2];
            for (int r = 0; r < OutputSize; r++)
            {
                float g = dQ[r];
                dB3[r] += g;
                int off = r * Hidden2;
                for (int c = 0; c < Hidden2; c++)
                {
                    dW3[off + c] += g * a2[c];
                    dA2[c] += g * Weight3[off + c];
                }
            }
            for (int i = 0; i < Hidden2; i++) if (a2[i] <= 0f) dA2[i] = 0f;

            var dA1 = new float[Hidden1];
            for (int r = 0; r < Hidden2; r++)
            {
                float g = dA2[r];
                dB2[r] += g;
                int off = r * Hidden1;
                for (int c = 0; c < Hidden1; c++)
                {
                    dW2[off + c] += g * a1[c];
                    dA1[c] += g * Weight2[off + c];
                }
            }
            for (int i = 0; i < Hidden1; i++) if (a1[i] <= 0f) dA1[i] = 0f;

            for (int r = 0; r < Hidden1; r++)
            {
                float g = dA1[r];
                dB1[r] += g;
                int off = r * InputSize;
                for (int c = 0; c < InputSize; c++)
                    dW1[off + c] += g * x[c];
            }
        }

        for (int i = 0; i < Weight3.Length; i++) Weight3[i] -= lr * dW3[i];
        for (int i = 0; i < Bias3.Length; i++) Bias3[i] -= lr * dB3[i];

        for (int i = 0; i < Weight2.Length; i++) Weight2[i] -= lr * dW2[i];
        for (int i = 0; i < Bias2.Length; i++) Bias2[i] -= lr * dB2[i];

        for (int i = 0; i < Weight1.Length; i++) Weight1[i] -= lr * dW1[i];
        for (int i = 0; i < Bias1.Length; i++) Bias1[i] -= lr * dB1[i];
    }

    public void Save(string path)
    {
        using var fs = File.Create(path);
        using var bw = new BinaryWriter(fs);

        bw.Write(FileMagic);
        bw.Write(Version);

        bw.Write(InputSize);
        bw.Write(Hidden1);
        bw.Write(Hidden2);
        bw.Write(OutputSize);

        WriteArray(bw, Weight1);
        WriteArray(bw, Bias1);
        WriteArray(bw, Weight2);
        WriteArray(bw, Bias2);
        WriteArray(bw, Weight3);
        WriteArray(bw, Bias3);
    }

    public void Load(string path)
    {
        using var fs = File.OpenRead(path);
        using var br = new BinaryReader(fs);

        int magic = br.ReadInt32();
        if (magic != FileMagic) throw new InvalidDataException("Invalid file format.");

        int version = br.ReadInt32();
        if (version != Version) throw new InvalidDataException($"Unsupported version: {version}");

        int inSize = br.ReadInt32();
        int h1Size = br.ReadInt32();
        int h2Size = br.ReadInt32();
        int outSize = br.ReadInt32();

        if (inSize != InputSize || h1Size != Hidden1 || h2Size != Hidden2 || outSize != OutputSize)
            throw new InvalidDataException("Network architecture mismatch.");

        ReadArray(br, Weight1);
        ReadArray(br, Bias1);
        ReadArray(br, Weight2);
        ReadArray(br, Bias2);
        ReadArray(br, Weight3);
        ReadArray(br, Bias3);
    }

    private void WriteArray(BinaryWriter bw, float[] array)
    {
        foreach (var v in array)
            bw.Write(v);
    }
    
    private void ReadArray(BinaryReader br, float[] array)
    {
        for (int i = 0; i < array.Length; i++)
            array[i] = br.ReadSingle();
    }
}
