using InfectionSimulator.Models;

namespace InfectionSimulator.Services;

public static class SimulationStatsCalc
{
    public static SimulationStats CalculateFromSnapshot(IEnumerable<PersonMemento> snapshot)
    {
        var list = snapshot.ToList();

        return new SimulationStats
        {
            Total = list.Count,
            Healthy = list.Count(p => p.Health == HealthState.Healthy && p.Immunity == Immunity.Susceptible && p.State != AgentState.Exited),
            Infected = list.Count(p => p.Health == HealthState.Infected),
            Immune = list.Count(p => p.Immunity == Immunity.Immune),
            Exited = list.Count(p => p.State == AgentState.Exited)
        };
    }
}