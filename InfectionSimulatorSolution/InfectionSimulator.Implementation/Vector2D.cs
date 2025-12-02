using InfectionSimulator.Interfaces;

namespace InfectionSimulator.Implementation;

public class Vector2D(double x, double y) : IVector
{
    public double X { get; } = x;
    public double Y { get; } = y;

    public double[] GetComponents() => [X, Y];

    // długość wektora
    public double Length() => Math.Sqrt(X * X + Y * Y);

    // znormalizowany wektor
    public Vector2D Normalized()
    {
        double len = Length();
        return len > 0 ? new Vector2D(X / len, Y / len) : new Vector2D(0, 0);
    }
    
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

    // mnożenie przez skalar
    public static Vector2D operator *(Vector2D v, double scalar) => new Vector2D(v.X * scalar, v.Y * scalar);
    public override string ToString() => $"({X}, {Y})";
}