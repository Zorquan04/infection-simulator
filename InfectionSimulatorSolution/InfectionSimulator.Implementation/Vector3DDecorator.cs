using InfectionSimulator.Interfaces;

namespace InfectionSimulator.Implementation;

public class Vector3DDecorator(IVector srcVector, double z) : IVector
{
    public double[] GetComponents()
    {
        var baseComp = srcVector.GetComponents();
        return [baseComp[0], baseComp[1], z];
    }

    public double Abs()
    {
        var c = GetComponents();
        return Math.Sqrt(c[0] * c[0] + c[1] * c[1] + c[2] * c[2]);
    }

    // Iloczyn skalarny 3D
    public double Cdot(IVector param)
    {
        var a = GetComponents();
        var b = param.GetComponents();
        return a[0]*b[0] + a[1]*b[1] + a[2]*b[2];
    }

    // Iloczyn wektorowy (cross product)
    public Vector3D Cross(IVector param)
    {
        var a = GetComponents();
        var b = param.GetComponents();
        return new Vector3D(
            a[1] * b[2] - a[2] * b[1],
            a[2] * b[0] - a[0] * b[2],
            a[0] * b[1] - a[1] * b[0]
        );
    }

    public override string ToString()
    {
        var c = GetComponents();
        return $"({c[0]}, {c[1]}, {c[2]})";
    }
}

// Pomocnicza klasa Vector3D do przechowywania wyniku cross product
public class Vector3D(double x, double y, double z)
{
    public readonly double X = x, Y = y, Z = z;

    public override string ToString() => $"({X}, {Y}, {Z})";
}