using System.Windows;

namespace InfectionSimulator.Animation;

public partial class ScenarioWindow : Window
{
    public bool IsPostEpidemic { get; private set; } = false;
    private readonly MainWindow _mainWindow;

    public ScenarioWindow(MainWindow mainWindow)
    {
        InitializeComponent();
        _mainWindow = mainWindow;

        Closing += ScenarioWindow_Closing;
    }

    private void NormalDayButton_Click(object sender, RoutedEventArgs e)
    {
        IsPostEpidemic = false;
        DialogResult = true; //we close the window and signal the selection
    }

    private void PostEpidemicButton_Click(object sender, RoutedEventArgs e)
    {
        IsPostEpidemic = true;
        DialogResult = true; // we close the window and signal the selection
    }

    // window closing support
    private void ScenarioWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        // if the user clicked X, we close the entire application
        if (!DialogResult.HasValue)
        {
            _mainWindow.Close();
        }
    }
}