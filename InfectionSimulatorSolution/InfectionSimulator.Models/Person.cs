using InfectionSimulator.Implementation;

namespace InfectionSimulator.Models;

public class Person
{
    public int Id { get; } // unikalny identyfikator agenta
    public Vector2D Position { get; private set; } // pozycja w przestrzeni
    public Vector2D Velocity { get; private set; } // prędkość i kierunek ruchu

    public HealthState Health { get; private set; } = HealthState.Healthy; // stan zdrowia
    public Immunity Immunity { get; private set; } = Immunity.Susceptible; // odporność
    private SymptomState Symptoms { get; set; } = SymptomState.Asymptomatic; // objawy infekcji
    public AgentState State { get; private set; } = AgentState.Moving; // stan agenta (ruch/exit)

    private double _infectionTimer;             // licznik czasu od zarażenia
    private readonly double _infectionDuration; // czas trwania infekcji (sekundy)

    private static readonly Random Rng = new(); // generator losowy dla wszystkich agentów

    public bool IsInfected => Health == HealthState.Infected; // czy agent jest zakażony

    // konstruktor agenta
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

        // losowy czas trwania infekcji 20–30s
        _infectionDuration = 20 + Rng.NextDouble() * 10;
    }

    // aktualizacja pozycji i stanu agenta w kroku dt
    public void Update(double dt)
    {
        if (State == AgentState.Exited) return; // jeśli agent wyszedł, nic nie robimy

        var p = Position.GetComponents();
        var v = Velocity.GetComponents();

        // proste przesunięcie pozycji zgodnie z prędkością
        Position = new Vector2D(
            p[0] + v[0] * dt,
            p[1] + v[1] * dt
        );

        // aktualizacja infekcji
        if (IsInfected)
        {
            _infectionTimer += dt;

            // jeśli czas infekcji minął -> agent wyzdrowiał i staje się odporny
            if (_infectionTimer >= _infectionDuration)
            {
                Health = HealthState.Healthy;
                Immunity = Immunity.Immune;
                Symptoms = SymptomState.Asymptomatic;
            }
        }
    }

    // wprowadzenie drobnych perturbacji kierunku oraz prędkości wektorów
    public void ApplyRandomVelocityPerturbation(double maxSpeed, double maxDelta = 0.2)
    {
        // losowa zmiana x i y w zakresie [-maxDelta, maxDelta]
        double dx = (Rng.NextDouble() * 2 - 1) * maxDelta;
        double dy = (Rng.NextDouble() * 2 - 1) * maxDelta;

        var newVel = new Vector2D(Velocity.X + dx, Velocity.Y + dy);

        // ograniczamy prędkość do maxSpeed
        double speed = newVel.Length();
        if (speed > maxSpeed)
            newVel = newVel.Normalized() * maxSpeed;

        Velocity = newVel;
    }
    
    // zmiana prędkości agenta
    public void SetVelocity(Vector2D vel)
    {
        Velocity = vel;
    }

    // oznaczenie agenta jako wyszedł
    public void MarkExited()
    {
        State = AgentState.Exited;
    }

    // zarażenie od innego agenta
    public void InfectFrom(Person other)
    {
        if (Immunity == Immunity.Immune) return;    // odporny nie zaraża się
        if (IsInfected) return;                     // już zakażony nie przyjmuje ponownie

        double chance = other.Symptoms == SymptomState.Symptomatic ? 1.0 : 0.5;

        if (Rng.NextDouble() < chance)
        {
            Health = HealthState.Infected;
            Symptoms = Rng.NextDouble() < 0.5 ? SymptomState.Asymptomatic : SymptomState.Symptomatic;
            _infectionTimer = 0; // start licznika infekcji
        }
    }

    // utworzenie memento (snapshot) stanu agenta
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
            InfectionRemainingSteps = (int)Math.Round(this._infectionTimer * 25) // konwersja na kroki
        };
    }
}