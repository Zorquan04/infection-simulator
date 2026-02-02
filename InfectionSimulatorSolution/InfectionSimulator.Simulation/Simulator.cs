using InfectionSimulator.Models;
using InfectionSimulator.Implementation;

namespace InfectionSimulator.Simulation;

public class Simulator(double width, double height, double maxSpeed)
{
    public double Width { get; } = width;   // width of the simulation field
    public double Height { get; } = height; // height of the simulation field

    private readonly Random _rng = new();          // random generator
    private readonly List<Person> _agents = new(); // list of all agents
    private int _nextId = 1;                       // ID of the next agent

    private const double InfectDistance = 2.0;     // distance at which infection can occur
    private const double InfectTime = 3.0;         // contact time required for infection

    private readonly Dictionary<(int, int), double> _proximityTimers = new(); // contact time between pairs of agents

    private readonly int _targetPopulation = 50;   // target population size

    public IReadOnlyList<Person> Agents => _agents;

    // seeding the starting population
    public void SeedInitialPopulation(int count, double immunityRatio, double infectionChanceForSusceptible)
    {
        for (int i = 0; i < count; i++)
        {
            bool immune = _rng.NextDouble() < immunityRatio;    // resistance draw
            SpawnPerson(immune, infectionChanceForSusceptible); // create agent
        }
    }

    // create a new agent
    private void SpawnPerson(bool immune, double infectionChance)
    {
        // random spawn at the edge of the field
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

        // direction of movement towards the center of the field
        double cx = Width / 2.0;
        double cy = Height / 2.0;
        double angle = Math.Atan2(cy - y, cx - x);

        // random speed (0.2 -> maxSpeed)
        double speed = 0.2 + _rng.NextDouble() * (maxSpeed - 0.2);
        var vel = new Vector2D(Math.Cos(angle) * speed, Math.Sin(angle) * speed);

        // Is the agent infected at the start?
        bool infected = !immune && _rng.NextDouble() < infectionChance;

        var p = new Person(_nextId++, pos, vel, immune, infected);
        _agents.Add(p);
    }

    // performing one simulation step
    public void Step(double dt = 0.04)
    {
        foreach (var p in _agents.ToList())
        {
            if (p.State != AgentState.Exited)
                p.Update(dt); // position and status update

            HandleBorders(p); // checking for collisions with boundaries
        }

        MaintainPopulation();  // keeping the population at a constant level
        HandleInfections(dt);  // infection management
    }

    // keeping an eye on the population size
    private void MaintainPopulation()
    {
        int alive = _agents.Count(a => a.State != AgentState.Exited);
        while (alive < _targetPopulation)
        {
            SpawnPerson(immune: false, infectionChance: 0.1);
            alive++;
        }
    }

    // handling collisions with field boundaries
    private void HandleBorders(Person p)
    {
        if (p.State == AgentState.Exited) return;

        var pos = p.Position.GetComponents();
        double x = pos[0];
        double y = pos[1];

        bool hit = x <= 0 || x >= Width || y <= 0 || y >= Height;
        if (!hit) return;

        if (_rng.NextDouble() < 0.5) // reflection from the wall
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
            p.MarkExited(); // the agent leaves the field
        }
    }

    // cross-agent infection handling
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

                    // if contact time is sufficient -> infection
                    if (_proximityTimers[key] >= InfectTime)
                    {
                        if (a.IsInfected) b.InfectFrom(a);
                        if (b.IsInfected) a.InfectFrom(b);
                    }
                }
                else
                {
                    _proximityTimers.Remove(key); // reset timer if too far
                }
            }
        }
    }
}