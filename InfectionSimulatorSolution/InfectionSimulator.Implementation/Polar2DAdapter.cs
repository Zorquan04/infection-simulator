using InfectionSimulator.Interfaces;

namespace InfectionSimulator.Implementation;

public class Polar2DAdapter(Vector2D vector) : IPolar2D
{
    public double Abs() => vector.Abs();

    // Using the cyclometric function atan2(y, x)
    // we return the angle relative to the x-axis in radians
    public double GetAngle()
    {
        var comp = vector.GetComponents();
        return Math.Atan2(comp[1], comp[0]);
    }

    public override string ToString()
    {
        return $"r = {Abs():F2}, θ = {GetAngle():F2} rad";
    }
}