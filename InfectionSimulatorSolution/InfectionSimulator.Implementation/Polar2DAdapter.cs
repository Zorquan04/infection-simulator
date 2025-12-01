using InfectionSimulator.Interfaces;

namespace InfectionSimulator.Implementation;

public class Polar2DAdapter(Vector2D vector) : IPolar2D
{
    public double Abs() => vector.Abs();

    // Z użyciem funkcji cyklometrycznej atan2(y, x)
    // zwracamy kąt względem osi OX w radianach
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