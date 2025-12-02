using System.Windows;

namespace InfectionSimulator.Animation;

public partial class ScenarioWindow : Window
{
    public bool IsPostEpidemic { get; private set; } = false;

    public ScenarioWindow()
    {
        InitializeComponent();
    }

    private void NormalDayButton_Click(object sender, RoutedEventArgs e)
    {
        IsPostEpidemic = false;
        this.DialogResult = true; // zamykamy okno i sygnalizujemy wybór
    }

    private void PostEpidemicButton_Click(object sender, RoutedEventArgs e)
    {
        IsPostEpidemic = true;
        this.DialogResult = true; // zamykamy okno i sygnalizujemy wybór
    }
}