using InfectionSimulator.Implementation;

namespace InfectionSimulator.Models;

public class Person
{
    public int Id { get; } // unique agent identifier
    public Vector2D Position { get; private set; } // position in space
    public Vector2D Velocity { get; private set; } // speed and direction of movement

    public HealthState Health { get; private set; } = HealthState.Healthy; // health condition
    public Immunity Immunity { get; private set; } = Immunity.Susceptible; // resistance
    private SymptomState Symptoms { get; set; } = SymptomState.Asymptomatic; // symptoms of infection
    public AgentState State { get; private set; } = AgentState.Moving; // agent state (move/exit)

    private double _infectionTimer;             // infection time counter
    private readonly double _infectionDuration; // duration of infection (seconds)

    private static readonly Random Rng = new(); // random generator for all agents

    public bool IsInfected => Health == HealthState.Infected; // whether the agent is infected

    // agent constructor
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

        // random infection duration 20–30s
        _infectionDuration = 20 + Rng.NextDouble() * 10;
    }

    // updating the agent's position and status in step dt
    public void Update(double dt)
    {
        if (State == AgentState.Exited) return; // if the agent left, we do nothing

        var p = Position.GetComponents();
        var v = Velocity.GetComponents();

        // simple position shift according to speed
        Position = new Vector2D(
            p[0] + v[0] * dt,
            p[1] + v[1] * dt
        );

        // infection update
        if (IsInfected)
        {
            _infectionTimer += dt;

            // if the infection time has passed -> the agent has recovered and becomes immune
            if (_infectionTimer >= _infectionDuration)
            {
                Health = HealthState.Healthy;
                Immunity = Immunity.Immune;
                Symptoms = SymptomState.Asymptomatic;
            }
        }
    }

    // introducing minor perturbations of the direction and speed of the vectors
    public void ApplyRandomVelocityPerturbation(double maxSpeed, double maxDelta = 0.2)
    {
        // random change of x and y in the range [-maxDelta, maxDelta]
        double dx = (Rng.NextDouble() * 2 - 1) * maxDelta;
        double dy = (Rng.NextDouble() * 2 - 1) * maxDelta;

        var newVel = new Vector2D(Velocity.X + dx, Velocity.Y + dy);

        // we limit the speed to maxSpeed
        double speed = newVel.Length();
        if (speed > maxSpeed)
            newVel = newVel.Normalized() * maxSpeed;

        Velocity = newVel;
    }

    // agent speed change
    public void SetVelocity(Vector2D vel)
    {
        Velocity = vel;
    }

    // marking the agent as exited
    public void MarkExited()
    {
        State = AgentState.Exited;
    }

    // infection from another agent
    public void InfectFrom(Person other)
    {
        if (Immunity == Immunity.Immune) return;    // immune does not get infected
        if (IsInfected) return;                     // already infected do not accept again

        double chance = other.Symptoms == SymptomState.Symptomatic ? 1.0 : 0.5;

        if (Rng.NextDouble() < chance)
        {
            Health = HealthState.Infected;
            Symptoms = Rng.NextDouble() < 0.5 ? SymptomState.Asymptomatic : SymptomState.Symptomatic;
            _infectionTimer = 0; // infection counter start
        }
    }

    // creating a memento (snapshot) of the agent's state
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
            InfectionRemainingSteps = (int)Math.Round(this._infectionTimer * 25) // conversion to steps
        };
    }
}