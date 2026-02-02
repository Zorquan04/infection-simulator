namespace InfectionSimulator.Interfaces;

public interface IVector
{
    double Abs();                      // vector length
    double Cdot(IVector param);        // dot product
    double[] GetComponents();          // returns coordinates as an array
}