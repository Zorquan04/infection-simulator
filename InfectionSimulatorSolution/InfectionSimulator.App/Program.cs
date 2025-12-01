using InfectionSimulator.Models;
using InfectionSimulator.Services;
using InfectionSimulator.Simulation;

namespace InfectionSimulator.App;

internal static class Program
{
    static void Main()
    {
        double width = 40; // m
        double height = 40; // m
        double maxSpeed = 0.4; // m/s - zmiana z 2.5 dla lepszej symulacji

        var sim = new Simulator(width, height, maxSpeed);
        sim.SeedInitialPopulation(50, initialHasImmunity: false, infectionChanceForNew: 0.1);

        int totalSteps = 25 * 60; // 1 minuta symulacji
        for (int step = 0; step < totalSteps; step++)
        {
            sim.Step();

            if (step % 25 == 0) // co 1s wypisuj podsumowanie
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
                Console.WriteLine($"t={step/25}s: total={agents.Count} healthy={healthy} infected={infected} immune={immune} exited={exited}");
            }
        }

        // przykładowy snapshot
        var memos = new List<PersonMemento>();
        foreach (var a in sim.Agents)
            memos.Add(a.CreateMemento());

        string path = Path.Combine(Environment.CurrentDirectory, "snapshot.json");
        SnapshotManager.SaveSnapshot(path, memos);
        Console.WriteLine($"Snapshot saved to {path}");
    }
}