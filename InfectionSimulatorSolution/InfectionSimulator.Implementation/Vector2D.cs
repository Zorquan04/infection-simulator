using InfectionSimulator.Interfaces;

namespace InfectionSimulator.Implementation;

public class Vector2D(double x, double y) : IVector
{
    public double X { get; } = x;
    public double Y { get; } = y;

    public double[] GetComponents() => [X, Y];

    // Długość wektora: √(x^2 + y^2)
    public double Abs() => Math.Sqrt(X * X + Y * Y);

    // Iloczyn skalarny: x1*x2 + y1*y2
    public double Cdot(IVector param)
    {
        var comp = param.GetComponents();
        if (comp == null || comp.Length < 2)
            throw new ArgumentException("Param must provide at least 2 components.", nameof(param));

        return X * comp[0] + Y * comp[1];
    }

    public override string ToString() => $"({X}, {Y})";
}