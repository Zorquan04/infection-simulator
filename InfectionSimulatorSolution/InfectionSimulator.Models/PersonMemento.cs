namespace InfectionSimulator.Models;

public class PersonMemento
{
    public int Id { get; set; }
    public double PosX { get; set; }
    public double PosY { get; set; }
    public double VelX { get; set; }
    public double VelY { get; set; }
    public Immunity Immunity { get; set; }
    public HealthState Health { get; set; }
    public SymptomState? Symptom { get; set; }
    public AgentState State { get; set; }
    public int InfectionRemainingSteps { get; set; }
}