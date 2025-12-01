using InfectionSimulator.Implementation;

namespace InfectionSimulator.Models;

public class Person
{
    public int Id { get; }
    public Vector2D Position { get; private set; }
    public Vector2D Velocity { get; private set; }

    public HealthState Health { get; private set; } = HealthState.Healthy;
    public Immunity Immunity { get; private set; } = Immunity.Susceptible;
    public SymptomState Symptoms { get; private set; } = SymptomState.Asymptomatic;
    public AgentState State { get; private set; } = AgentState.Moving;

    private double _infectionTimer;
    private readonly double _infectionDuration;

    private static readonly Random Rng = new();

    public bool IsInfected => Health == HealthState.Infected;

    public Person(int id, Vector2D pos, Vector2D vel, bool immune = false, bool infected = false)
    {
        Id = id;
        Position = pos;
        Velocity = vel;

        if (immune)
        {
            Immunity = Immunity.Immune;
            Health = HealthState.Healthy;
            Symptoms = SymptomState.Asymptomatic;
        }
        else if (infected)
        {
            Health = HealthState.Infected;
            Immunity = Immunity.Susceptible;
            Symptoms = Rng.NextDouble() < 0.5 ? SymptomState.Asymptomatic : SymptomState.Symptomatic;
        }

        _infectionDuration = 20 + Rng.NextDouble() * 10; // sekundy
    }

    public void Update(double dt)
    {
        if (State == AgentState.Exited) return;

        var p = Position.GetComponents();
        var v = Velocity.GetComponents();
        Position = new Vector2D(
            p[0] + v[0] * dt,
            p[1] + v[1] * dt
        );

        if (IsInfected)
        {
            _infectionTimer += dt;

            if (_infectionTimer >= _infectionDuration)
            {
                // wyzdrowiał → staje się odporny
                Health = HealthState.Healthy;
                Immunity = Immunity.Immune;
                Symptoms = SymptomState.Asymptomatic;
            }
        }
    }

    public void SetVelocity(Vector2D vel)
    {
        Velocity = vel;
    }

    public void MarkExited()
    {
        State = AgentState.Exited;
    }

    public void InfectFrom(Person other)
    {
        if (Immunity == Immunity.Immune) return;
        if (IsInfected) return;

        double chance = other.Symptoms == SymptomState.Symptomatic ? 1.0 : 0.5;

        if (Rng.NextDouble() < chance)
        {
            Health = HealthState.Infected;
            Symptoms = Rng.NextDouble() < 0.5 ? SymptomState.Asymptomatic : SymptomState.Symptomatic;
            _infectionTimer = 0;
        }
    }

    public PersonMemento CreateMemento()
    {
        var p = Position.GetComponents();
        var v = Velocity.GetComponents();

        return new PersonMemento
        {
            Id = this.Id,
            PosX = p[0],
            PosY = p[1],
            VelX = v[0],
            VelY = v[1],
            Immunity = this.Immunity,
            Health = this.Health,
            Symptom = this.Symptoms,
            State = this.State,
            InfectionRemainingSteps = (int)Math.Round(this._infectionTimer * 25)
        };
    }
    
    public static Person FromMemento(PersonMemento m)
    {
        var p = new Person(
            m.Id,
            new Vector2D(m.PosX, m.PosY),
            new Vector2D(m.VelX, m.VelY)
        )
        {
            Immunity = m.Immunity,
            Health = m.Health,
            Symptoms = m.Symptom ?? SymptomState.Asymptomatic,
            State = m.State
        };

        // odtwórz timer infekcji
        p.SetInfectionProgress(m.InfectionRemainingSteps / 25.0);

        return p;
    }

    public void SetInfectionProgress(double seconds)
    {
        _infectionTimer = seconds;
    }
}