using InfectionSimulator.Models;
using InfectionSimulator.Services;
using InfectionSimulator.Simulation;

namespace InfectionSimulator.App;

internal static class Program
{
    static void Main()
    {
        // wymiary pola symulacji (metry)
        double width = 30;
        double height = 30;
        double maxSpeed = 2.5; // maksymalna prędkość agenta (m/s)

        // menu wyboru scenariusza
        Console.WriteLine("Select scenario:");
        Console.WriteLine("1 — Normal day");
        Console.WriteLine("2 — Post-epidemic world");
        int choice = int.Parse(Console.ReadLine() ?? "1");

        // parametry startowe populacji
        double infectChance;    // prawdopodobieństwo bycia zakażonym na starcie
        double immunityRatio;   // odsetek osób już odpornych

        if (choice == 2) // post-epidemic world
        {
            immunityRatio = 0.7;    // 70% populacji odpornej
            infectChance = 0.03;    // 3% zakażonych na starcie
        }
        else
        {
            immunityRatio = 0.0;    // normal day — brak odporności
            infectChance = 0.1;     // 10% zakażonych na starcie
        }

        // inicjalizacja symulatora
        var sim = new Simulator(width, height, maxSpeed);
        sim.SeedInitialPopulation(50, immunityRatio, infectChance); // zasianie populacji

        int totalSteps = 25 * 60; // liczba kroków symulacji (1 minuta, 25 kroków/s)
        for (int step = 0; step < totalSteps; step++)
        {
            sim.Step(); // wykonanie kroku symulacji

            // co 1s (25 kroków) wypisz podsumowanie stanu populacji
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

        // utworzenie snapshotu całej populacji na koniec symulacji
        var memos = new List<PersonMemento>();
        foreach (var a in sim.Agents)
            memos.Add(a.CreateMemento());

        // zapis snapshotu do pliku JSON
        string path = Path.Combine(Environment.CurrentDirectory, "snapshot.json");
        SnapshotManager.SaveSnapshot(path, memos);
        Console.WriteLine($"Snapshot saved to {path}");
    }
}