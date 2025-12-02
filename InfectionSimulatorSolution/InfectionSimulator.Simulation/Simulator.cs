using InfectionSimulator.Models;
using InfectionSimulator.Implementation;

namespace InfectionSimulator.Simulation;

public class Simulator(double width, double height, double maxSpeed)
{
    public double Width { get; } = width;   // szerokość pola symulacji
    public double Height { get; } = height; // wysokość pola symulacji

    private readonly Random _rng = new();          // generator losowy
    private readonly List<Person> _agents = new(); // lista wszystkich agentów
    private int _nextId = 1;                       // ID kolejnego agenta

    private const double InfectDistance = 2.0;     // dystans w którym może nastąpić infekcja
    private const double InfectTime = 3.0;         // czas kontaktu wymagany do zarażenia

    private readonly Dictionary<(int, int), double> _proximityTimers = new(); // czas kontaktu między parami agentów

    private readonly int _targetPopulation = 50;   // docelowa liczebność populacji

    public IReadOnlyList<Person> Agents => _agents;

    // zasianie populacji startowej
    public void SeedInitialPopulation(int count, double immunityRatio, double infectionChanceForSusceptible)
    {
        for (int i = 0; i < count; i++)
        {
            bool immune = _rng.NextDouble() < immunityRatio;    // losowanie odporności
            SpawnPerson(immune, infectionChanceForSusceptible); // utworzenie agenta
        }
    }

    // utworzenie nowego agenta
    private void SpawnPerson(bool immune, double infectionChance)
    {
        // losowy spawn na granicy pola
        int edge = _rng.Next(4);
        double x = 0, y = 0;
        switch (edge)
        {
            case 0: x = 0; y = _rng.NextDouble() * Height; break;
            case 1: x = Width; y = _rng.NextDouble() * Height; break;
            case 2: x = _rng.NextDouble() * Width; y = 0; break;
            case 3: x = _rng.NextDouble() * Width; y = Height; break;
        }

        var pos = new Vector2D(x, y);

        // kierunek ruchu w stronę środka pola
        double cx = Width / 2.0;
        double cy = Height / 2.0;
        double angle = Math.Atan2(cy - y, cx - x);

        // losowa prędkość (0.2 -> maxSpeed)
        double speed = 0.2 + _rng.NextDouble() * (maxSpeed - 0.2);
        var vel = new Vector2D(Math.Cos(angle) * speed, Math.Sin(angle) * speed);

        // czy agent jest zakażony na starcie
        bool infected = !immune && _rng.NextDouble() < infectionChance;

        var p = new Person(_nextId++, pos, vel, immune, infected);
        _agents.Add(p);
    }

    // wykonanie jednego kroku symulacji
    public void Step(double dt = 0.04)
    {
        foreach (var p in _agents.ToList())
        {
            if (p.State != AgentState.Exited)
                p.Update(dt); // aktualizacja pozycji i stanu

            HandleBorders(p); // sprawdzenie kolizji z granicami
        }

        MaintainPopulation();  // utrzymanie populacji na stałym poziomie
        HandleInfections(dt);  // obsługa infekcji
    }

    // dopilnowanie liczebności populacji
    private void MaintainPopulation()
    {
        int alive = _agents.Count(a => a.State != AgentState.Exited);
        while (alive < _targetPopulation)
        {
            SpawnPerson(immune: false, infectionChance: 0.1);
            alive++;
        }
    }

    // obsługa kolizji z granicami pola
    private void HandleBorders(Person p)
    {
        if (p.State == AgentState.Exited) return;

        var pos = p.Position.GetComponents();
        double x = pos[0];
        double y = pos[1];

        bool hit = x <= 0 || x >= Width || y <= 0 || y >= Height;
        if (!hit) return;

        if (_rng.NextDouble() < 0.5) // odbicie od ściany
        {
            var vel = p.Velocity.GetComponents();
            double vx = vel[0];
            double vy = vel[1];

            if (x <= 0 || x >= Width) vx = -vx;
            if (y <= 0 || y >= Height) vy = -vy;

            p.SetVelocity(new Vector2D(vx, vy));
        }
        else
        {
            p.MarkExited(); // agent wychodzi z pola
        }
    }

    // obsługa infekcji między agentami
    private void HandleInfections(double dt)
    {
        for (int i = 0; i < _agents.Count; i++)
        {
            for (int j = i + 1; j < _agents.Count; j++)
            {
                var a = _agents[i];
                var b = _agents[j];

                if (a.State == AgentState.Exited || b.State == AgentState.Exited)
                    continue;

                var ap = a.Position.GetComponents();
                var bp = b.Position.GetComponents();

                double dx = ap[0] - bp[0];
                double dy = ap[1] - bp[1];

                double dist = Math.Sqrt(dx * dx + dy * dy);
                var key = (Math.Min(a.Id, b.Id), Math.Max(a.Id, b.Id));

                if (dist <= InfectDistance)
                {
                    _proximityTimers.TryAdd(key, 0);
                    _proximityTimers[key] += dt;

                    // jeśli czas kontaktu wystarczający -> infekcja
                    if (_proximityTimers[key] >= InfectTime)
                    {
                        if (a.IsInfected) b.InfectFrom(a);
                        if (b.IsInfected) a.InfectFrom(b);
                    }
                }
                else
                {
                    _proximityTimers.Remove(key); // zresetuj timer jeśli zbyt daleko
                }
            }
        }
    }
}