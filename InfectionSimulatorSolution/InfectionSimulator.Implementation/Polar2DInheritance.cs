using InfectionSimulator.Interfaces;

namespace InfectionSimulator.Implementation;

public class Polar2DInheritance(double x, double y) : Vector2D(x, y), IPolar2D
{
    public double GetAngle()
    {
        var comp = GetComponents();
        return Math.Atan2(comp[1], comp[0]);
    }
}