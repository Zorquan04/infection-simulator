using InfectionSimulator.Interfaces;

namespace InfectionSimulator.Implementation;

public class Vector3DInheritance(double x, double y, double z) : Vector2D(x, y)
{
    private new double[] GetComponents() => [base.GetComponents()[0], base.GetComponents()[1], z];

    public new double Abs()
    {
        var c = GetComponents();
        return Math.Sqrt(c[0]*c[0] + c[1]*c[1] + c[2]*c[2]);
    }

    public new double Cdot(IVector param)
    {
        var a = GetComponents();
        var b = param.GetComponents();
        return a[0]*b[0] + a[1]*b[1] + a[2]*b[2];
    }

    public Vector3D Cross(IVector param)
    {
        var a = GetComponents();
        var b = param.GetComponents();
        return new Vector3D(
            a[1]*b[2] - a[2]*b[1],
            a[2]*b[0] - a[0]*b[2],
            a[0]*b[1] - a[1]*b[0]
        );
    }

    public override string ToString()
    {
        var c = GetComponents();
        return $"({c[0]}, {c[1]}, {c[2]})";
    }
}