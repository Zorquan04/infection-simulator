namespace InfectionSimulator.Interfaces;

public interface IVector
{
    double Abs();                      // długość wektora
    double Cdot(IVector param);        // iloczyn skalarny
    double[] GetComponents();          // zwraca współrzędne jako tablicę
}