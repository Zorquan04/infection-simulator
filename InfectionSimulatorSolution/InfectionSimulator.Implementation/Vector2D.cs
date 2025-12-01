using InfectionSimulator.Interfaces;

namespace InfectionSimulator.Implementation;

public class Vector2D(double x, double y) : IVector
{
    public double[] GetComponents() => [x, y];

    // Długość wektora: √(x^2 + y^2)
    public double Abs() => Math.Sqrt(x * x + y * y);

    // Iloczyn skalarny: x1*x2 + y1*y2
    public double Cdot(IVector param)
    {
        var comp = param.GetComponents();
        return x * comp[0] + y * comp[1];
    }

    public override string ToString() => $"({x}, {y})";
}