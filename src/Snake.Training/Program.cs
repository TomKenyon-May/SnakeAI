using System;
using SnakeAI;

namespace Snake.Training;

public static class Program
{
    public static void Main(string[] args)
    {
        Console.WriteLine("=== Snake AI Training ===");

        // ---- Network configuration ----
        int inputSize = 406;
        int hidden1 = 128;
        int hidden2 = 128;
        int outputSize = 3;

        // ---- Initialize components ----
        var agent = new SnakeAgent();
        var qNet = new Mlp(inputSize, hidden1, hidden2, outputSize);
        var targetNet = new Mlp(inputSize, hidden1, hidden2, outputSize);

        string savePath = "snake_model.mlp";
        //try load existing model
        if (File.Exists(savePath))
        {
            Console.WriteLine($"Loading existing model from '{savePath}'...");
            qNet.Load(savePath);
            targetNet.CopyWeightsFrom(qNet);
        }
        else
        {
            Console.WriteLine("No existing model found. A new model will be created.");
        }

        // ---- Create trainer ----
        var trainer = new SnakeTraining(agent, qNet, targetNet, outputSize);

        // ---- Training parameters ----
        int episodes = 1000;
        if (args.Length > 0 && int.TryParse(args[0], out int userEpisodes))
            episodes = userEpisodes;

        Console.WriteLine($"Starting training for {episodes} episodes...\n");

        // ---- Run training ----
        trainer.Run(episodes);

        // ---- Save trained model ----
        qNet.Save(savePath);

        Console.WriteLine($"\nTraining complete. Model saved to '{savePath}'.");
        Console.WriteLine("Press any key to exit...");
        Console.ReadKey();
    }
}