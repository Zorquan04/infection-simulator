namespace InfectionSimulator.Models;

public class SimulationStats
{
    public int Total { get; set; }
    public int Healthy { get; set; }
    public int Infected { get; set; }
    public int Immune { get; set; }
    public int Exited { get; set; }
}