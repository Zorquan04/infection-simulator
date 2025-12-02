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
    private Simulator sim;                  // instancja symulatora populacji
    private List<Ellipse> ellipses = new(); // lista wizualnych reprezentacji agentów
    private DateTime lastUpdate;            // czas ostatniej aktualizacji animacji
    private const double FPS = 60.0;        // docelowa liczba klatek na sekundę

    private double elapsedTime = 0.0;       // czas symulacji w sekundach
    private const double TotalSimulationTime = 60.0; // 60 sekund symulacji realnego czasu
    private double statsAccumulator = 0; // do liczenia kiedy aktualizować pasek

    public MainWindow()
    {
        InitializeComponent();

        // Pokaż okno wyboru scenariusza
        var scenarioWindow = new ScenarioWindow();
        bool? result = scenarioWindow.ShowDialog();

        if (result != true)
        {
            Close(); // jeśli użytkownik zamknął okno bez wyboru, zamykamy aplikację
            return;
        }

        // Ustawienie parametrów startowych w zależności od wyboru
        double immunityRatio = scenarioWindow.IsPostEpidemic ? 0.7 : 0.0;
        double infectChance = scenarioWindow.IsPostEpidemic ? 0.03 : 0.1;

        // Tworzymy symulator: wymiary mapy w metrach i maksymalna prędkość agentów
        sim = new Simulator(30.0, 30.0, 2.5);

        // Tworzymy początkową populację 50 osób
        // Bez odporności (0.0) i 10% szansą bycia zakażonym
        sim.SeedInitialPopulation(50, immunityRatio, infectChance);

        // Zapamiętujemy czas startowy
        lastUpdate = DateTime.Now;

        // Subskrybujemy do pętli renderowania WPF
        // To wywołuje OnRendering przy każdej klatce obrazu
        CompositionTarget.Rendering += OnRendering;
    }

    // Wywoływana przy każdej klatce renderowania
    private void OnRendering(object sender, EventArgs e)
    {
        // Obliczamy czas od ostatniej aktualizacji
        var now = DateTime.Now;
        double deltaMs = (now - lastUpdate).TotalMilliseconds;
        double frameTime = 1000.0 / FPS; // czas jednej klatki w ms

        // Jeśli jeszcze nie minął czas na kolejną klatkę, wychodzimy
        if (deltaMs < frameTime)
            return;

        // Aktualizujemy czas ostatniej aktualizacji
        lastUpdate = now;

        double dt = deltaMs / 1000.0; // sekundy na krok

        // aktualizujemy czas symulacji
        elapsedTime += dt;
        statsAccumulator += dt;

        if (elapsedTime >= TotalSimulationTime)
        {
            // zatrzymujemy animację
            CompositionTarget.Rendering -= OnRendering;
            CreateSnapshot();
            MessageBox.Show("Symulacja zakończona po 60 sekundach. Utworzono Snapshot.");
            return;
        }

        // Wykonujemy krok symulacji z deltaTime = 1/FPS
        sim.Step(dt);

        // Dodajemy drobne losowe zmiany kierunku i prędkości dla każdego agenta
        foreach (var p in sim.Agents)
        {
            if (p.State != AgentState.Exited)
                p.ApplyRandomVelocityPerturbation(2.5, 0.2);
        }

        // Rysujemy wszystkich agentów na canvasie
        DrawAgents();

        // Aktualizacja statystyk co 1 sekundę
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

    // Rysowanie agentów na canvasie
    private void DrawAgents()
    {
        // Wymiary mapy w metrach
        double mapWidth = 30.0;
        double mapHeight = 30.0;

        // Skalowanie mapy do wymiarów canvasu
        double scaleX = SimulationCanvas.ActualWidth / mapWidth;
        double scaleY = SimulationCanvas.ActualHeight / mapHeight;
        double scale = Math.Min(scaleX, scaleY); // zachowujemy proporcje, aby cała mapa się zmieściła

        // Dodajemy brakujące Ellipse jeśli pojawili się nowi agenci
        while (ellipses.Count < sim.Agents.Count)
        {
            Ellipse ellipse = new()
            {
                Width = 6,  // szerokość kropki
                Height = 6  // wysokość kropki
            };
            SimulationCanvas.Children.Add(ellipse);
            ellipses.Add(ellipse);
        }

        // Aktualizacja pozycji i koloru wszystkich agentów
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
                Canvas.SetLeft(e, p.Position.X * scale); // pozycja X na canvasie
                Canvas.SetTop(e, p.Position.Y * scale);  // pozycja Y na canvasie
                e.Fill = GetBrushForPerson(p);           // kolor zależny od stanu zdrowia/odporności
            }
        }
    }

    // Zwraca odpowiedni kolor dla agenta
    private Brush GetBrushForPerson(Person p)
    {
        if (p.Immunity == Immunity.Immune)
            return Brushes.Yellow; // odporny
        if (p.Health == HealthState.Infected)
            return Brushes.Red;  // zakażony
        return Brushes.Green;    // zdrowy i wrażliwy
    }

    // Aktualizacja TextBlocka z podsumowaniem populacji
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
}