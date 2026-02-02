using InfectionSimulator.Models;
using InfectionSimulator.Services;
using InfectionSimulator.Simulation;

namespace InfectionSimulator.App;

internal static class Program
{
    static void Main()
    {
        // dimensions of the simulation field (meters)
        double width = 30;
        double height = 30;
        double maxSpeed = 2.5; // maximum agent speed (m/s)

        // scenario selection menu
        Console.WriteLine("Select a scenario:");
        Console.WriteLine("1 — Normal day");
        Console.WriteLine("2 — Post-epidemic world");
        int choice = int.Parse(Console.ReadLine() ?? "1");

        //starting parameters of the population
        double infectChance;    // probability of being infected at the start
        double immunityRatio;   // percentage of people already immune

        if (choice == 2) // post-epidemic world
        {
            immunityRatio = 0.7;    // 70% of the population is immune
            infectChance = 0.03;    // 3% infected at the start
        }
        else
        {
            immunityRatio = 0.0;    // normal day - no immunity
            infectChance = 0.1;     // 10% infected at the start
        }

        // simulator initialization
        var sim = new Simulator(width, height, maxSpeed);
        sim.SeedInitialPopulation(50, immunityRatio, infectChance); // seeding the population

        int totalSteps = 25 * 60; // number of simulation steps (1 minute, 25 steps/s)
        for (int step = 0; step < totalSteps; step++)
        {
            sim.Step(); // execution of the simulation step

            // every 1s (25 steps) print a summary of the population status
            if (step % 25 == 0)
            {
                var agents = sim.Agents;
                int healthy = 0, infected = 0, immune = 0, exited = 0;

                foreach (var a in agents)
                {
                    if (a.State == AgentState.Exited) { exited++; continue; }
                    if (a.Immunity == Immunity.Immune) immune++;
                    if (a.Health == HealthState.Infected) infected++;
                    if (a is { Health: HealthState.Healthy, Immunity: Immunity.Susceptible }) healthy++;
                }

                int remaining = agents.Count(a => a.State != AgentState.Exited);

                Console.WriteLine($"t={step/25}s: remaining={remaining} total={agents.Count} healthy={healthy} infected={infected} immune={immune} exited={exited}");
            }
        }

        // creating a snapshot of the entire population at the end of the simulation
        var memos = new List<PersonMemento>();
        foreach (var a in sim.Agents)
            memos.Add(a.CreateMemento());

        // saving the snapshot to a JSON file
        string path = Path.Combine(Environment.CurrentDirectory, "snapshot.json");
        SnapshotManager.SaveSnapshot(path, memos);
        Console.WriteLine($"Snapshot saved to {path}");
    }
}