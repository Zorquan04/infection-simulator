using InfectionSimulator.Models;
using System.Windows;

namespace InfectionSimulator.Animation;

public partial class SimulationResultWindow : Window
{
    private readonly MainWindow _mainWindow;
    private bool isOkClicked = false;

    public SimulationResultWindow(SimulationStats stats, MainWindow mainWindow)
    {
        InitializeComponent();
        DataContext = stats;
        _mainWindow = mainWindow;

        Closing += SimulationResultWindow_Closing;
    }

    private void Close_Click(object sender, RoutedEventArgs e)
    {
        isOkClicked = true;
        Close();

        // We open the scenario selection window again
        var scenarioWindow = new ScenarioWindow(_mainWindow);
        bool? result = scenarioWindow.ShowDialog();

        if (result == true)
        {
            _mainWindow.ResetSimulation(scenarioWindow.IsPostEpidemic);
        }
    }

    // window closing support
    private void SimulationResultWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        // if we don't want to go back to the menu, we close the main window
        if (!isOkClicked)
        {
            _mainWindow.Close();
        }
    }
}