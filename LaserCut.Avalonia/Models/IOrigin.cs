using MathNet.Numerics.LinearAlgebra.Double;

namespace LaserCut.Avalonia.Models;

public interface IOrigin
{
    Guid Id { get; }
    double X { get; }
    double Y { get; }
    double R { get; }
    
    IObservable<Matrix> MatrixChanged { get; }
    Matrix Transform { get; }
}