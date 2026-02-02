using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using InfectionSimulator.Models;
using InfectionSimulator.Services;
using InfectionSimulator.Simulation;

namespace InfectionSimulator.Animation;

public partial class MainWindow : Window
{
    private Simulator sim;                  // population simulator instance
    private List<Ellipse> ellipses = new(); // list of visual representations of agents
    private DateTime lastUpdate;            // last animation update time
    private const double FPS = 60.0;        // target frame rate
    private bool isPaused = false;          // is the animation paused

    private double timeScale = 1.0;         // time scale for simulation speed
    private double elapsedTime = 0.0;       // simulation time in seconds
    private const double TotalSimulationTime = 60.0; // 60 seconds of real-time simulation
    private double statsAccumulator = 0;    // to count when to update the bar

    private bool isFast = false;            // is fast simulation mode
    private bool isPostEpidemic;            // is it a post-epidemic scenario
    private double immunityRatio;           // ratio of immune agents at start
    private double infectChance;            // chance of infection at start
    
    public MainWindow()
    {
        InitializeComponent();

        // Show scenario selection window
        var scenarioWindow = new ScenarioWindow(this);
        bool? result = scenarioWindow.ShowDialog();

        if (result != true)
        {
            Close(); // if the user closes the window without making a selection, we close the application
            return;
        }

        // Setting the startup parameters depending on the selection
        isPostEpidemic = scenarioWindow.IsPostEpidemic;

        immunityRatio = isPostEpidemic ? 0.7 : 0.0;
        infectChance = isPostEpidemic ? 0.03 : 0.1;

        InitializeSimulation(); // we initialize and start the simulation

        // Subscribe to the WPF rendering loop
        // This calls OnRendering on each image frame
        CompositionTarget.Rendering += OnRendering;
    }

    private void InitializeSimulation()
    {
        // Start parameters for speed
        timeScale = 1.0;
        isFast = false;
        SpeedButton.Content = "Speed up x2";

        // We are creating a new simulator
        sim = new Simulator(30.0, 30.0, 2.5);
        sim.SeedInitialPopulation(50, immunityRatio, infectChance);

        // We clean the visualization
        ellipses.Clear();
        SimulationCanvas.Children.Clear();

        // Time reset
        elapsedTime = 0.0;
        statsAccumulator = 0.0;

        // Animation state reset
        isPaused = false;
        lastUpdate = DateTime.Now;

        StartStopButton.Content = "Stop";
        StatsTextBlock.Text = "Simulation started...";
    }

    // Resets the simulation with new parameters (the case of ending the previous simulation and starting a new one)
    public void ResetSimulation(bool isPostEpidemic)
    {
        double immunityRatio = isPostEpidemic ? 0.7 : 0.0;
        double infectChance = isPostEpidemic ? 0.03 : 0.1;

        sim = new Simulator(30.0, 30.0, 2.5);
        sim.SeedInitialPopulation(50, immunityRatio, infectChance);

        elapsedTime = 0;
        statsAccumulator = 0;
        isPaused = false;
        StartStopButton.Content = "Stop";

        timeScale = 1.0;
        isFast = false;
        SpeedButton.Content = "Speed up x2";

        lastUpdate = DateTime.Now;

        SimulationCanvas.Children.Clear();
        ellipses.Clear();

        CompositionTarget.Rendering += OnRendering;
    }

    // Called on each rendering frame
    private void OnRendering(object sender, EventArgs e)
    {
        // If the animation is paused, we leave - stop updating
        if (isPaused)
            return;

        // We calculate the time since the last update
        var now = DateTime.Now;
        double deltaMs = (now - lastUpdate).TotalMilliseconds * timeScale;
        double frameTime = 1000.0 / FPS; // time of one frame in ms

        // If the time for the next frame hasn't passed yet, we leave
        if (deltaMs < frameTime)
            return;

        // We are updating the last update time
        lastUpdate = now;

        double dt = deltaMs / 1000.0; // seconds per step

        // we update the simulation time
        elapsedTime += dt;
        statsAccumulator += dt;

        if (elapsedTime >= TotalSimulationTime)
        {
            // we stop the animation
            CompositionTarget.Rendering -= OnRendering;
            CreateSnapshot();

            string path = System.IO.Path.Combine(Environment.CurrentDirectory, "snapshot.json");
            var snapshot = SnapshotManager.LoadSnapshot(path);
            var stats = SimulationStatsCalc.CalculateFromSnapshot(snapshot);

            // show results window
            var resultWindow = new SimulationResultWindow(stats, this)
            {
                Owner = this
            };

            resultWindow.ShowDialog();

            return;
        }

        // We execute a simulation step with deltaTime = 1/FPS
        sim.Step(dt);

        // We add small random changes in direction and speed for each agent
        foreach (var p in sim.Agents)
        {
            if (p.State != AgentState.Exited)
                p.ApplyRandomVelocityPerturbation(2.5, 0.2);
        }

        // We draw all agents on the canvas
        DrawAgents();

        // Update statistics every 1 second
        if (statsAccumulator >= 1.0)
        {
            UpdateStats();
            statsAccumulator = 0;
        }
    }

    private void CreateSnapshot()
    {
        var memos = new List<PersonMemento>();
        foreach (var a in sim.Agents)
            memos.Add(a.CreateMemento());

        string path = System.IO.Path.Combine(Environment.CurrentDirectory, "snapshot.json");
        SnapshotManager.SaveSnapshot(path, memos);
    }

    // Drawing agents on canvas
    private void DrawAgents()
    {
        // Map dimensions in meters
        double mapWidth = 30.0;
        double mapHeight = 30.0;

        // Scaling the map to the canvas dimensions
        double scaleX = SimulationCanvas.ActualWidth / mapWidth;
        double scaleY = SimulationCanvas.ActualHeight / mapHeight;
        double scale = Math.Min(scaleX, scaleY); // we keep the proportions so that the entire map fits

        // We add missing Ellipses if new agents appear
        while (ellipses.Count < sim.Agents.Count)
        {
            Ellipse ellipse = new()
            {
                Width = 6,  // dot width
                Height = 6  // dot height
            };
            SimulationCanvas.Children.Add(ellipse);
            ellipses.Add(ellipse);
        }

        // Update position and color of all agents
        for (int i = 0; i < sim.Agents.Count; i++)
        {
            var p = sim.Agents[i];
            var e = ellipses[i];

            if (p.State == AgentState.Exited)
            {
                // Ukrywamy agentów, którzy opuścili mapę
                e.Visibility = Visibility.Hidden;
            }
            else
            {
                e.Visibility = Visibility.Visible;
                Canvas.SetLeft(e, p.Position.X * scale); // position X on the canvas
                Canvas.SetTop(e, p.Position.Y * scale);  // position Y on the canvas
                e.Fill = GetBrushForPerson(p);           // color depends on health/immunity
            }
        }
    }

    // Returns the appropriate color for the agent
    private Brush GetBrushForPerson(Person p)
    {
        if (p.Immunity == Immunity.Immune)
            return Brushes.Yellow; // resistant
        if (p.Health == HealthState.Infected)
            return Brushes.Red;  // infected
        return Brushes.Green;    // healthy and sensitive
    }

    // TextBlock update with population summary
    private void UpdateStats()
    {
        int healthy = 0, infected = 0, immune = 0, exited = 0;

        foreach (var a in sim.Agents)
        {
            if (a.State == AgentState.Exited) { exited++; continue; }
            if (a.Immunity == Immunity.Immune) immune++;
            if (a.Health == HealthState.Infected) infected++;
            if (a is { Health: HealthState.Healthy, Immunity: Immunity.Susceptible }) healthy++;
        }

        int remaining = sim.Agents.Count - exited;

        StatsTextBlock.Text = $"t={(int)elapsedTime}s: remaining={remaining} total={sim.Agents.Count} healthy={healthy} infected={infected} immune={immune} exited={exited}";
    }

    // Start/Stop button click handler
    private void StartStopButton_Click(object sender, RoutedEventArgs e)
    {
        isPaused = !isPaused;

        if (isPaused)
        {
            StartStopButton.Content = "Start";
        }
        else
        {
            // we reset the clock so that dt is not huge
            lastUpdate = DateTime.Now;
            StartStopButton.Content = "Stop";
        }
    }

    // Restart button click handler
    private void RestartButton_Click(object sender, RoutedEventArgs e)
    {
        InitializeSimulation();
    }

    // Speed toggle button click handler
    private void SpeedButton_Click(object sender, RoutedEventArgs e)
    {
        if (!isFast)
        {
            timeScale = 2.0;
            isFast = true;
            SpeedButton.Content = "Slow down";
        }
        else
        {
            timeScale = 1.0;
            isFast = false;
            SpeedButton.Content = "Speed up x2";
        }
    }
}