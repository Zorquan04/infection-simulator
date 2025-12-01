using System.Text;
using InfectionSimulator.Models;
using InfectionSimulator.Implementation;

namespace InfectionSimulator.Simulation;

public class Simulator(double width, double height, double maxSpeed)
{
    public double Width { get; } = width;
    public double Height { get; } = height;
    private readonly double _entryCooldown = 2.0;   // min. 2 sek przerwy na wejścia
    private double _entryTimer;
    private readonly double _spawnRate = 0.2; 
    private readonly Random _rng = new();
    private readonly List<Person> _agents = new();

    private int _nextId = 1;

    private const double InfectDistance = 3.0; // zmiana z 2m dla lepszej symulacji
    private const double InfectTime = 3.0;

    private readonly Dictionary<(int, int), double> _proximityTimers = new();

    public IReadOnlyList<Person> Agents => _agents;

    public void SeedInitialPopulation(int count, bool initialHasImmunity, double infectionChanceForNew)
    {
        for (int i = 0; i < count; i++)
        {
            SpawnPerson(initialHasImmunity, infectionChanceForNew);
        }
    }

    public void LoadPopulation(List<PersonMemento> memos)
    {
        _agents.Clear();
        foreach (var m in memos)
        {
            var p = Person.FromMemento(m);
            _agents.Add(p);
        }

        // ustaw NextId wyżej niż max z snapshotu
        if (_agents.Count > 0)
            _nextId = _agents.Max(a => a.Id) + 1;
    }
    
    private void SpawnPerson(bool forceImmunity, double infectionChance)
    {
        // wybór jednej z 4 krawędzi
        int edge = _rng.Next(4);
        double x = 0, y = 0;

        switch (edge)
        {
            case 0: // lewa
                x = 0;
                y = _rng.NextDouble() * Height;
                break;
            case 1: // prawa
                x = Width;
                y = _rng.NextDouble() * Height;
                break;
            case 2: // góra
                x = _rng.NextDouble() * Width;
                y = 0;
                break;
            case 3: // dół
                x = _rng.NextDouble() * Width;
                y = Height;
                break;
        }

        var pos = new Vector2D(x, y);

        // kierunek DO środka
        double cx = Width / 2.0;
        double cy = Height / 2.0;

        double angle = Math.Atan2(cy - y, cx - x);

        double speed = 0.2 + _rng.NextDouble() * (maxSpeed - 0.2);
        var vel = new Vector2D(Math.Cos(angle) * speed, Math.Sin(angle) * speed);

        bool immune = forceImmunity;
        bool infected = !immune && _rng.NextDouble() < infectionChance;

        var p = new Person(_nextId++, pos, vel, immune, infected);
        _agents.Add(p);
    }

    public void Step()
    {
        double dt = 0.04; // 25 kroków/s

        // spawn ciągły
        _entryTimer += dt;
        if (_entryTimer >= _entryCooldown)
        {
            if (_rng.NextDouble() < _spawnRate * dt)
            {
                SpawnPerson(forceImmunity: false, infectionChance: 0.1);
                _entryTimer = 0;
            }
        }
        
        // aktualizuj pozycje i sanity checks granic
        foreach (var p in _agents.ToList())
        {
            if (p.State != AgentState.Exited)
                p.Update(dt);

            HandleBorders(p);
        }

        // zajmij się infekcjami
        HandleInfections(dt);
    }

    private void HandleBorders(Person p)
    {
        if (p.State == AgentState.Exited) return;

        var pos = p.Position.GetComponents();
        double x = pos[0];
        double y = pos[1];

        bool hit = x <= 0 || x >= Width || y <= 0 || y >= Height;

        if (!hit) return;

        if (_rng.NextDouble() < 0.5)
        {
            // odbicie
            var vel = p.Velocity.GetComponents();
            double vx = vel[0];
            double vy = vel[1];

            if (x <= 0 || x >= Width) vx = -vx;
            if (y <= 0 || y >= Height) vy = -vy;

            p.SetVelocity(new Vector2D(vx, vy));
        }
        else
        {
            p.MarkExited();
        }
    }

    private void HandleInfections(double dt)
    {
        for (int i = 0; i < _agents.Count; i++)
        {
            for (int j = i + 1; j < _agents.Count; j++)
            {
                var a = _agents[i];
                var b = _agents[j];

                if (a.State == AgentState.Exited || b.State == AgentState.Exited) continue;

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

                    if (_proximityTimers[key] >= InfectTime)
                    {
                        if (a.IsInfected) b.InfectFrom(a);
                        if (b.IsInfected) a.InfectFrom(b);
                    }
                }
                else
                {
                    _proximityTimers.Remove(key);
                }
            }
        }
    }
}